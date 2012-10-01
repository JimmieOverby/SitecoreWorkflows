/* *********************************************************************** *
 * File   : WorkflowEscalatorAgent.cs                     Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents the Scheduled agent                                 *
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
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Applications.Security.AccountSelector;
using Sitecore.Tasks;

namespace Sitecore.Modules.WorkflowEscalator
{
    public sealed class WorkflowEscalatorAgent
    {
        #region Properties, constants and variables

        // the field containing notification recipients
        private const string RecipientsFieldName = CommonSettings.RecipientsFieldName;
        // the template name, workflow state must use to enable notification 
        private const string StateTemplateName = CommonSettings.StateTemplateName;
        // the field containing time interval during which item will stay in a workflow state without sending a notification
        private const string TimeFiledName = CommonSettings.TimeFieldName;

        #endregion

        #region Public methods

//        The entry point of the agent
        public void Run(Item[] itemArray, CommandItem commandItem, ScheduleItem scheduledItem)
        {
            Logger.Info(string.Format("Started on {0}", DateTime.Now), this);
//          Association between users and items:key - user email, value - items to include in notification
            Dictionary<string, List<Item>> recipientItemsDictionary = new Dictionary<string, List<Item>>();
            try
            {
                EmailUtility emailUtility = new EmailUtility();
                var allWorkflows = WorkflowHelper.GetAllWorkflows();
                foreach (var workflow in allWorkflows)
                {
                    var worklowStates = WorkflowHelper.GetNonFinalStatesByTemplate(workflow, StateTemplateName);
                    foreach (var state in worklowStates)
                    {
                        string timeFrameString = WorkflowHelper.GetStateFieldValue(state, TimeFiledName);
                        if (string.IsNullOrEmpty(timeFrameString))
                        {
                            Logger.Warn(
                                string.Format("{0} field isn't specified for state {1}", TimeFiledName,
                                              state.DisplayName), this);
                            continue;
                        }

                        TimeSpan timeFrame = DateUtil.ParseTimeSpan(timeFrameString, TimeSpan.Zero);
                        if (timeFrame == TimeSpan.Zero)
                        {
                            Logger.Warn(
                                string.Format("{0} field isn't well formatted for state {1}", TimeFiledName,
                                              state.DisplayName), this);
                            continue;
                        }

                        Field recipientsField = WorkflowHelper.GetStateField(state, RecipientsFieldName);
                        if (recipientsField == null || string.IsNullOrEmpty(recipientsField.Value))
                        {
                            Logger.Warn(
                                string.Format("{0} field isn't specified for state {1}", RecipientsFieldName,
                                              state.DisplayName), this);
                            continue;
                        }

                        List<string> recepients = GetEmailsForUsersAndRoles(recipientsField);
                        if (recepients.Count == 0)
                        {
                            Logger.Warn(
                                string.Format("There's no users with valid email addresses to notify for state {0}",
                                              state.DisplayName), this);
                            continue;
                        }

                        var itemsInState = WorkflowHelper.GetItemsInState(workflow, state.StateID);
                        foreach (var item in  itemsInState)
                        {
                            if (WorkflowHelper.IsTimeLimitExceeded(item, workflow, timeFrame))
                            {
                                AddToResultSet(item, recipientItemsDictionary, recepients.ToArray());
                            }
                        }
                    }
                }

                ProcessResultSet(recipientItemsDictionary, emailUtility);
            }
            catch (Exception exc)
            {
                Logger.Error("Unspecified error ocurred", exc, this);
            }
            finally
            {
                Logger.Info("Stopped", this);
            }
        }

        #endregion

        #region Private methods

//        Adds the item to the items list of each recipient
        private void AddToResultSet(Item item, Dictionary<string, List<Item>> set, string[] recepients)
        {
            foreach (var recepient in recepients)
            {
                if (!set.ContainsKey(recepient))
                {
                    set.Add(recepient, new List<Item>());
                }

                set[recepient].Add(item);
            }
        }

        //Returns unique email addresses of users that correspond to the selected list of users/roles  
        private List<string> GetEmailsForUsersAndRoles(Field field)
        {
            List<string> emails = new List<string>();

            AccountSelectorField accountSelectorField = new AccountSelectorField(field);
            List<Account> selectedRoles = accountSelectorField.GetSelectedAccountsByType(AccountType.Role);
            List<Account> selectedUsers = accountSelectorField.GetSelectedAccountsByType(AccountType.User);            
            //The shell site has Sitecore Domain
            var allUsers = Sitecore.Context.Domain.GetUsers();           
            //var allUsers = Sitecore.SecurityModel.DomainManager.GetDomain("Sitecore").GetUsers();
            foreach (var user in allUsers)
            {
                string userEmail = user.Profile.Email;
                if (string.IsNullOrEmpty(userEmail)) continue;
                //if current user is in selected role add his email and check next user
                if (selectedRoles.Any(role => user.IsInRole(role.Name)))
                {
                    if (!emails.Contains(userEmail))
                    {
                        emails.Add(userEmail);
                    }
                    continue;
                }
                //if current user is one of the selected one add his email
                if (selectedUsers.Any(u => string.Equals(u.Name, user.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!emails.Contains(userEmail))
                    {
                        emails.Add(userEmail);
                    }
                }
            }

            return emails;
        }

        private void ProcessResultSet(Dictionary<string, List<Item>> set, EmailUtility emailUtility)
        {
            foreach (var recipient in set.Keys)
            {
                try
                {
                    emailUtility.SendNotificationEmail(set[recipient], recipient);
                }
                catch (Exception exc)
                {
                    Logger.Error(string.Format("Sending notification to {0} failed", recipient), exc, this);
                }
            }
        }

        #endregion
    }
}