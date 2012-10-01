using System;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Workflows.Simple;
using Convert = System.Convert;

namespace SetArchive
{
	public class SetArchive
	{
      public void Process(WorkflowPipelineArgs args)
      {
         int addDays = Convert.ToInt32(args.ProcessorItem.InnerItem.Fields["AddDays"].Value);
         DateTime taskRunDate = DateTime.Now.AddDays(addDays);
         string isoDate = DateUtil.ToIsoDate(taskRunDate);
         isoDate = isoDate.Substring(0, 9) + "120000";
         using(new EditContext(args.DataItem))
         {
            args.DataItem.Fields["__Archive date"].Value = isoDate;
         }
      }
	}
}
