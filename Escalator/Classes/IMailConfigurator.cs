/* *********************************************************************** *
 * File   : IMailConfigurator.cs                          Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents contract, which custom classes must implement       *
 *          to realize specific e-mail rendering logic                     *
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

using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.Modules.WorkflowEscalator
{
    public interface IMailConfigurator
    {
        //Renders mail subject
        string GetMailSubject(string subjectPattern);

        //Renders the header of the mail
        string GetMailHeader(string headerPattern);

        //Renders main content of the mail's body
        string GetMailBody(string itemPattern, IEnumerable<Item> items);

        //Renders the footer of the mail
        string GetMailFooter(string footerPattern);
    }
}
