﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=1.1.4322.2032.
// 
using System.Diagnostics;
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;


/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="DomainServiceSoap", Namespace="http://tempuri.org/")]
public class DomainService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public DomainService() {
        this.Url = "http://localhost:8086/DomainService.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetDomainInfo", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public DomainInfo GetDomainInfo() {
        object[] results = this.Invoke("GetDomainInfo", new object[0]);
        return ((DomainInfo)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDomainInfo(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDomainInfo", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public DomainInfo EndGetDomainInfo(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((DomainInfo)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/ProvisionUser", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public ProvisionInfo ProvisionUser(string user, string password) {
        object[] results = this.Invoke("ProvisionUser", new object[] {
                    user,
                    password});
        return ((ProvisionInfo)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginProvisionUser(string user, string password, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("ProvisionUser", new object[] {
                    user,
                    password}, callback, asyncState);
    }
    
    /// <remarks/>
    public ProvisionInfo EndProvisionUser(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((ProvisionInfo)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/CreateMaster", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string CreateMaster(string id, string name, string rootID, string rootName, string user) {
        object[] results = this.Invoke("CreateMaster", new object[] {
                    id,
                    name,
                    rootID,
                    rootName,
                    user});
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCreateMaster(string id, string name, string rootID, string rootName, string user, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CreateMaster", new object[] {
                    id,
                    name,
                    rootID,
                    rootName,
                    user}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndCreateMaster(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
public class DomainInfo {
    
    /// <remarks/>
    public string Name;
    
    /// <remarks/>
    public string Description;
    
    /// <remarks/>
    public string ID;
    
    /// <remarks/>
    public string RosterID;
    
    /// <remarks/>
    public string RosterName;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
public class ProvisionInfo {
    
    /// <remarks/>
    public string UserID;
    
    /// <remarks/>
    public string POBoxID;
    
    /// <remarks/>
    public string POBoxName;
}
