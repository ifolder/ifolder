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
import java.awt.Component;
import java.awt.Image;
import java.io.IOException;
import java.net.MalformedURLException;
import java.rmi.RemoteException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Comparator;
import java.util.Iterator;
import java.util.TreeSet;

import javax.swing.Icon;
import javax.swing.ImageIcon;
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
import javax.swing.table.DefaultTableModel;
import javax.swing.tree.DefaultMutableTreeNode;
import javax.swing.tree.DefaultTreeCellRenderer;
import javax.swing.tree.DefaultTreeModel;
import javax.swing.tree.ExpandVetoException;
import javax.swing.tree.TreePath;
import javax.swing.tree.TreeSelectionModel;

import com.novell.simias.browser.ArrayOfBrowserNode;
import com.novell.simias.browser.BrowserNode;
import com.novell.simias.browser.Browser_x0020_ServiceSoap;
/**
 * @author Boyd Timothy <btimothy@novell.com>
 */
public class StoreBrowserWindow extends javax.swing.JFrame implements TreeSelectionListener, TreeWillExpandListener {

	private static final long serialVersionUID = 12345L;

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
	private DefaultTreeModel treeModel = null;
	
	private OpenStoreDialog openStoreDialog = null;
	private Config config = null;
	
	private String[] columnNames = {"Name", "Value", "Type", "Flags"};
	
	static Icon localDatabaseIcon = null;
	static Icon domainIcon = null;
	static Icon poBoxIcon = null;
	static Icon iFolderIcon = null;
	static Icon collectionIcon = null;
	static Icon memberIcon = null;
	static Icon subscriptionIcon = null;
	static Icon dirNodeIcon = null;
	static Icon fileNodeIcon = null;
	
	static
	{
		ImageIcon icon;

		icon = createImageIcon("images/localdatabase.gif");
		if (icon != null)
			localDatabaseIcon = icon;
		
		icon = createImageIcon("images/domain.gif");
		if (icon != null)
			domainIcon = icon;
		
		icon = createImageIcon("images/pobox.gif");
		if (icon != null)
			poBoxIcon = icon;
		
		icon = createImageIcon("images/ifolder.png");
		if (icon != null)
			iFolderIcon = icon;
		
		icon = createImageIcon("images/collection.gif");
		if (icon != null)
			collectionIcon = icon;
		
		icon = createImageIcon("images/member.png");
		if (icon != null)
			memberIcon = icon;
		
		icon = createImageIcon("images/subscription.png");
		if (icon != null)
			subscriptionIcon = icon;
		
		icon = createImageIcon("images/dirnode.gif");
		if (icon != null)
			dirNodeIcon = icon;
		
		icon = createImageIcon("images/filenode.gif");
		if (icon != null)
			fileNodeIcon = icon;
	}
	
	protected static ImageIcon createImageIcon(String path)
	{
		java.io.File iconFile = new java.io.File(path);
		if (iconFile.exists())
		{
			ImageIcon icon = new ImageIcon(path);
			if (icon != null)
			{
				System.out.println("Loaded: " + path);

				Image img =
					icon.getImage().getScaledInstance(16, 16, Image.SCALE_SMOOTH);
				return new ImageIcon(img);
			}
		}
		else
		{
			System.err.println("Couldn't find file: " + iconFile.getAbsolutePath());
		}

		return null;
	}

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
		this.setSize(800, 600);
		this.setContentPane(getJContentPane());
		this.addWindowListener(new WindowAdapter() {
			public void windowClosing(WindowEvent e) {
				System.out.println("Exiting Store Browser");
				System.exit(0);
			}
		});

		// Attempt to open/connect to the local machine's store
		openLocalStore();
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
		
		OpenStoreDialog dialog = getOpenStoreDialog();
		
		dialog.setVisible(true);
		
		if (!dialog.getOkButtonPressed()) return;
		
		String url = openStoreDialog.getSelectedURL();
		String username = openStoreDialog.getUsername();
		String password = openStoreDialog.getPassword();
		
		if (url == null)
		{
			return;
		}

		Browser_x0020_ServiceSoap newService = null;
		
		try {
			newService = SwingStoreBrowser.getSoapService(url, username, password);
			
		} catch (MalformedURLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return;
		}
		
		if (newService == null)
		{
			// TODO: Tell the user that the service is null
			return;
		}
		
