/* *********************************************************************** *
 * File   : WorkflowEscalatorAgent.cs                     Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Sending email using module specific settings                   *
 *                                                                         *
 * Bugs   : None known.                                                    *
 *                                                                         *
 * Status : Published.                                                     *
 *                                                                         *
 * Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.           *
 *                                                                         *
 * This work is the property of:                                           *
 *                                                                         *
 *        Sitecore A/S                                                     *
 *        Meldahlsgade 5, 4.                                               *
 *        1613 Copenhagen V.                                               *
 *        Denmark                                                          *
 *                                                                         *
 * This is a Sitecore published work under Sitecore's                      *
 * shared source license.                                                  *
 *                                                                         *
 * *********************************************************************** */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Modules.WorkflowEscalator
{
    class EmailUtility
    {
        #region Properties, constants and variables
        private const string ItemDescField = "Item description";
        private const string DbName = CommonSettings.DbName;
        private const string DefaultSettingsPath = "/Sitecore/System/Modules/Workflow escalator";
        private const string FooterField = "Footer";
        private const string HeaderField = "Header";
        private const string PortField = "Port";
        private const string SubjectField = "Subject";
        private const string SenderEmailField = "Email Address";
        private const string SenderNameField = "Display Name";
        private const string ServerField = "Server";
        private const string UserNameField = "User Name";
        private const string PasswordFiled = "Password";
        
        private readonly string mailBody;
        private readonly string mailFooter;
        private readonly string mailHeader;
        private readonly string mailSubject;
        private readonly string senderAddress;
        private readonly string senderDisplayName;
        private readonly string smtpServer;
        private readonly string smtpPort;
        private readonly string userName;
        private readonly string userPassword;

        private IMailConfigurator mailConfigurator;
        public IMailConfigurator MailConfigurator
        {
            set { this.mailConfigurator = value; }
        }
        #endregion

        #region Public methods
        public EmailUtility()
            : this(DefaultSettingsPath)
        {
        }

        public EmailUtility(string settingsPath)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Database masterDb = Factory.GetDatabase(DbName);
                Item settings = masterDb.GetItem(settingsPath);
                if (settings != null)
                {
                    this.senderAddress = string.IsNullOrEmpty(settings[SenderEmailField])
                                        ? Settings.GetAppSetting("EmailReminder.FromAddress")
                                        : settings[SenderEmailField].Trim();

                    this.senderDisplayName = settings[SenderNameField];

                    this.smtpServer = string.IsNullOrEmpty(settings[ServerField])
                                        ? Settings.MailServer
                                        : settings[ServerField].Trim();

                    //NOTE: Do we need to get default value from web.config
                    this.smtpPort = settings[PortField];
                    if (!string.IsNullOrEmpty(this.smtpPort))
                    {
                        this.smtpPort = this.smtpPort.Trim();
                    }

                    //NOTE: Do we need to get default value from web.config
                    this.userName = settings[UserNameField];

                    //NOTE: Do we need to get default value from web.config
                    this.userPassword = settings[PasswordFiled];

                    this.mailSubject = string.IsNullOrEmpty(settings[SubjectField])
                                   ? Settings.Tasks.EmailReminderSubject
                                   : settings[SubjectField];
                    
                    this.mailBody = string.IsNullOrEmpty(settings[ItemDescField])
                                   ? Settings.Tasks.EmailReminderText
                                   : settings[ItemDescField];

                    this.mailHeader = settings[HeaderField];
                    this.mailFooter = settings[FooterField];                    
                }
                else
                {
                    this.senderAddress = Settings.GetAppSetting("EmailReminder.FromAddress");
                    this.smtpServer = Settings.MailServer;
                    this.smtpPort = Settings.MailServerPort.ToString();
                    this.userName = Settings.MailServerUserName;
                    this.userPassword = Settings.MailServerPassword;
                    this.mailSubject = Settings.Tasks.EmailReminderSubject;
                    this.mailBody = Settings.Tasks.EmailReminderText;
                }
                
                Assert.IsNotNullOrEmpty(this.senderAddress, "Sender isn't specified");
                Assert.IsNotNullOrEmpty(this.smtpServer, "SMTP server name isn't specified");
                Assert.IsNotNullOrEmpty(this.mailSubject, "Mail body isn't specified");
                Assert.IsNotNullOrEmpty(this.mailBody, "Mail subject isn't specified");

                this.mailConfigurator = new DefaultMailConfigurator(this.senderDisplayName);
            }

        }
     
//        Sends notification email to a recipient about all items he's assigned to 
        public void SendNotificationEmail(IEnumerable<Item> items, string recipient)
        {
            SmtpClient smtpClient = GetSmtpClient();
            string currentSite = Sitecore.Context.Site.Name;
            Sitecore.Context.SetActiveSite("shell");
            try
            {               
                using (MailMessage msg = new MailMessage())
                {
                    PopulateMessage(msg, items);

                    MailAddress sender = !string.IsNullOrEmpty(this.senderDisplayName)
                                             ? new MailAddress(this.senderAddress, this.senderDisplayName)
                                             : new MailAddress(this.senderAddress);

                    msg.From = sender;

                    msg.To.Add(new MailAddress(recipient));

                    Logger.Info(
                        string.Format("Sending notification email TO: {0}", recipient), this);
                    smtpClient.Send(msg);
                }
            }
            finally
            {
                Sitecore.Context.SetActiveSite(currentSite);
            }
        }

        #endregion

        #region Private methods
        //        Returns the configured SMTP client 
        private SmtpClient GetSmtpClient()
        {
            SmtpClient smtpClient = new SmtpClient(this.smtpServer);
            if (!string.IsNullOrEmpty(this.smtpPort))
            {
                smtpClient.Port = int.Parse(this.smtpPort);
            }

            if (!string.IsNullOrEmpty(this.userName))
            {
                NetworkCredential credential = new NetworkCredential(this.userName, this.userPassword);
                smtpClient.Credentials = credential;
            }

            return smtpClient;
        }

        //Populates mail subject and body
        private void PopulateMessage(MailMessage msg, IEnumerable<Item> items)
        {
            msg.Subject = this.mailConfigurator.GetMailSubject(this.mailSubject);
            msg.SubjectEncoding = Encoding.UTF8;

            StringBuilder msgBody = new StringBuilder();

            if (!string.IsNullOrEmpty(this.mailHeader))
            {
                msgBody.AppendLine(this.mailConfigurator.GetMailHeader(this.mailHeader));
            }

            msgBody.Append(this.mailConfigurator.GetMailBody(this.mailBody, items));

            if (!string.IsNullOrEmpty(this.mailFooter))
            {
                msgBody.AppendLine();
                msgBody.Append(this.mailConfigurator.GetMailFooter(this.mailFooter));
            }
            msg.Body = msgBody.ToString();

            msg.IsBodyHtml = false;
            msg.BodyEncoding = Encoding.UTF8;
        }
        #endregion
    }
}
