/* *********************************************************************** *
 * File   : Logger.cs                                     Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents logging logic for the module                        *
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
using Sitecore.Diagnostics;

namespace Sitecore.Modules.WorkflowEscalator
{
    static class Logger
    {
        #region Properties, constants and variables

        //Defines the appearance of log messages in log file
        private const string LogPattern = "WORKFLOW ESCALATOR: {0}";

        #endregion

        #region Public methods

        public static void Error(string message, Exception exc, object owner)
        {
            Log.Error(string.Format(LogPattern, message), exc, owner);
        }

        public static void Info(string message, object owner)
        {
            Log.Info(string.Format(LogPattern, message), owner);
        }

        public static void Warn(string message, object owner)
        {
            Log.Warn(string.Format(LogPattern, message), owner);
        }

        #endregion
    }
}