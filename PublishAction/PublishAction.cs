/* *********************************************************************** *
 * File   : PublishAction.cs                              Part of Sitecore *
 * Version: 1.00                                          www.sitecore.net *
 * Author : Sitecore A/S                                                   *
 *                                                                         *
 * Purpose: To implement simple publish action to defined targets          *
 *                                                                         *
 * Bugs   : None known.                                                    *
 *                                                                         *
 * Status : Published.                                                     *
 *                                                                         *
 * Copyright (C) 1999-2004 by Sitecore A/S. All rights reserved.           *
 *                                                                         *
 * This work is the property of:                                           *
 *                                                                         *
 *        Sitecore A/S                                                     *
 *        Meldahlsgade 5, 4.                                               *
 *        DK-1613 Copenhagen V.                                            *
 *        Denmark                                                          *
 *                                                                         *
 * This is a Sitecore published work under Sitecore's                      *
 * shared source license.                                                  *
 *                                                                         *
 * *********************************************************************** */

using System;
using System.Collections;
using System.Collections.Specialized; 

using Sitecore.SecurityModel;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Publishing;
using Sitecore.Web;

namespace Sitecore.Workflows.Simple
{
	public class PublishAction
	{
      public void Process(WorkflowPipelineArgs args)
      {
         // Get item 
         Item item = args.DataItem; 
         // Get action item
         Item paramItem = args.ProcessorItem.InnerItem; 
         
         // retrieving the publish tarets' IDs
         string targets = string.Empty;
         if (paramItem != null && paramItem["Targets"] != null)
         {
            targets = paramItem["Targets"];
         }
         
         // Get list of databases where you should publish
         Database[] publishDatabaseArray = GetPublishDatabases(targets);
         //Publish
         for(int i = 0; i < publishDatabaseArray.Length; i++)
         {
            Database database = publishDatabaseArray[i];
            PublishOptions options = new PublishOptions(PublishMode.SingleItem, item.Language, DateTime.Now);
            options.RootItem = item;
            options.Deep = IsDeepPublish(item);
            options.SourceDatabase = item.Database;
            options.TargetDatabase = database;
            Publisher publisher = new Publisher(options);
            publisher.PublishAsync();
         }
      }

      #region Private Methods

      private Database[] GetPublishDatabases(string publishTargets)
      {
         using (new SecurityDisabler())
         {
            // biuld the arary of publish targets IDs
            string[] targetIDs = publishTargets.Split('|');
            ArrayList list = new ArrayList();

            // selecting all the target databases to publish item to
            foreach (string targetID in targetIDs)
            {
               if (targetID != null && targetID != string.Empty)
               {
                  Item publishTargetItem = Sitecore.Context.ContentDatabase.Items[targetID];
                  
                  // checking the template correspondence
                  if (publishTargetItem.Template.Name == "Publishing target")
                  {
                     string targetDatabaseName = publishTargetItem["Target database"];
                     if(targetDatabaseName.Length > 0)
                     {
                        // Get database from name
                        Database database = Factory.GetDatabase(targetDatabaseName, false);
                        if(database!=null)
                        {
                           list.Add(database);
                           continue;
                        }
                        // Log warning (database cannot be null)
                        Log.Warn(string.Format("Unknown database in PublishAction: {0}",targetDatabaseName), this);
                     }
                  }
               }
            }
            return (list.ToArray(typeof(Database)) as Database[]);
         }
      }
      
      private bool IsDeepPublish(Item item)
      {
         if (item["deep"] == "1")
         {
            return true;
         }
         // if parameter is not defined in action's field serch paramater in item["parameters"]
         NameValueCollection collection = WebUtil.ParseUrlParameters(item["parameters"]);
         if ((collection != null) && (collection["deep"] == "1"))
         {
            return true;
         }
         return false;
      }

      #endregion
	}
}
