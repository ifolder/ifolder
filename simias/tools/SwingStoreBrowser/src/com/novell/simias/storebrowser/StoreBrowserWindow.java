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
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.net.MalformedURLException;
import java.rmi.RemoteException;
import java.util.ArrayList;

import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JSplitPane;
import javax.swing.JTable;
import javax.swing.JTree;
import javax.swing.event.TreeExpansionEvent;
import javax.swing.event.TreeSelectionEvent;
import javax.swing.event.TreeSelectionListener;
import javax.swing.event.TreeWillExpandListener;
import javax.swing.table.DefaultTableColumnModel;
import javax.swing.table.DefaultTableModel;
import javax.swing.table.TableColumn;
import javax.swing.tree.DefaultMutableTreeNode;
import javax.swing.tree.DefaultTreeModel;
import javax.swing.tree.ExpandVetoException;
import javax.swing.tree.TreeModel;
import javax.swing.tree.TreePath;
import javax.swing.tree.TreeSelectionModel;

import com.novell.simias.browser.ArrayOfBrowserNode;
import com.novell.simias.browser.BrowserNode;
import com.novell.simias.browser.Browser_x0020_ServiceSoap;
/**
 * @author Boyd Timothy <btimothy@novell.com>
 */
public class StoreBrowserWindow extends javax.swing.JFrame implements TreeSelectionListener, TreeWillExpandListener {

	private javax.swing.JPanel jContentPane = null;

	private Browser_x0020_ServiceSoap service = null;
	private DefaultMutableTreeNode tree_top = null;
	
	private JMenuBar jJMenuBar = null;
	private JMenu jMenuFile = null;
	private JMenuItem jMenuItemExit = null;
	private JMenu jMenu = null;
	private JMenuItem jMenuItemOpenStore = null;
	private JPanel jPanel = null;
	private JSplitPane jSplitPane = null;
	private JScrollPane jScrollPane = null;
	private JTree jTree = null;
	private JTable jTable = null;
	private JScrollPane jScrollPane1 = null;
	private TreeModel treeModel = null;
	
	private String[] columnNames = {"Name", "Value", "Type", "Flags"};
	
	/**
	 * This is the default constructor
	 */
	public StoreBrowserWindow() {
		super();
		initialize();
	}
	/**
	 * This method initializes this
	 * 
	 * @return void
	 */
	private void initialize() {
		this.setTitle("Simias Store Browser");
		this.setName("SimiasStoreBrowser");
		this.setSize(600, 600);
		this.setContentPane(getJContentPane());
		this.addWindowListener(new WindowAdapter() {
			public void windowClosing(WindowEvent e) {
				System.out.println("Exiting Store Browser");
				System.exit(0);
			}
		});

	}
	/**
	 * This method initializes jContentPane
	 * 
	 * @return javax.swing.JPanel
	 */
	private javax.swing.JPanel getJContentPane() {
		if (jContentPane == null) {
			jContentPane = new javax.swing.JPanel();
			jContentPane.setLayout(new java.awt.BorderLayout());
			jContentPane.add(getJPanel(), java.awt.BorderLayout.NORTH);
			jContentPane.add(getJSplitPane(), java.awt.BorderLayout.CENTER);
		}
		return jContentPane;
	}
	/**
	 * This method initializes jJMenuBar
	 * 
	 * @return javax.swing.JMenuBar
	 */
	private JMenuBar getJJMenuBar() {
		if (jJMenuBar == null) {
			jJMenuBar = new JMenuBar();
			jJMenuBar.add(getJMenu());
		}
		return jJMenuBar;
	}
	/**
	 * This method initializes jMenu
	 * 
	 * @return javax.swing.JMenu
	 */
	private JMenu getJMenu() {
		if (jMenuFile == null) {
			jMenuFile = new JMenu();
			jMenuFile.setText("File");
			jMenuFile.setMnemonic(java.awt.event.KeyEvent.VK_F);
			jMenuFile.add(getJMenuItemOpenStore());
			jMenuFile.add(getJMenuItemExit());
		}
		return jMenuFile;
	}
	/**
	 * @return
	 */
	private JMenuItem getJMenuItemOpenStore() {
		if (jMenuItemOpenStore == null) {
			jMenuItemOpenStore = new JMenuItem();
			jMenuItemOpenStore.setText("Open Store...");
			jMenuItemOpenStore.setMnemonic(java.awt.event.KeyEvent.VK_O);
			jMenuItemOpenStore
					.addActionListener(new java.awt.event.ActionListener() {
						public void actionPerformed(java.awt.event.ActionEvent e) {
							System.out.println("File | Open Store... Button Selected");


							openStore();
						}
					});
		}
		return jMenuItemOpenStore;
	}
	/**
	 * 
	 */
	protected void openStore() {
		
		// TODO: Prompt the user for a URL
		String url = (String) JOptionPane.showInputDialog(this, "Simias Store URL", "Open Simias Store",
												 JOptionPane.QUESTION_MESSAGE, null, null,
												 "http://bht-linux.provo.novell.com:49448/simias10/boyd/SimiasBrowser.asmx");
		if (url == null)
		{
			return;
		}
//		URLPrompt urlPrompt = new URLPrompt(this, true);
//		urlPrompt.show();

//		String url = "http://bht-linux.provo.novell.com:49448/simias10/boyd/SimiasBrowser.asmx";
		
		try {
			service = SwingStoreBrowser.getSoapService(url);
		} catch (MalformedURLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return;
		}
		
		if (service == null)
		{
			// TODO: Tell the user that the service is null
			return;
		}
		
		tree_top = new DefaultMutableTreeNode(url);
		
		addNodes(tree_top);

		treeModel = new DefaultTreeModel(tree_top);
		treeModel.addTreeModelListener(new StoreTreeModelListener());
		
		jTree.setModel(treeModel);
	}
	/**
	 * @param top
	 */
	private void addNodes(DefaultMutableTreeNode top) {
		top.removeAllChildren();	// Remove the old
		
		DefaultMutableTreeNode collection = null;
		ArrayOfBrowserNode collections = null;
		try {
			collections = service.enumerateCollections();
		} catch (RemoteException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return;
		}
		
		BrowserNode browserNodeA[] = collections.getBrowserNode();
		for (int i = 0; i < browserNodeA.length; i++)
		{
			String nodeData = browserNodeA[i].getNodeData();
			if (nodeData == null)
			{
				// TODO: Inform the user somehow
				continue;
			}
			
			SimiasNode simiasNode = new SimiasNode(nodeData);
			
			collection = new DefaultMutableTreeNode();
			collection.setUserObject(simiasNode);
			// FIXME: Figure out a better way to do this (only put expanders on Collections)
			// Add a dummy child node so that the collection will have
			// and expander on it.  When it goes to expand, we will remove
			// the dummy child and make a call over the wire to actually
			// get the children.
			collection.add(new DefaultMutableTreeNode("temp"));
			
			top.add(collection);
		}
	}
	
