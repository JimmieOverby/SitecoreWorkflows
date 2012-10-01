/* *********************************************************************** *
 * File   : CreateFromMasterAction.cs                     Part of Sitecore *
 * Version: 1.00                                          www.sitecore.net *
 * Author : Sitecore A/S                                                   *
 *                                                                         *
 * Purpose: To implement create item workflow action                       *
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
using System.IO;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Workflows;
using Sitecore.Workflows.Simple;
using Sitecore.SecurityModel;
using NVelocity;
using NVelocity.App;
using Sitecore.Modules.Items;

namespace Sitecore.Modules.Workflow
{
	/// <summary>
	///   Custom workflow action that creates an item from a given master, with given
	///   state and puts it into workflow (if defined)
	/// </summary>
	public class CreateFromMasterAction
	{
      public CreateFromMasterAction()
      {
         Velocity.Init();
      }
      
      public void Process(WorkflowPipelineArgs args)
      {
         CreateFromMasterActionItem action = args.ProcessorItem.InnerItem;
         
         if (action.TargetMaster != string.Empty && action.TargetName != string.Empty) 
         {
            Item parent = null;
            if (action.TargetParent != string.Empty)
            {
               parent = Sitecore.Context.ContentDatabase.Items[action.TargetParent];
            }

            if (parent == null)
            {
               parent = args.DataItem;
            }

            Sitecore.Data.ID id = Sitecore.Data.ID.Parse(action.TargetMaster);
            BranchItem branch = args.DataItem.Database.GetItem(id);
            Item target = parent.Add(this.GetTargetName(action.TargetName, parent), branch);

            if (target != null) 
            {
               if (action.TargetWorkflow != string.Empty)
               {
                  using (new SecurityDisabler())
                  {
                     using (new EditContext(target))
                     {
                        target[FieldIDs.Workflow] = action.TargetWorkflow;
                     }
                  }
               }

               if (action.TargetState != string.Empty)
               {
                  using (new SecurityDisabler())
                  {
                     using (new EditContext(target)) 
                     {
                        target[FieldIDs.State] = action.TargetState;
                     }
                  }
               }
            }
         }
      }

      private string GetTargetName(string template, Item parent)
      {
         string unique = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("U");
         VelocityContext context = new VelocityContext();
         context.Put("unique", unique);
         context.Put("parent", parent);
         
         using (StringWriter writer = new StringWriter())
         {
            Velocity.Evaluate(context, writer, "template", template);
            return writer.GetStringBuilder().ToString();
         }
      }
	}
}
