/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Alp Toker (alp@atoker.com) 
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/
//
// elabel.cs: An ellipsizing label widget
//
// Author:
//   Alp Toker (alp@atoker.com)
//
// (C) 2003 Alp Toker
//
using System;
using Gtk;

public class ELabel : Label {
	string text = "";
	const string ellipsis = "...";
	const string en_char = "n";
	int ellipsis_width, en_width;
	int old_width;
	Pango.Layout layout;
	bool refreshed = false;

	public ELabel (string text) : base ("")
	{
		int width, height;
		GetSizeRequest (out width, out height);
		SetSizeRequest (0, height);
		SizeAllocated += new SizeAllocatedHandler (OnSizeAllocated);

		Text = text;
	}

	new public string Text
	{
		get {
			return text;
		} set {
			text = value;
			Reload ();
			Refresh ();
		}
	}

	void OnSizeAllocated (object o, SizeAllocatedArgs args)
	{
		if (refreshed) {
			refreshed = false;
			return;
		}

		if (Allocation.Width != old_width) 
			Refresh ();
		old_width = Allocation.Width;

		refreshed = true;
	}

	void Reload ()
	{
		int tmp;
		layout = Layout.Copy ();

		layout.SetText (ellipsis);
		layout.GetPixelSize (out ellipsis_width, out tmp);

		layout.SetText (en_char);
		layout.GetPixelSize (out en_width, out tmp);
	}

	void Refresh ()
	{
		string ellipsized = Ellipsize (layout, text, Allocation.Width, ellipsis_width, en_width, 0);
		if (base.Text != ellipsized)
			base.Text = ellipsized;
	}

	public static string Ellipsize (Pango.Layout layout, string newtext, int bound, int ellipsis_width, int en_width, int hAdjust)
	{
		int width, tmp;

		layout.SetText (newtext);
		layout.GetPixelSize (out width, out tmp);

		if (bound <= ellipsis_width)
			return ellipsis;

		string ellipsized = "";
		int i = 0;


		if(hAdjust !=0)
		{
			i+=hAdjust;
			while(i<newtext.Length)
			{
				ellipsized = ellipsized+newtext[i];
				layout.SetText(ellipsized);
				layout.GetPixelSize(out width, out tmp);
				if(width > bound - ellipsis_width)
					break;
				i++;
			}
			return ellipsized;
		}


		if (width < bound)
			return newtext;
		//make a guess of where to start
		i = (bound - ellipsis_width) / (en_width);
		if (i >= newtext.Length)
			i = 0;
		ellipsized = newtext.Substring (hAdjust, i);

		//add chars one by one to determine how many are allowed
		while (true)
		{
			ellipsized = ellipsized + newtext[i];
			layout.SetText (ellipsized);
			layout.GetPixelSize (out width, out tmp);

			if (i == newtext.Length - 1) {
				//bad guess, start from the beginning
				ellipsized = "";
				i = 0;
				continue;
			}

			if (width > bound - ellipsis_width)
				break;

			i++;
		}

		ellipsized = ellipsized.Remove (ellipsized.Length - 1, 1);
		ellipsized += ellipsis;

		return ellipsized;
	}
}