	private void addChildNodes(DefaultMutableTreeNode parentNode, BrowserNode[] childNodesA)
	{
		for (int i = 0; i < childNodesA.length; i++)
		{
			String nodeData = childNodesA[i].getNodeData();
			if (nodeData == null)
			{
				// TODO: Inform the user somehow
				continue;
			}
			
			SimiasNode simiasNode = new SimiasNode(nodeData);
			
			DefaultMutableTreeNode childNode = new DefaultMutableTreeNode();
			childNode.setUserObject(simiasNode);
			if (simiasNode.isCollection())
			{
				// FIXME: Figure out a better way to do this (only put expanders on Collections)
				// Add a dummy child node so that the collection will have
				// and expander on it.  When it goes to expand, we will remove
				// the dummy child and make a call over the wire to actually
				// get the children.
				childNode.add(new DefaultMutableTreeNode("temp"));
			}
			
			parentNode.add(childNode);
		}
	}
	
	/**
	 * This method initializes jMenuItemExit
	 * 
	 * @return javax.swing.JMenuItem
	 */
	private JMenuItem getJMenuItemExit() {
		if (jMenuItemExit == null) {
			jMenuItemExit = new JMenuItem();
			jMenuItemExit.setText("Exit");
			jMenuItemExit.setMnemonic(java.awt.event.KeyEvent.VK_X);
			jMenuItemExit
					.addActionListener(new java.awt.event.ActionListener() {
						public void actionPerformed(java.awt.event.ActionEvent e) {
							System.out.println("File | Exit Button Selected");

							// TODO: Add a confirm exit dialog prompt

							System.exit(0);
						}
					});
		}
		return jMenuItemExit;
	}
	/**
	 * This method initializes jPanel	
	 * 	
	 * @return javax.swing.JPanel	
	 */    
	private JPanel getJPanel() {
		if (jPanel == null) {
			jPanel = new JPanel();
			jPanel.setLayout(new BorderLayout());
			this.setJMenuBar(getJJMenuBar());
// CRG- If you do this, it will be more compatible on OS X
//			jPanel.add(getJJMenuBar(), java.awt.BorderLayout.NORTH);
		}
		return jPanel;
	}
	/**
	 * This method initializes jSplitPane	
	 * 	
	 * @return javax.swing.JSplitPane	
	 */    
	private JSplitPane getJSplitPane() {
		if (jSplitPane == null) {
			jSplitPane = new JSplitPane();
			jSplitPane.setLeftComponent(getJScrollPane());
			jSplitPane.setRightComponent(getJScrollPane1());
		}
		return jSplitPane;
	}
	/**
	 * This method initializes jScrollPane	
	 * 	
	 * @return javax.swing.JScrollPane	
	 */    
	private JScrollPane getJScrollPane() {
		if (jScrollPane == null) {
			jScrollPane = new JScrollPane();
			jScrollPane.setViewportView(getJTree());
		}
		return jScrollPane;
	}
	/**
	 * This method initializes jTree	
	 * 	
	 * @return javax.swing.JTree	
	 */    
	private JTree getJTree() {
		if (jTree == null) {
			jTree = new JTree();
			jTree.getSelectionModel().setSelectionMode(TreeSelectionModel.SINGLE_TREE_SELECTION);
			jTree.addTreeSelectionListener(this);
			jTree.addTreeWillExpandListener(this);
			jTree.setShowsRootHandles(true);
		}
		return jTree;
	}
	/**
	 * This method initializes jTable	
	 * 	
	 * @return javax.swing.JTable	
	 */    
	private JTable getJTable() {
		if (jTable == null) {
			jTable = new JTable();
		}
		return jTable;
	}
	/**
	 * This method initializes jScrollPane1	
	 * 	
	 * @return javax.swing.JScrollPane	
	 */    
	private JScrollPane getJScrollPane1() {
		if (jScrollPane1 == null) {
			jScrollPane1 = new JScrollPane();
			jScrollPane1.setViewportView(getJTable());
		}
		return jScrollPane1;
	}
	/* (non-Javadoc)
	 * @see javax.swing.event.TreeSelectionListener#valueChanged(javax.swing.event.TreeSelectionEvent)
	 */
	public void valueChanged(TreeSelectionEvent arg0) {
		DefaultMutableTreeNode node = (DefaultMutableTreeNode)
									  jTree.getLastSelectedPathComponent();
		
		if (node == null) return;
		
		if (node == tree_top) return; // Do nothing
		
		SimiasNode simiasNode = (SimiasNode)node.getUserObject();
		if (simiasNode == null) return;
		
		displaySimiasNode(simiasNode);
	}
	
