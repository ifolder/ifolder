/**
 * Browser_x0020_ServiceSoap.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.2RC2 Nov 16, 2004 (12:19:44 EST) WSDL2Java emitter.
 */

package com.novell.simias.browser;

public interface Browser_x0020_ServiceSoap extends java.rmi.Remote {
    public com.novell.simias.browser.ArrayOfBrowserNode enumerateCollections() throws java.rmi.RemoteException;
    public com.novell.simias.browser.ArrayOfBrowserNode enumerateNodes(java.lang.String collectionID) throws java.rmi.RemoteException;
    public com.novell.simias.browser.BrowserNode getCollectionByID(java.lang.String collectionID) throws java.rmi.RemoteException;
    public com.novell.simias.browser.BrowserNode getNodeByID(java.lang.String collectionID, java.lang.String nodeID) throws java.rmi.RemoteException;
    public void modifyProperty(java.lang.String collectionID, java.lang.String nodeID, java.lang.String propertyName, java.lang.String propertyType, java.lang.String oldPropertyValue, java.lang.String newPropertyValue, org.apache.axis.types.UnsignedInt propertyFlags) throws java.rmi.RemoteException;
    public void addProperty(java.lang.String collectionID, java.lang.String nodeID, java.lang.String propertyName, java.lang.String propertyType, java.lang.String propertyValue, org.apache.axis.types.UnsignedInt propertyFlags) throws java.rmi.RemoteException;
    public void deleteProperty(java.lang.String collectionID, java.lang.String nodeID, java.lang.String propertyName, java.lang.String propertyType, java.lang.String propertyValue) throws java.rmi.RemoteException;
    public void deleteCollection(java.lang.String collectionID) throws java.rmi.RemoteException;
    public void deleteNode(java.lang.String collectionID, java.lang.String nodeID) throws java.rmi.RemoteException;
    public com.novell.simias.browser.ArrayOfBrowserShallowNode enumerateShallowCollections() throws java.rmi.RemoteException;
    public com.novell.simias.browser.ArrayOfBrowserShallowNode enumerateShallowNodes(java.lang.String collectionID) throws java.rmi.RemoteException;
    public java.lang.String getVersion() throws java.rmi.RemoteException;
}
