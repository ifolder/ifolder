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
  *                 $Author: Boyd Timothy <btimothy@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using System;
using System.Threading;

using Gtk;

namespace Novell.iFolder
{

    /// <summary>
    /// class iFolderWaitDialog
    /// </summary>
public class iFolderWaitDialog : Dialog
{
	private ButtonSet		buttonSet;
	private ProgressBar	progressBar;
	private Timer			progressBarTimer;
	private bool			bHideCalled;

    /// <summary>
    /// Enum ButtonSet
    /// </summary>
	public enum ButtonSet : int
	{
		Cancel,
		None
	}

	
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parent">Parent Window</param>
    /// <param name="waitingWidget">Waiting Widget</param>
    /// <param name="buttonSet">Button Set</param>
    /// <param name="title">Title message</param>
    /// <param name="statement">Message</param>
    /// <param name="secondaryStatement"></param>
	public iFolderWaitDialog(Window parent,
							 Widget waitingWidget,
							 ButtonSet buttonSet,
							 string title,
							 string statement, 
							 string secondaryStatement)
		: base()
	{
		Init(parent, waitingWidget, buttonSet, title, statement, secondaryStatement);
	}

	internal void Init(Gtk.Window parent,
					   Widget waitingWidget,
					   ButtonSet buttonSet,
					   string title,
					   string statement,
					   string secondaryStatement)
	{
		this.Title = title;
		this.HasSeparator = false;
//		this.BorderWidth = 10;
		this.Resizable = false;
		this.Modal = true;
		if(parent != null)
			this.TransientFor = parent;
		
		this.buttonSet = buttonSet;
		
		VBox contentVBox = new VBox();
		this.VBox.Add(contentVBox);

		HBox h = new HBox();
		contentVBox.PackStart(h, true, true, 0);
		h.BorderWidth = 10;
		h.Spacing = 10;

		if (waitingWidget != null)
		{
			h.PackStart(waitingWidget, false, false, 0);
		}

		VBox v = new VBox();
		v.Spacing = 10;
		Label l = new Label();
		l.LineWrap = true;
		l.UseMarkup = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		l.Markup = "<span weight=\"bold\" size=\"larger\">" + GLib.Markup.EscapeText(statement) + "</span>";
		v.PackStart(l);

		l = new Label(secondaryStatement);
		l.LineWrap = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		v.PackStart(l, true, true, 8);
		
		h.PackEnd(v);

		progressBar = new ProgressBar();
		contentVBox.PackStart(progressBar, true, false, 8);
		//progressBar.ActivityBlocks = 20;
		progressBar.Orientation = ProgressBarOrientation.LeftToRight;
		progressBar.PulseStep = 0.05;
		
		contentVBox.ShowAll();
		
		this.Realized += new EventHandler(OnRealizeWidget);
		
		switch(buttonSet)
		{
			case ButtonSet.Cancel:
				this.AddButton(Stock.Cancel, ResponseType.Cancel);
				break;
			case ButtonSet.None:
			default:
				break;
		}
		
		bHideCalled = false;
	}
	
	private void OnRealizeWidget(object o, EventArgs args)
	{
		progressBarTimer =
			new Timer(new TimerCallback(UpdateProgress),
						null,
						250,
						250);
	}
	
	private void UpdateProgress(object state)
	{
		if (!bHideCalled)
			GLib.Idle.Add(PulseProgressBar);
	}
	
    /// <summary>
    /// Pulse Progress Bar
    /// </summary>
    /// <returns>true on success</returns>
	private bool PulseProgressBar()
	{
		if (!bHideCalled)
			progressBar.Pulse();
		return false; // Prevent GLib.Idle from automatically calling this again
	}
	
	protected override bool OnDeleteEvent(Gdk.Event evnt)
	{
		return true;
		/*
		if (progressBarTimer != null)
		{
			progressBarTimer.Dispose();
			progressBarTimer = null;
		}

		if (buttonSet == ButtonSet.None)
			return true; // Do nothing and don't let this be seen elsewhere
		else
			return false;	// Allow this to send the cancel
		*/
	}
	
	protected override bool OnDestroyEvent(Gdk.Event evnt)
	{
		if (progressBarTimer != null)
		{
			progressBarTimer.Dispose();
			progressBarTimer = null;
		}

		if (buttonSet == ButtonSet.None)
			return true; // Do nothing and don't let this be seen elsewhere
		else
			return false;	// Allow this to send the cancel
	}
	
	protected override void OnHidden()
	{
		bHideCalled = true;
		if (progressBarTimer != null)
		{
			progressBarTimer.Dispose();
			progressBarTimer = null;
		}
	}

//	private void ShowDetailsButtonPressed(object o, EventArgs args)
//	{
//		if (showDetailsButton.Label == Util.GS("Show _Details"))
//		{
//			showDetailsButton.Label = Util.GS("Hide _Details");
//			showDetailsScrolledWindow.Visible = true;
//		}
//		else
//		{
//			showDetailsButton.Label = Util.GS("Show _Details");
//			showDetailsScrolledWindow.Visible = false;
//		}
//	}
}

}
