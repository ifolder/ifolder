/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Text;
using System.Text.RegularExpressions;
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Used to parse a string into a name object.
	/// </summary>
	public class FullNameParser
	{
		/// <summary>
		/// Initializes a new instance of the FullNameParser class.
		/// </summary>
		public FullNameParser()
		{
		}

		// TODO - we will need to provide a way to perform culture-specific parsing.

		/// <summary>
		/// Parses a string to a name object
		/// </summary>
		/// <param name="fullName">The string to parse.</param>
		/// <param name="name">The resulting name object.</param>
		/// <returns>true, if the name was parsed successfully.</returns>
		public static bool Parse(string fullName, ref Name name)
		{
			bool validName = true;

			// TODO - much more to be done here but this is a good starting point.

			if (fullName.Length > 0)
			{
				// Get the prefix.
				Regex prefix = new Regex(@"mr |mr\. |mrs |mrs\. |miss |ms |ms\. |dr |dr\. ", RegexOptions.IgnoreCase);
				Match matchPrefix = prefix.Match(fullName);
				if (matchPrefix.Success)
				{
					// TODO - need to fixup prefix ... i.e. change mr to Mr.

					if (matchPrefix.Index != 0)
					{
						// If the prefix is not at the beginning, then we don't know how to handle the name.
						validName = false;
					}

					// Save the prefix.
					name.Prefix = fullName.Substring(0, matchPrefix.Index + matchPrefix.Length - 1);

					// Remove the prefix from the name.
					fullName = fullName.Substring(matchPrefix.Index + matchPrefix.Length).Trim();
				}

				// Get the suffix.
				Regex suffix = new Regex(@" i| ii| iii| jr| jr\.| sr| sr\.| esq| esq\.| md| m\.d\.", RegexOptions.IgnoreCase);
				Match matchSuffix = suffix.Match(fullName);
				if (matchSuffix.Success)
				{
					// TODO - need to fixup suffix ... i.e. change jr to Jr.

					if (fullName.IndexOf(" ", matchSuffix.Index + 1) != -1)
					{
						// If the suffix is not at the end, then we don't know how to handle the name.
						validName = false;
					}

					// Save the suffix.
					name.Suffix = fullName.Substring(matchSuffix.Index + 1);

					// Remove the suffix from the name.
					fullName = fullName.Substring(0, matchSuffix.Index).Trim();
				}

				// Process the rest of the name.
				if (fullName.Length > 0)
				{
					// TODO - need to make sure names are capitalized.

					// Split the name into substrings.
					Regex ex = new Regex("[ ]+");
					string[] s = ex.Split(fullName);
					int length = s.Length;

					// Save the given name.
					name.Given = s[0];

					if (length != 2)
					{
						if (length == 1)
						{
							// If only one string was specified, we don't know how to handle this.
							validName = false;
						}
						else
						{
							StringBuilder middleName = new StringBuilder();

							for (int n = 1; n < length - 1; n++)
							{
								// Build the middle name ... currently any excess names are combined in the
								// middle name.
								middleName.Append(s[n] + " ");
							}

							name.Other = middleName.ToString().Trim();

							// Save the last name.
							name.Family = s[length - 1];
						}

						if (length > 3)
						{
							// If more than three strings were specified, we don't know how to handle this.
							validName = false;
						}
					}
					else
					{
						// The second name must be the last name.
						name.Family = s[length - 1];

						// Clear out the middle name.
						name.Other = "";
					}
				}
			}
			else
			{
				// No string was passed in so reset all the name fields.
				name.Given = "";
				name.Other = "";
				name.Family = "";
				name.Prefix = "";
				name.Suffix = "";
			}

			return validName;
		}
	}
}
