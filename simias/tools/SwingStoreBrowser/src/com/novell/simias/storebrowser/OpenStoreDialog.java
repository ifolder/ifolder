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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

package com.novell.simias.storebrowser;

import java.awt.BorderLayout;
import java.awt.Component;
import java.awt.Container;
import java.awt.FlowLayout;
import java.awt.Frame;
import java.awt.HeadlessException;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import java.net.MalformedURLException;
import java.net.URL;

import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;

public class OpenStoreDialog extends JDialog implements ActionListener, KeyListener {

	private JPanel mainJPanel = null;  //  @jve:decl-index=0:visual-constraint="10,10"
	
	private JPanel messageJPanel = null;
	private JPanel entryJPanel = null;
	private JLabel urlJLabel = null;
	private JComboBox urlJComboBox = null;
	private JPanel buttonsJPanel = null;
	private JButton okButton = null;
	private JButton cancelButton = null;
	
	private boolean okButtonPressed = false;
	
	/**
	 * @param arg0
	 * @param arg1
	 * @throws java.awt.HeadlessException
	 */
	public OpenStoreDialog(Frame arg0, boolean arg1) throws HeadlessException {
		super(arg0, arg1);
		initialize();
	}
	/**
	 * This method initializes this
	 * 
	 * @return void
	 */
	private void initialize() {
        this.setTitle("Open Simias Store");
        this.setModal(true);
        this.setContentPane(getMainJPanel());
        this.setSize(800, 100);
	}
	/**
	 * @return
	 */
	private Container getMainJPanel() {
		if (mainJPanel == null)
		{
			mainJPanel = new JPanel();
			mainJPanel.setLayout(new BorderLayout());
			mainJPanel.add(getMessageJPanel(), java.awt.BorderLayout.CENTER);
			mainJPanel.add(getButtonsJPanel(), java.awt.BorderLayout.SOUTH);
		}
		return mainJPanel;
	}
	/**
	 * @return
	 */
	private Component getMessageJPanel() {
		if (messageJPanel == null)
		{
			messageJPanel = new JPanel();
			messageJPanel.setLayout(new BorderLayout());
			messageJPanel.add(getEntryJPanel(), BorderLayout.CENTER);
			
		}
		return messageJPanel;
	}
	/**
	 * @return
	 */
	private Component getEntryJPanel() {
		if (entryJPanel == null)
		{
			entryJPanel = new JPanel();
			entryJPanel.setLayout(new FlowLayout());
			entryJPanel.add(getUrlJLabel());
			entryJPanel.add(getUrlJComboBox());
			
		}
		return entryJPanel;
	}
	/**
	 * @return
	 */
	private Component getUrlJLabel() {
		if (urlJLabel == null)
		{
			urlJLabel = new JLabel("URL:");
		}
		return urlJLabel;
	}
	/**
	 * @return
	 */
	private Component getUrlJComboBox() {
		if (urlJComboBox == null)
		{
			urlJComboBox = new JComboBox();
			urlJComboBox.setEditable(true);
			urlJComboBox.addKeyListener(this);
		}
		return urlJComboBox;
	}
	/**
	 * @return
	 */
	private Component getButtonsJPanel() {
		if (buttonsJPanel == null)
		{
			buttonsJPanel = new JPanel();
			buttonsJPanel.add(getOKButton(), null);
			buttonsJPanel.add(getCancelButton(), null);
		}
		return buttonsJPanel;
	}
	/**
	 * @return
	 */
	private Component getCancelButton() {
		if (cancelButton == null)
		{
			cancelButton = new JButton("Cancel");
			cancelButton.addActionListener(this);
		}
		return cancelButton;
	}
	/**
	 * @return
	 */
	private Component getOKButton() {
		if (okButton == null)
		{
			okButton = new JButton("OK");
			okButton.setDefaultCapable(true);
			this.getRootPane().setDefaultButton(okButton);
			okButton.addActionListener(this);
		}
		return okButton;
	}
	
	private void doOKAction()
	{
		String urlString = (String)urlJComboBox.getSelectedItem();
		if (urlString == null || urlString.trim().length() == 0)
		{
			JOptionPane.showMessageDialog(this, "No URL selected/specified",
					"Invalid URL", JOptionPane.DEFAULT_OPTION);
		}
		else
		{
			urlString = urlString.trim();
			try {
				// Make sure this is a valid URL
				URL testURL = new URL(urlString);
			} catch (MalformedURLException e1) {
				JOptionPane.showMessageDialog(this, "The specified URL is not a valid URL",
											"Invalid URL", JOptionPane.DEFAULT_OPTION);
				return;
			}
			
			okButtonPressed = true;
			this.setVisible(false);
		}
	}
	
	/* (non-Javadoc)
	 * @see java.awt.event.ActionListener#actionPerformed(java.awt.event.ActionEvent)
	 */
	public void actionPerformed(ActionEvent e) {

		if (e.getSource().equals(okButton))
		{
			System.out.println("OK button pressed");
			
			doOKAction();
		}
		else if (e.getSource().equals(cancelButton))
		{
			System.out.println("Cancel button pressed");
			
			okButtonPressed = false;
			this.setVisible(false);
		}
	}
	/**
	 * Add the URL to the combo box
	 * 
	 * @param url
	 */
	public void addStoreURL(String url) {
		// If the item is in the list already, don't add it again
		for (int i = 0; i < urlJComboBox.getItemCount(); i++)
		{
			String item = (String)urlJComboBox.getItemAt(i);
			if (item.compareTo(url) == 0)
				return; // Item is already in the list
		}
		
		// If the for loop doesn't return, it's not already in the list 
		urlJComboBox.addItem(url);
	}

	/**
	 * @return Returns the selectedURL.
	 */
	public String getSelectedURL() {
		String selectedURL = (String) urlJComboBox.getSelectedItem();
		return selectedURL;
	}
	/**
	 * @return Returns the okButtonPressed.
	 */
	public boolean getOkButtonPressed() {
		return okButtonPressed;
	}
	/* (non-Javadoc)
	 * @see java.awt.event.KeyListener#keyTyped(java.awt.event.KeyEvent)
	 */
	public void keyTyped(KeyEvent arg0) {
		// Intentionally unimplemented
	}
	/* (non-Javadoc)
	 * @see java.awt.event.KeyListener#keyPressed(java.awt.event.KeyEvent)
	 */
	public void keyPressed(KeyEvent arg0) {
		// If the user presses ENTER while in the ComboBox, take the same
		// actions as if they pressed OK.
		if (arg0.getKeyCode() == KeyEvent.VK_ENTER)
		{
			doOKAction();
		}
	}
	/* (non-Javadoc)
	 * @see java.awt.event.KeyListener#keyReleased(java.awt.event.KeyEvent)
	 */
	public void keyReleased(KeyEvent arg0) {
		// Intentionally unimplemented
	}
}