		// Test the service to see if it is valid
		try
		{
			// FIXME: We should really add a Ping() method to the WebService instead
			// of using this function
System.out.print("Attempting to connect to store...");
			newService.enumerateCollections();
System.out.println("connected.");
		}
		catch(Exception e)
		{
System.out.println("failed.");
			// The service is invalid because we weren't able to make a call on it
			JOptionPane.showMessageDialog(this,
				"There is no valid store service running at the specified URL",
				"Invalid Store", JOptionPane.DEFAULT_OPTION);
			return;
		}

		// If we make it this far, the service is good and we're connected
		service = newService;
		
		// Add this URL to the config file
		Config c = getConfig();
		try {
			c.addStoreURL(url);
		} catch (IOException e1) {
			// The service is invalid because we weren't able to make a call on it
			JOptionPane.showMessageDialog(this,
				"Error saving URL into config file: " + e1.getLocalizedMessage(),
				"Invalid URL", JOptionPane.DEFAULT_OPTION);
		}
		
		// Also add it into the combo box
		dialog.addStoreURL(url);

		tree_top = new DefaultMutableTreeNode(url);
		
		addNodes(tree_top);

		treeModel = new DefaultTreeModel(tree_top);
		
		jTree.setModel(treeModel);
	}

	protected void openLocalStore()
	{
		Browser_x0020_ServiceSoap newService =
			SwingStoreBrowser.getLocalSoapService();
		
		if (newService == null)
		{
			// TODO: Tell the user that the service is null
			return;
		}

		// Test the service to see if it is valid
		try
		{
			// FIXME: We should really add a Ping() method to the WebService instead
			// of using this function
System.out.print("Attempting to connect to local store...");
			newService.enumerateCollections();
System.out.println("connected.");
		}
		catch(Exception e)
		{
System.out.println("failed.");
			// Couldn't connect to the local store, so bring up the open store dialog
			openStore();
			return;
		}

		// If we make it this far, the service is good and we're connected
		service = newService;
		
		tree_top = new DefaultMutableTreeNode("localhost");
		addNodes(tree_top);
		treeModel = new DefaultTreeModel(tree_top);
		jTree.setModel(treeModel);
	}
	
	/**
	 * @return
	 */
	private OpenStoreDialog getOpenStoreDialog()
	{
		if (openStoreDialog == null)
		{
			openStoreDialog = new OpenStoreDialog(this, true);
			
			// Read in the saved store URLs from a config file
			Config c = getConfig();

			Collection storeURLs = c.getStoreURLs();
			Iterator i = storeURLs.iterator();
			while(i.hasNext())
			{
				openStoreDialog.addStoreURL((String)i.next());
			}
			
			// Add an example URL
			openStoreDialog.addStoreURL("http://bht-linux.provo.novell.com:49448/simias10/boyd/SimiasBrowser.asmx");
		}
		return openStoreDialog;
	}
	/**
	 * @return
	 */
	private Config getConfig() {
		if (config == null)
		{
			try {
				config = Config.loadConfig("SwingStoreBrowserConfig.xml");
			} catch (Exception e) {
				// Create a new config file
				config = new Config("SwingStoreBrowserConfig.xml");
			}
		}
		return config;
	}

	/**
	 * @param top
	 */
	private void addNodes(DefaultMutableTreeNode top) {
		top.removeAllChildren();	// Remove the old
		
		DefaultMutableTreeNode treeNode = null;
		TreeSet treeSet = new TreeSet(new TreeSetComparator());
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
			
			treeNode = new DefaultMutableTreeNode();
			treeNode.setUserObject(simiasNode);
			// FIXME: Figure out a better way to do this (only put expanders on Collections)
			// Add a dummy child node so that the collection will have
			// and expander on it.  When it goes to expand, we will remove
			// the dummy child and make a call over the wire to actually
			// get the children.
			treeNode.add(new DefaultMutableTreeNode("temp"));
		
			treeSet.add(treeNode);
		}
		
		// Add the sorted list into the tree
		Iterator i = treeSet.iterator();
		if (i != null)
		{
			while (i.hasNext())
			{
				top.add((DefaultMutableTreeNode)i.next());
			}
		}
	}
	
	private void addChildNodes(DefaultMutableTreeNode parentNode, BrowserNode[] childNodesA)
	{
		TreeSet treeSet = new TreeSet(new TreeSetComparator());
		for (int i = 0; i < childNodesA.length; i++)
		{
			String nodeData = childNodesA[i].getNodeData();
			if (nodeData == null)
			{
				// TODO: Inform the user somehow
				continue;
			}
			
			SimiasNode simiasNode = new SimiasNode(nodeData);

			// Don't add the node if it's the same as the parent
			SimiasNode parentSimiasNode = (SimiasNode) parentNode.getUserObject();
			String parentID = parentSimiasNode.getId();
			String childID = simiasNode.getId();
			if (parentID.equals(childID)) {
				continue;
			}
			
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
			
			treeSet.add(childNode);
		}
		
		Iterator i = treeSet.iterator();
		while (i.hasNext())
		{
			parentNode.add((DefaultMutableTreeNode)i.next());
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
			jSplitPane.setDividerLocation(200);
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
			
			// Make sure there's no children when the app opens
			jTree.setModel(new DefaultTreeModel(tree_top));
			
			jTree.setShowsRootHandles(true);
			
			jTree.setCellRenderer(new iFolderTreeCellRenderer());
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
			System.out.println("SERVICE IS NULL!");
			return;
		}

		TreePath treePath = arg0.getPath();
		
		DefaultMutableTreeNode node = (DefaultMutableTreeNode)
		  treePath.getLastPathComponent();
		
		if (node == null) return;	// Can't do anything if this happens

		if (node == tree_top)
		{
			addNodes(node); // Re-read the top-level items
			treeModel.reload(node);
			return;
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
				treeModel.reload(node);
			}
		}
	}

	/* (non-Javadoc)
	 * @see javax.swing.event.TreeWillExpandListener#treeWillCollapse(javax.swing.event.TreeExpansionEvent)
	 */
	public void treeWillCollapse(TreeExpansionEvent arg0) throws ExpandVetoException {
		// Intentionally left blank
	}

	/**
	 * Comparator used to keep the items in the JTree sorted
	 */
	public class TreeSetComparator implements Comparator{

		/* (non-Javadoc)
		 * @see java.util.Comparator#compare(java.lang.Object, java.lang.Object)
		 */
		public int compare(Object arg0, Object arg1) {
			
			SimiasNode node0 = (SimiasNode) ((DefaultMutableTreeNode)arg0).getUserObject();
			SimiasNode node1 = (SimiasNode) ((DefaultMutableTreeNode)arg1).getUserObject();

			String name0 = node0.getName();
			String name1 = node1.getName();
			
			if (name0 != null && name1 != null)
			{
				return name0.compareToIgnoreCase(name1);
			}
			
			return 0;
		}
	}

	public class iFolderTreeCellRenderer extends DefaultTreeCellRenderer
	{
		public iFolderTreeCellRenderer()
		{
		}
		
		public Component getTreeCellRendererComponent(
							JTree tree,
							Object value,
							boolean sel,
							boolean expanded,
							boolean leaf,
							int row,
							boolean hasFocus)
		{
			super.getTreeCellRendererComponent(
					tree, value, sel, expanded, leaf, row, hasFocus);
			if (isType(value, "LocalDatabase"))
			{
				setIcon(localDatabaseIcon);
			}
			else if (isType(value, "Domain"))
			{
				setIcon(domainIcon);
			}
			else if (isType(value, "POBox"))
			{
				setIcon(poBoxIcon);
			}
			else if (isType(value, "iFolder"))
			{
				setIcon(iFolderIcon);
			}
			else if (isType(value, "Collection"))
			{
				setIcon(collectionIcon);
			}
			else if (isType(value, "Member"))
			{
				setIcon(memberIcon);
			}
			else if (isType(value, "Subscription"))
			{
				setIcon(subscriptionIcon);
			}
			else if (isType(value, "DirNode"))
			{
				setIcon(dirNodeIcon);
			}
			else if (isType(value, "FileNode"))
			{
				setIcon(fileNodeIcon);
			}
			else
			{
				setToolTipText(null);	// no tool tip
			}
			
			return this;
		}
		
		protected boolean isType(Object value, String type)
		{
			DefaultMutableTreeNode node = (DefaultMutableTreeNode)value;
			SimiasNode simiasNode = null;
			try
			{
				simiasNode = (SimiasNode)node.getUserObject();
			}
			catch (ClassCastException e)
			{
				return false;
			}

			return simiasNode.isType(type);
		}
	}
} //  @jve:visual-info  decl-index=0 visual-constraint="0,0"
