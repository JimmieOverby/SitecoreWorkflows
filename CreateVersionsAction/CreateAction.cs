using System;
using Sitecore;
using Sitecore.Workflows.Simple;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Configuration;

namespace CreateVersions         
{
    public class CreateVersionsAction
    {
        public CreateVersionsAction()
        {
        }

        private string ReplaceVariables(string text, WorkflowPipelineArgs args)//using System.Data;
        {
            text = text.Replace("$itemPath$", args.DataItem.Paths.FullPath);
            text = text.Replace("$itemLanguage$", args.DataItem.Language.ToString());
            text = text.Replace("$itemVersion$", args.DataItem.Version.ToString());
            return text;
        }

 
        private string GetText(Item commandItem, string field, WorkflowPipelineArgs args)
        {
            string text = commandItem[field];
            if (text.Length > 0)
            {
                return this.ReplaceVariables(text, args);
            }
            return string.Empty;
        }


        public void Process(WorkflowPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            ProcessorItem processorItem = args.ProcessorItem;
            if (processorItem != null)
            {
                Item innerItem = processorItem.InnerItem;
                string[] langsIDs = this.GetText(innerItem, "Languages", args).Split('|');

                foreach (string langID in langsIDs)
                {
                    Item langItm = Factory.GetDatabase("master").GetItem(langID);
                    if (langItm != null)
                    {
                        Item workFlowItem = args.DataItem;
                        Language language = Language.Parse(langItm.Name);
                        if (language != workFlowItem.Language)
                        {
                            Item langItem = workFlowItem.Database.GetItem(workFlowItem.ID, language);
                            langItem = langItem.Versions.AddVersion();
                        }
                    }
                }

            }
        }
    }
}
