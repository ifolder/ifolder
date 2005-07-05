/**
 * ArrayOfBrowserShallowNode.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.2RC2 Nov 16, 2004 (12:19:44 EST) WSDL2Java emitter.
 */

package com.novell.simias.browser;

public class ArrayOfBrowserShallowNode  implements java.io.Serializable {
    private com.novell.simias.browser.BrowserShallowNode[] browserShallowNode;

    public ArrayOfBrowserShallowNode() {
    }

    public ArrayOfBrowserShallowNode(
           com.novell.simias.browser.BrowserShallowNode[] browserShallowNode) {
           this.browserShallowNode = browserShallowNode;
    }


    /**
     * Gets the browserShallowNode value for this ArrayOfBrowserShallowNode.
     * 
     * @return browserShallowNode
     */
    public com.novell.simias.browser.BrowserShallowNode[] getBrowserShallowNode() {
        return browserShallowNode;
    }


    /**
     * Sets the browserShallowNode value for this ArrayOfBrowserShallowNode.
     * 
     * @param browserShallowNode
     */
    public void setBrowserShallowNode(com.novell.simias.browser.BrowserShallowNode[] browserShallowNode) {
        this.browserShallowNode = browserShallowNode;
    }

    public com.novell.simias.browser.BrowserShallowNode getBrowserShallowNode(int i) {
        return this.browserShallowNode[i];
    }

    public void setBrowserShallowNode(int i, com.novell.simias.browser.BrowserShallowNode _value) {
        this.browserShallowNode[i] = _value;
    }

    private java.lang.Object __equalsCalc = null;
    public synchronized boolean equals(java.lang.Object obj) {
        if (!(obj instanceof ArrayOfBrowserShallowNode)) return false;
        ArrayOfBrowserShallowNode other = (ArrayOfBrowserShallowNode) obj;
        if (obj == null) return false;
        if (this == obj) return true;
        if (__equalsCalc != null) {
            return (__equalsCalc == obj);
        }
        __equalsCalc = obj;
        boolean _equals;
        _equals = true && 
            ((this.browserShallowNode==null && other.getBrowserShallowNode()==null) || 
             (this.browserShallowNode!=null &&
              java.util.Arrays.equals(this.browserShallowNode, other.getBrowserShallowNode())));
        __equalsCalc = null;
        return _equals;
    }

    private boolean __hashCodeCalc = false;
    public synchronized int hashCode() {
        if (__hashCodeCalc) {
            return 0;
        }
        __hashCodeCalc = true;
        int _hashCode = 1;
        if (getBrowserShallowNode() != null) {
            for (int i=0;
                 i<java.lang.reflect.Array.getLength(getBrowserShallowNode());
                 i++) {
                java.lang.Object obj = java.lang.reflect.Array.get(getBrowserShallowNode(), i);
                if (obj != null &&
                    !obj.getClass().isArray()) {
                    _hashCode += obj.hashCode();
                }
            }
        }
        __hashCodeCalc = false;
        return _hashCode;
    }

    // Type metadata
    private static org.apache.axis.description.TypeDesc typeDesc =
        new org.apache.axis.description.TypeDesc(ArrayOfBrowserShallowNode.class, true);

    static {
        typeDesc.setXmlType(new javax.xml.namespace.QName("http://novell.com/simias/browser", "ArrayOfBrowserShallowNode"));
        org.apache.axis.description.ElementDesc elemField = new org.apache.axis.description.ElementDesc();
        elemField.setFieldName("browserShallowNode");
        elemField.setXmlName(new javax.xml.namespace.QName("http://novell.com/simias/browser", "BrowserShallowNode"));
        elemField.setXmlType(new javax.xml.namespace.QName("http://novell.com/simias/browser", "BrowserShallowNode"));
        elemField.setMinOccurs(0);
        typeDesc.addFieldDesc(elemField);
    }

    /**
     * Return type metadata object
     */
    public static org.apache.axis.description.TypeDesc getTypeDesc() {
        return typeDesc;
    }

    /**
     * Get Custom Serializer
     */
    public static org.apache.axis.encoding.Serializer getSerializer(
           java.lang.String mechType, 
           java.lang.Class _javaType,  
           javax.xml.namespace.QName _xmlType) {
        return 
          new  org.apache.axis.encoding.ser.BeanSerializer(
            _javaType, _xmlType, typeDesc);
    }

    /**
     * Get Custom Deserializer
     */
    public static org.apache.axis.encoding.Deserializer getDeserializer(
           java.lang.String mechType, 
           java.lang.Class _javaType,  
           javax.xml.namespace.QName _xmlType) {
        return 
          new  org.apache.axis.encoding.ser.BeanDeserializer(
            _javaType, _xmlType, typeDesc);
    }

}