	/**
	 * Populate the jTable with the contents of simiasNode
	 * @param simiasNode
	 */
	private void displaySimiasNode(SimiasNode simiasNode) {
		DefaultTableModel tableModel = new DefaultTableModel();
		tableModel.addColumn(columnNames[0]);
		tableModel.addColumn(columnNames[1]);
		tableModel.addColumn(columnNames[2]);
		tableModel.addColumn(columnNames[3]);

		ArrayList props = simiasNode.getProperties();
		for (int i = 0; i < props.size(); i++)
		{
			SimiasNodeProperty prop = (SimiasNodeProperty)props.get(i);
			Object[] rowData = {prop.getName(), prop.getValue(), prop.getType(), prop.getFlags()};
			tableModel.addRow(rowData);
		}
		
		jTable.setModel(tableModel);
	}
	/* (non-Javadoc)
	 * @see javax.swing.event.TreeWillExpandListener#treeWillExpand(javax.swing.event.TreeExpansionEvent)
	 */
	public void treeWillExpand(TreeExpansionEvent arg0) throws ExpandVetoException {
		if (service == null)
		{
			// TODO: Tell the user that the service is null
			return;
		}

		TreePath treePath = arg0.getPath();
		
		DefaultMutableTreeNode node = (DefaultMutableTreeNode)
		  treePath.getLastPathComponent();
		
		if (node == null) return;	// Can't do anything if this happens

		if (node == tree_top)
		{
			addNodes(node);
			return;	// Do nothing for now.  TODO: Re-read the top-level items
		}
		
		// Clear any existing children and re-read from server
		node.removeAllChildren();
		
		SimiasNode simiasNode = (SimiasNode)node.getUserObject();
		
		// Go read any children of this node
		String nodeID = simiasNode.getId();
		if (nodeID != null && nodeID.length() > 0)
		{
			/* See if the collection has other nodes */
			ArrayOfBrowserNode childNodes = null;
			try {
				childNodes = service.enumerateNodes(nodeID);
			} catch (RemoteException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
			if (childNodes != null)
			{
				addChildNodes(node, childNodes.getBrowserNode());
			}
		}
	}

	/* (non-Javadoc)
	 * @see javax.swing.event.TreeWillExpandListener#treeWillCollapse(javax.swing.event.TreeExpansionEvent)
	 */
	public void treeWillCollapse(TreeExpansionEvent arg0) throws ExpandVetoException {
		// Intentionally left blank
	}
} //  @jve:visual-info  decl-index=0 visual-constraint="0,0"
