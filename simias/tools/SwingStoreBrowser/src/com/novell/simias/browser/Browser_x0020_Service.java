/**
 * Browser_x0020_Service.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.2RC2 Nov 16, 2004 (12:19:44 EST) WSDL2Java emitter.
 */

package com.novell.simias.browser;

public interface Browser_x0020_Service extends javax.xml.rpc.Service {

/**
 * Web Service providing access to the simias database.
 */
    public java.lang.String getBrowser_x0020_ServiceSoapAddress();

    public com.novell.simias.browser.Browser_x0020_ServiceSoap getBrowser_x0020_ServiceSoap() throws javax.xml.rpc.ServiceException;

    public com.novell.simias.browser.Browser_x0020_ServiceSoap getBrowser_x0020_ServiceSoap(java.net.URL portAddress) throws javax.xml.rpc.ServiceException;
}
