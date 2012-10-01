using System;
using ExecuteCommand;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Workflows;
using Sitecore.Workflows.Simple;

namespace WaitForLanguage
{
	public class WaitForLanguage
	{
		public void Process(WorkflowPipelineArgs args)
		{
		   Item item = args.DataItem;
         string stateId = item.State.GetWorkflowState().StateID;
         ProcessorItem workflowItem = args.ProcessorItem;
		   string langs = workflowItem.InnerItem.Fields["LanguagesToWaitFor"].Value;
         string commandID = workflowItem.InnerItem.Fields["CommandToExecute"].Value;
         if(commandID == "")
         {
            Log.Error("WaitForLanguage action failed. The field 'CommandToExecute' value is not set.", this);
            return;
         }
         Item command = item.Database.Items[commandID];
         if(command["Next state"] == "")
         {
            Log.Error("WaitForLanguage action failed. The field 'Next State' value of the command is not set.", this);
            return;
         }
		   bool result = true;
		   foreach(Language lang in item.Languages)
         {
            if(langs != "")
            {
               if(langs.IndexOf(lang.GetItem(item.Database).ID.ToString()) == -1)
               {
                  continue;
               }
            }
            if(lang.Name == item.Language.Name)
            {
               continue;
            }

            Item langItem = item.Database.Items[item.Paths.FullPath, lang, item.Version];
            WorkflowState workflowState = langItem.State.GetWorkflowState();
            
            result = result && (workflowState.StateID == stateId || workflowState.FinalState);
         }
         if(result)
         {
            foreach(Language lang in item.Languages)
            {
               Item langItem = item.Database.Items[item.Paths.FullPath, lang, item.Version];

               WorkflowState state = langItem.State.GetWorkflowState();
               
               if(workflowItem.InnerItem.Parent.ID.ToString() == state.StateID)
               {
                  
                  WorkflowResult execute = CommandExecuter.Execute(commandID, langItem, "", true);
                  if(!execute.Succeeded)
                  {
                     Log.Error("WaitForLanguage action failed: " + execute.Message, this);
                  }
               }
            }
         }
		}
	}
}
