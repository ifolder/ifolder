// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 1.1.4322.573
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

// 
// This source code was auto-generated by Mono Web Services Description Language Utility
//

/// <remarks/>
/// <remarks>
///Web Service providing Syncronization to Simias
///</remarks>
[System.Web.Services.WebServiceBinding(Name="Simias Sync ServiceSoap",Namespace="http://novell.com/simias/sync/"),
System.Diagnostics.DebuggerStepThroughAttribute(),
System.ComponentModel.DesignerCategoryAttribute("code"),
System.Xml.Serialization.SoapInclude(typeof(SyncNodeStamp)),
System.Xml.Serialization.SoapInclude(typeof(HashData))]
public class SimiasSyncService : System.Web.Services.Protocols.SoapHttpClientProtocol {

    public SimiasSyncService () {
        this.Url = "http://localhost:8086/simias10/SyncService.asmx";
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Start",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNodeStamp[] Start(SyncStartInfo si, string user, out SyncStartInfo siout) {
        object[] results = this.Invoke("Start", new object[] {
            si,
            user});
        siout = ((SyncStartInfo)(results[1]));
        return ((SyncNodeStamp[])(results[0]));
    }

    public System.IAsyncResult BeginStart(SyncStartInfo si, string user, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Start", new object[] {
            si,
            user}, callback, asyncState);
    }

    public SyncNodeStamp[] EndStart(System.IAsyncResult asyncResult, out SyncStartInfo siout) {
        object[] results = this.EndInvoke(asyncResult);
        siout = ((SyncStartInfo)(results[1]));
        return ((SyncNodeStamp[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Stop",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public void Stop() {
        this.Invoke("Stop", new object[0]);
    }

    public System.IAsyncResult BeginStop(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Stop", new object[0], callback, asyncState);
    }

    public void EndStop(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/KeepAlive",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public bool KeepAlive() {
        object[] results = this.Invoke("KeepAlive", new object[0]);
        return ((bool)(results[0]));
    }

    public System.IAsyncResult BeginKeepAlive(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("KeepAlive", new object[0], callback, asyncState);
    }

    public bool EndKeepAlive(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Version",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public string Version() {
        object[] results = this.Invoke("Version", new object[0]);
        return ((string)(results[0]));
    }

    public System.IAsyncResult BeginVersion(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Version", new object[0], callback, asyncState);
    }

    public string EndVersion(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/PutNodes",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNodeStatus[] PutNodes(SyncNode[] nodes) {
        object[] results = this.Invoke("PutNodes", new object[] {
            nodes});
        return ((SyncNodeStatus[])(results[0]));
    }

    public System.IAsyncResult BeginPutNodes(SyncNode[] nodes, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutNodes", new object[] {
            nodes}, callback, asyncState);
    }

    public SyncNodeStatus[] EndPutNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/GetNodes",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNode[] GetNodes(string[] nids) {
        object[] results = this.Invoke("GetNodes", new object[] {
            nids});
        return ((SyncNode[])(results[0]));
    }

    public System.IAsyncResult BeginGetNodes(string[] nids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetNodes", new object[] {
            nids}, callback, asyncState);
    }

    public SyncNode[] EndGetNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/GetDirs",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNode[] GetDirs(string[] nids) {
        object[] results = this.Invoke("GetDirs", new object[] {
            nids});
        return ((SyncNode[])(results[0]));
    }

    public System.IAsyncResult BeginGetDirs(string[] nids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDirs", new object[] {
            nids}, callback, asyncState);
    }

    public SyncNode[] EndGetDirs(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/PutDirs",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNodeStatus[] PutDirs(SyncNode[] nodes) {
        object[] results = this.Invoke("PutDirs", new object[] {
            nodes});
        return ((SyncNodeStatus[])(results[0]));
    }

    public System.IAsyncResult BeginPutDirs(SyncNode[] nodes, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutDirs", new object[] {
            nodes}, callback, asyncState);
    }

    public SyncNodeStatus[] EndPutDirs(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/PutFileNode",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public bool PutFileNode(SyncNode node) {
        object[] results = this.Invoke("PutFileNode", new object[] {
            node});
        return ((bool)(results[0]));
    }

    public System.IAsyncResult BeginPutFileNode(SyncNode node, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutFileNode", new object[] {
            node}, callback, asyncState);
    }

    public bool EndPutFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/GetFileNode",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNode GetFileNode(string nodeID) {
        object[] results = this.Invoke("GetFileNode", new object[] {
            nodeID});
        return ((SyncNode)(results[0]));
    }

    public System.IAsyncResult BeginGetFileNode(string nodeID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetFileNode", new object[] {
            nodeID}, callback, asyncState);
    }

    public SyncNode EndGetFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode)(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/DeleteNodes",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNodeStatus[] DeleteNodes(string[] nodeIDs) {
        object[] results = this.Invoke("DeleteNodes", new object[] {
            nodeIDs});
        return ((SyncNodeStatus[])(results[0]));
    }

    public System.IAsyncResult BeginDeleteNodes(string[] nodeIDs, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteNodes", new object[] {
            nodeIDs}, callback, asyncState);
    }

    public SyncNodeStatus[] EndDeleteNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/GetHashMap",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public HashData[] GetHashMap(int blockSize) {
        object[] results = this.Invoke("GetHashMap", new object[] {
            blockSize});
        return ((HashData[])(results[0]));
    }

    public System.IAsyncResult BeginGetHashMap(int blockSize, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetHashMap", new object[] {
            blockSize}, callback, asyncState);
    }

    public HashData[] EndGetHashMap(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((HashData[])(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Write",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public void Write(byte[] buffer, long offset, int count) {
        this.Invoke("Write", new object[] {
            buffer,
            offset,
            count});
    }

    public System.IAsyncResult BeginWrite(byte[] buffer, long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Write", new object[] {
            buffer,
            offset,
            count}, callback, asyncState);
    }

    public void EndWrite(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Copy",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public void Copy(long oldOffset, long offset, int count) {
        this.Invoke("Copy", new object[] {
            oldOffset,
            offset,
            count});
    }

    public System.IAsyncResult BeginCopy(long oldOffset, long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Copy", new object[] {
            oldOffset,
            offset,
            count}, callback, asyncState);
    }

    public void EndCopy(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/Read",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public int Read(long offset, int count, out byte[] buffer) {
        object[] results = this.Invoke("Read", new object[] {
            offset,
            count});
        buffer = ((byte[])(results[1]));
        return ((int)(results[0]));
    }

    public System.IAsyncResult BeginRead(long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Read", new object[] {
            offset,
            count}, callback, asyncState);
    }

    public int EndRead(System.IAsyncResult asyncResult, out byte[] buffer) {
        object[] results = this.EndInvoke(asyncResult);
        buffer = ((byte[])(results[1]));
        return ((int)(results[0]));
    }

    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/simias/sync/CloseFileNode",RequestNamespace="http://novell.com/simias/sync/",ResponseNamespace="http://novell.com/simias/sync/")]
    public SyncNodeStatus CloseFileNode(bool commit) {
        object[] results = this.Invoke("CloseFileNode", new object[] {
            commit});
        return ((SyncNodeStatus)(results[0]));
    }

    public System.IAsyncResult BeginCloseFileNode(bool commit, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CloseFileNode", new object[] {
            commit}, callback, asyncState);
    }

    public SyncNodeStatus EndCloseFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public class SyncStartInfo  {

    /// <remarks/>
    public string CollectionID;

    /// <remarks/>
    public string Context;

    /// <remarks/>
    public bool ChangesOnly;

    /// <remarks/>
    public bool ClientHasChanges;

    /// <remarks/>
    public SyncColStatus Status;

    /// <remarks/>
    public Rights Access;
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public enum SyncColStatus  {

    /// <remarks/>
    Success,

    /// <remarks/>
    NoWork,

    /// <remarks/>
    NotFound,

    /// <remarks/>
    Busy,
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public enum Rights  {

    /// <remarks/>
    Deny,

    /// <remarks/>
    ReadOnly,

    /// <remarks/>
    ReadWrite,

    /// <remarks/>
    Admin,
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public class SyncNodeStamp  {

    /// <remarks/>
    public string ID;

    /// <remarks/>
    public ulong MasterIncarnation;

    /// <remarks/>
    public ulong LocalIncarnation;

    /// <remarks/>
    public string BaseType;

    /// <remarks/>
    public SyncOperation Operation;
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public enum SyncOperation  {

    /// <remarks/>
    Unknown,

    /// <remarks/>
    Create,

    /// <remarks/>
    Delete,

    /// <remarks/>
    Change,

    /// <remarks/>
    Rename,
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public class SyncNode  {

    /// <remarks/>
    public string nodeID;

    /// <remarks/>
    public string node;

    /// <remarks/>
    public ulong expectedIncarn;

    /// <remarks/>
    public SyncOperation operation;
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public class SyncNodeStatus  {

    /// <remarks/>
    public string nodeID;

    /// <remarks/>
    public SyncStatus status;
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public enum SyncStatus  {

    /// <remarks/>
    Success,

    /// <remarks/>
    UpdateConflict,

    /// <remarks/>
    FileNameConflict,

    /// <remarks/>
    ServerFailure,

    /// <remarks/>
    InProgess,

    /// <remarks/>
    InUse,

    /// <remarks/>
    Busy,
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/simias/sync/encodedTypes")]
public class HashData  {

    /// <remarks/>
    public int BlockNumber;

    /// <remarks/>
    public uint WeakHash;

    /// <remarks/>
    public byte[] StrongHash;
}

