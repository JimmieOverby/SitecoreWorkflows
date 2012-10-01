/* *********************************************************************** *
 * File   : WorkflowHelper.cs                             Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Helper for workflow routines                                   *
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
using Sitecore.Workflows;

namespace Sitecore.Modules.WorkflowEscalator
{
    public static class WorkflowHelper
    {
        #region Properties, constants and variables

        //        Database name to invoke operations on
        private const string DbName = CommonSettings.DbName;

        #endregion

        #region Public methods

        //        Returns all workflows in a database
        public static IEnumerable<IWorkflow> GetAllWorkflows()
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Database masterDb = Factory.GetDatabase(DbName);
                return masterDb.WorkflowProvider.GetWorkflows();
            }
        }

        //        Returns all items in a specific state of a specific workflow  
        public static IEnumerable<Item> GetItemsInState(IWorkflow workflow, string stateId)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Database masterDb = Factory.GetDatabase(DbName);
                List<Item> itemsInSate = new List<Item>();

                var itemsDataUri = workflow.GetItems(stateId);
                foreach (var uri in itemsDataUri)
                {
                    var item = masterDb.GetItem(uri);
                    if (item != null)
                    {
                        itemsInSate.Add(item);
                    }
                }

                return itemsInSate;
            }
        }

        //        Returns all none-final states based on a specific template of a specific workflow 
        public static IEnumerable<WorkflowState> GetNonFinalStatesByTemplate(IWorkflow workflow, string templateName)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Database masterDb = Factory.GetDatabase(DbName);
                var allStates = workflow.GetStates();
                return
                    allStates.Where(
                        s => !s.FinalState &&
                             string.Equals(masterDb.GetItem(new ID(s.StateID)).TemplateName, templateName,
                                           StringComparison.InvariantCultureIgnoreCase));
            }
        }

        //        Returns state's specific field value or null if absent 
        public static string GetStateFieldValue(WorkflowState state, string fieldName)
        {
            Field field = GetStateField(state, fieldName);
            return field != null ? field.Value : null;
        }

        //        Returns state's specific field or null if absent
        public static Field GetStateField(WorkflowState state, string fieldName)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Database masterDb = Factory.GetDatabase(DbName);
                Item item = masterDb.GetItem(new ID(state.StateID));
                return item != null ? item.Fields[fieldName] : null;
            }
        }

//        Defines if an item has exceeded its staying in workflow state time limit  
        public static bool IsTimeLimitExceeded(Item item, IWorkflow workflow, TimeSpan maxTimeInState)
        {
            var events = workflow.GetHistory(item);
            return events.Length > 0 && (DateTime.Now - events[events.Length - 1].Date) > maxTimeInState;
        }

        #endregion
    }
}