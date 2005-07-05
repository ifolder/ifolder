/**
 * Browser_x0020_ServiceLocator.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.2RC2 Nov 16, 2004 (12:19:44 EST) WSDL2Java emitter.
 */

package com.novell.simias.browser;

public class Browser_x0020_ServiceLocator extends org.apache.axis.client.Service implements com.novell.simias.browser.Browser_x0020_Service {

/**
 * Web Service providing access to the simias database.
 */

    public Browser_x0020_ServiceLocator() {
    }


    public Browser_x0020_ServiceLocator(org.apache.axis.EngineConfiguration config) {
        super(config);
    }

    // Use to get a proxy class for Browser_x0020_ServiceSoap
    private java.lang.String Browser_x0020_ServiceSoap_address = "http://localhost:8086/simias10/SimiasBrowser.asmx";

    public java.lang.String getBrowser_x0020_ServiceSoapAddress() {
        return Browser_x0020_ServiceSoap_address;
    }

    // The WSDD service name defaults to the port name.
    private java.lang.String Browser_x0020_ServiceSoapWSDDServiceName = "Browser_x0020_ServiceSoap";

    public java.lang.String getBrowser_x0020_ServiceSoapWSDDServiceName() {
        return Browser_x0020_ServiceSoapWSDDServiceName;
    }

    public void setBrowser_x0020_ServiceSoapWSDDServiceName(java.lang.String name) {
        Browser_x0020_ServiceSoapWSDDServiceName = name;
    }

    public com.novell.simias.browser.Browser_x0020_ServiceSoap getBrowser_x0020_ServiceSoap() throws javax.xml.rpc.ServiceException {
       java.net.URL endpoint;
        try {
            endpoint = new java.net.URL(Browser_x0020_ServiceSoap_address);
        }
        catch (java.net.MalformedURLException e) {
            throw new javax.xml.rpc.ServiceException(e);
        }
        return getBrowser_x0020_ServiceSoap(endpoint);
    }

    public com.novell.simias.browser.Browser_x0020_ServiceSoap getBrowser_x0020_ServiceSoap(java.net.URL portAddress) throws javax.xml.rpc.ServiceException {
        try {
            com.novell.simias.browser.Browser_x0020_ServiceSoapStub _stub = new com.novell.simias.browser.Browser_x0020_ServiceSoapStub(portAddress, this);
            _stub.setPortName(getBrowser_x0020_ServiceSoapWSDDServiceName());
            return _stub;
        }
        catch (org.apache.axis.AxisFault e) {
            return null;
        }
    }

    public void setBrowser_x0020_ServiceSoapEndpointAddress(java.lang.String address) {
        Browser_x0020_ServiceSoap_address = address;
    }

    /**
     * For the given interface, get the stub implementation.
     * If this service has no port for the given interface,
     * then ServiceException is thrown.
     */
    public java.rmi.Remote getPort(Class serviceEndpointInterface) throws javax.xml.rpc.ServiceException {
        try {
            if (com.novell.simias.browser.Browser_x0020_ServiceSoap.class.isAssignableFrom(serviceEndpointInterface)) {
                com.novell.simias.browser.Browser_x0020_ServiceSoapStub _stub = new com.novell.simias.browser.Browser_x0020_ServiceSoapStub(new java.net.URL(Browser_x0020_ServiceSoap_address), this);
                _stub.setPortName(getBrowser_x0020_ServiceSoapWSDDServiceName());
                return _stub;
            }
        }
        catch (java.lang.Throwable t) {
            throw new javax.xml.rpc.ServiceException(t);
        }
        throw new javax.xml.rpc.ServiceException("There is no stub implementation for the interface:  " + (serviceEndpointInterface == null ? "null" : serviceEndpointInterface.getName()));
    }

    /**
     * For the given interface, get the stub implementation.
     * If this service has no port for the given interface,
     * then ServiceException is thrown.
     */
    public java.rmi.Remote getPort(javax.xml.namespace.QName portName, Class serviceEndpointInterface) throws javax.xml.rpc.ServiceException {
        if (portName == null) {
            return getPort(serviceEndpointInterface);
        }
        java.lang.String inputPortName = portName.getLocalPart();
        if ("Browser_x0020_ServiceSoap".equals(inputPortName)) {
            return getBrowser_x0020_ServiceSoap();
        }
        else  {
            java.rmi.Remote _stub = getPort(serviceEndpointInterface);
            ((org.apache.axis.client.Stub) _stub).setPortName(portName);
            return _stub;
        }
    }

    public javax.xml.namespace.QName getServiceName() {
        return new javax.xml.namespace.QName("http://novell.com/simias/browser", "Browser_x0020_Service");
    }

    private java.util.HashSet ports = null;

    public java.util.Iterator getPorts() {
        if (ports == null) {
            ports = new java.util.HashSet();
            ports.add(new javax.xml.namespace.QName("http://novell.com/simias/browser", "Browser_x0020_ServiceSoap"));
        }
        return ports.iterator();
    }

    /**
    * Set the endpoint address for the specified port name.
    */
    public void setEndpointAddress(java.lang.String portName, java.lang.String address) throws javax.xml.rpc.ServiceException {
        if ("Browser_x0020_ServiceSoap".equals(portName)) {
            setBrowser_x0020_ServiceSoapEndpointAddress(address);
        }
        else { // Unknown Port Name
            throw new javax.xml.rpc.ServiceException(" Cannot set Endpoint Address for Unknown Port" + portName);
        }
    }

    /**
    * Set the endpoint address for the specified port name.
    */
    public void setEndpointAddress(javax.xml.namespace.QName portName, java.lang.String address) throws javax.xml.rpc.ServiceException {
        setEndpointAddress(portName.getLocalPart(), address);
    }

}
