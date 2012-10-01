using System;

using Sitecore.Data.Items;

namespace Sitecore.Workflows.Simple
{
	public class DeleteItemAction
	{
		public DeleteItemAction()
		{
		}

      public void Process(WorkflowPipelineArgs args)
      {
         Item item = args.DataItem;
         item.Recycle();
      }
	}
}
