// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 1.1.4322.573
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------


/// <remarks/>
[System.Web.Services.WebServiceBinding(Name="POBoxServiceSoap",Namespace="http://novell.com/simias/pobox/"),
System.Diagnostics.DebuggerStepThroughAttribute(),
System.ComponentModel.DesignerCategoryAttribute("code")]
public class POBoxService : System.Web.Services.Protocols.SoapHttpClientProtocol {

    public POBoxService () {
        this.Url = "http://137.65.58.216:8086/POBoxService.asmx";
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/Ping",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual int Ping(int sleepFor) {
        System.Object[] results = this.Invoke("Ping", new object[] {
            sleepFor});
        return ((int)(results[0]));
    }

    public virtual System.IAsyncResult BeginPing(int sleepFor, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Ping", new object[] {
            sleepFor}, callback, asyncState);
    }

    public virtual int EndPing(System.IAsyncResult asyncResult) {
        System.Object[] results = this.EndInvoke(asyncResult);
        return ((int)(results[0]));
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/AcceptSubscription",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual void AcceptSubscription(string domainID, string identityID, string subscriptionID) {
        this.Invoke("AcceptSubscription", new object[] {
            domainID,
            identityID,
            subscriptionID});
    }

    public virtual System.IAsyncResult BeginAcceptSubscription(string domainID, string identityID, string subscriptionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AcceptSubscription", new object[] {
            domainID,
            identityID,
            subscriptionID}, callback, asyncState);
    }

    public virtual void EndAcceptSubscription(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/DeclineSubscription",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual void DeclineSubscription(string domainID, string identityID, string subscriptionID) {
        this.Invoke("DeclineSubscription", new object[] {
            domainID,
            identityID,
            subscriptionID});
    }

    public virtual System.IAsyncResult BeginDeclineSubscription(string domainID, string identityID, string subscriptionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeclineSubscription", new object[] {
            domainID,
            identityID,
            subscriptionID}, callback, asyncState);
    }

    public virtual void EndDeclineSubscription(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/AckSubscription",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual void AckSubscription(string domainID, string identityID, string messageID) {
        this.Invoke("AckSubscription", new object[] {
            domainID,
            identityID,
            messageID});
    }

    public virtual System.IAsyncResult BeginAckSubscription(string domainID, string identityID, string messageID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AckSubscription", new object[] {
            domainID,
            identityID,
            messageID}, callback, asyncState);
    }

    public virtual void EndAckSubscription(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/GetSubscriptionInfo",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual SubscriptionInformation GetSubscriptionInfo(string domainID, string identityID, string messageID) {
        System.Object[] results = this.Invoke("GetSubscriptionInfo", new object[] {
            domainID,
            identityID,
            messageID});
        return ((SubscriptionInformation)(results[0]));
    }

    public virtual System.IAsyncResult BeginGetSubscriptionInfo(string domainID, string identityID, string messageID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetSubscriptionInfo", new object[] {
            domainID,
            identityID,
            messageID}, callback, asyncState);
    }

    public virtual SubscriptionInformation EndGetSubscriptionInfo(System.IAsyncResult asyncResult) {
        System.Object[] results = this.EndInvoke(asyncResult);
        return ((SubscriptionInformation)(results[0]));
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/Invite",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual string Invite(string domainID, string fromUserID, string toUserID, string sharedCollectionID, string sharedCollectionType) {
        System.Object[] results = this.Invoke("Invite", new object[] {
            domainID,
            fromUserID,
            toUserID,
            sharedCollectionID,
            sharedCollectionType});
        return ((string)(results[0]));
    }

    public virtual System.IAsyncResult BeginInvite(string domainID, string fromUserID, string toUserID, string sharedCollectionID, string sharedCollectionType, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Invite", new object[] {
            domainID,
            fromUserID,
            toUserID,
            sharedCollectionID,
            sharedCollectionType}, callback, asyncState);
    }

    public virtual string EndInvite(System.IAsyncResult asyncResult) {
        System.Object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/Subscribe",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual string Subscribe(string domainID, string fromUserID, string fromUserAlias, string fromUserPubKey, string toUserName, string toUserID, string collectionID, string subscriptionName) {
        System.Object[] results = this.Invoke("Subscribe", new object[] {
            domainID,
            fromUserID,
            fromUserAlias,
            fromUserPubKey,
            toUserName,
            toUserID,
            collectionID,
            subscriptionName});
        return ((string)(results[0]));
    }

    public virtual System.IAsyncResult BeginSubscribe(string domainID, string fromUserID, string fromUserAlias, string fromUserPubKey, string toUserName, string toUserID, string collectionID, string subscriptionName, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Subscribe", new object[] {
            domainID,
            fromUserID,
            fromUserAlias,
            fromUserPubKey,
            toUserName,
            toUserID,
            collectionID,
            subscriptionName}, callback, asyncState);
    }

    public virtual string EndSubscribe(System.IAsyncResult asyncResult) {
        System.Object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/GetDefaultDomain",RequestNamespace="http://novell.com/simias/pobox/",ResponseNamespace="http://novell.com/simias/pobox/",ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped,Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public virtual string GetDefaultDomain(int dummy) {
        System.Object[] results = this.Invoke("GetDefaultDomain", new object[] {
            dummy});
        return ((string)(results[0]));
    }

    public virtual System.IAsyncResult BeginGetDefaultDomain(int dummy, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDefaultDomain", new object[] {
            dummy}, callback, asyncState);
    }

    public virtual string EndGetDefaultDomain(System.IAsyncResult asyncResult) {
        System.Object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlType(Namespace="http://novell.com/simias/pobox/")]
public class SubscriptionInformation {

    /// <remarks/>
    public string MsgID;

    /// <remarks/>
    public string FromID;

    /// <remarks/>
    public string FromName;

    /// <remarks/>
    public string ToID;

    /// <remarks/>
    public string ToName;

    /// <remarks/>
    public string CollectionID;

    /// <remarks/>
    public string CollectionName;

    /// <remarks/>
    public string CollectionType;

    /// <remarks/>
    public string CollectionUrl;

    /// <remarks/>
    public string DirNodeID;

    /// <remarks/>
    public string DirNodeName;

    /// <remarks/>
    public string DomainID;

    /// <remarks/>
    public string DomainName;

    /// <remarks/>
    public int State;

    /// <remarks/>
    public int Disposition;
}

