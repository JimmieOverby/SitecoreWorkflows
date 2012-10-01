/* *********************************************************************** *
 * File   : ContentEditorWarnings.cs                      Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Displaying warnings in Content Editor                          *
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
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Pipelines.GetContentEditorWarnings;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Applications.Security.AccountSelector;

namespace Sitecore.Modules.WorkflowEscalator
{
    public class ContentEditorWarnings
    {
        #region Properties, constants and variables

        private const string DbName = CommonSettings.DbName;
        private const string RecipientsFieldName = CommonSettings.RecipientsFieldName;
        private const string TemplateName = CommonSettings.StateTemplateName;
        private const string TimeFieldName = CommonSettings.TimeFieldName;
        //Key in the dictionary for localized string
        private const string WarningTitleKey = "WarningEscalatorTitle";
        //Key in the dictionary for localized string
        private const string WarningTextKey = "WarningEscalatorText";

        #endregion

        #region Public methods

        public void Process(GetContentEditorWarningsArgs args)
        {
            Item currentItem = args.Item;
            if (currentItem == null) return;

            if (!Sitecore.Context.User.IsAuthenticated) return;

            var state = currentItem.State.GetWorkflowState();
            if (state != null && !state.FinalState)
            {
                using (new SecurityModel.SecurityDisabler())
                {
                    Database masterDb = Factory.GetDatabase(DbName);
                    Item stateItem = masterDb.GetItem(state.StateID);
                    if (stateItem != null &&
                        TemplateName.Equals(stateItem.TemplateName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string timeFrameString = stateItem[TimeFieldName];
                        if (string.IsNullOrEmpty(timeFrameString)) return;

                        TimeSpan timeFrame = DateUtil.ParseTimeSpan(timeFrameString, TimeSpan.Zero);
                        if (timeFrame == TimeSpan.Zero) return;
                       
                        if (IsInSelectedList(stateItem.Fields[RecipientsFieldName]) &&
                            WorkflowHelper.IsTimeLimitExceeded(currentItem, currentItem.State.GetWorkflow(),
                                                               timeFrame))
                        {
                            AddWarning(args);
                        }

                    }

                }
            }
        }
       
        #endregion

        #region Protected Methods

        //        Adds warning message to Content Editor and specifies its details
        protected virtual void AddWarning(GetContentEditorWarningsArgs args)
        {
            var warning = args.Add();
            warning.Title = Globalization.Translate.Text(WarningTitleKey);
            warning.Text = Globalization.Translate.Text(WarningTextKey);           
        }

        //Derermines if current user is in one of the selected users (or is in a selected role)
        protected bool IsInSelectedList(Field recipientsField)
        {
            AccountSelectorField accountSelectorField = new AccountSelectorField(recipientsField);

            List<Account> selectedRoles = accountSelectorField.GetSelectedAccountsByType(AccountType.Role);
            List<Account> selectedUsers = accountSelectorField.GetSelectedAccountsByType(AccountType.User);

            if ((selectedUsers.Count + selectedRoles.Count) == 0) return false;

            User currentUser = Context.User;
            if (
                selectedUsers.Any(
                    user => string.Equals(user.Name, currentUser.Name, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            if (selectedRoles.Any(role => currentUser.IsInRole(role.Name))) return true;

            return false;
        }

        #endregion
    }
}