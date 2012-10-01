/* *********************************************************************** *
 * File   : MailConfigurator.cs                           Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents default mail rendering logic                        *
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
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace Sitecore.Modules.WorkflowEscalator
{    
    public class DefaultMailConfigurator:IMailConfigurator
    {
        protected string senderName;
        
        public  DefaultMailConfigurator(){}
        
        public DefaultMailConfigurator(string senderName)        
        {
            this.senderName = senderName;
        }

        #region IMailConfigurator Members

        public virtual string GetMailSubject(string subjectPattern)
        {
            return subjectPattern;
        }

        public virtual string GetMailHeader(string headerPattern)
        {
            return headerPattern;
        }

        public virtual string GetMailBody(string itemPattern, IEnumerable<Item> items)
        {            
            StringBuilder mailBody = new StringBuilder();
            var itemsGroup = items.GroupBy(
                i =>
                string.Format("{0}/{1}", i.State.GetWorkflow().Appearance.DisplayName,
                              i.State.GetWorkflowState().DisplayName));
            
            foreach (var group in itemsGroup)
            {
                mailBody.AppendLine();
                mailBody.AppendLine(string.Concat(group.Key,":"));
                foreach (var item in group)
                {
                    mailBody.AppendLine(itemPattern.Replace("{itempath}", item.Paths.ContentPath)
                   .Replace("{database}", item.Database.Name)
                   .Replace("{version}", item.Version.ToString())
                   .Replace("{language}", item.Language.Name));     
                } 
                             
            }

            return mailBody.ToString();
        }

        public virtual string GetMailFooter(string footerPattern)
        {
            return footerPattern.Replace("{sender}", senderName ?? string.Empty);
        }

        #endregion
    }
}
