/* *********************************************************************** *
 * File   : CreateFromMasterActionItem.cs                 Part of Sitecore *
 * Version: 1.00                                          www.sitecore.net *
 * Author : Sitecore A/S                                                   *
 *                                                                         *
 * Purpose: To implement helper class for create item workflow action      *
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
using Sitecore.Data.Items;


namespace Sitecore.Modules.Items
{
	/// <summary>
	///   
	/// </summary>
   public class CreateFromMasterActionItem : CustomItem
   {
      public CreateFromMasterActionItem(Item item) : base(item) { }

      #region public properties

      public string TargetMaster
      {
         get
         {
            return this["TargetMaster"];
         }
      }

      public string TargetName
      {
         get
         {
            return this["TargetName"];
         }
      }

      public string TargetState
      {
         get
         {
            return this["TargetState"];
         }
      }

      public string TargetWorkflow
      {
         get
         {
            return this["TargetWorkflow"];
         }
      }

      public string TargetParent
      {
         get
         {
            return this["TargetParent"];
         }
      }

      #endregion public properties

      #region static helpers

      public static implicit operator CreateFromMasterActionItem(Item item)
      {
         return item == null ? null : new CreateFromMasterActionItem(item);
      }

      #endregion static helpers

   }
}
