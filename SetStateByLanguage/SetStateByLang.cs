using ExecuteCommand;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Workflows.Simple;

namespace SetStateByLanguage
{
   public class SetStateByLang
   {
      public void Process(WorkflowPipelineArgs args)
      {
         Item item = args.DataItem;

         ProcessorItem workflowItem = args.ProcessorItem;
         string commandToExecute = workflowItem.InnerItem["CommandToExecute"];
         if(commandToExecute == "" || item.Database.Items[commandToExecute] == null || item.Database.Items[commandToExecute]["Next state"] == "")
         {
            Log.Error("Action SetStateByLanguage is failed: 'CommandToExecute' field is not set properly", this);
            return;
         }
         foreach (Language language in item.Languages)
         {
            if (language.Name == item.Language.Name)
            {
               continue;
            }

            Item langItem = item.Database.Items[item.Paths.FullPath, language, item.Version];
            if (GetIsAllowToChangeState(langItem, workflowItem))
            {
               using (new EditContext(langItem))
               {
                  CommandExecuter.Execute(commandToExecute, langItem, "", true);
               }
            }
         }
      }

      bool GetIsAllowToChangeState(Item langItem, ProcessorItem workflowItem)
      {
         if(langItem.State.GetWorkflowState() == null)
            return false;
         return workflowItem.InnerItem.Fields["AllowedToChangeStates"].Value.IndexOf(langItem.State.GetWorkflowState().StateID) != -1;
      }
   }
}