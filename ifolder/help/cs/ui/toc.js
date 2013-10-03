function getDHTMLObjHeight(_1){
return parseInt(_1.offsetHeight,10);
}
function getPixelsFromTop(_2){
var _3=(theBrowser.code=="MSIE")?_2.pixelTop:_2.top;
if(typeof (_3)=="string"){
_3=parseInt(_3);
}
return _3;
}
function simpleArray(){
this.item=0;
}
function imgStoreItem(n,s,w,h){
this.name=n;
this.src=s;
this.obj=null;
this.w=w;
this.h=h;
if((theBrowser.canCache)&&(s)){
this.obj=new Image(w,h);
this.obj.src=s;
}
}
function imgStoreObject(){
this.count=-1;
this.img=new imgStoreItem;
this.find=imgStoreFind;
this.add=imgStoreAdd;
this.getSrc=imgStoreGetSrc;
this.getTag=imgStoreGetTag;
}
function imgStoreFind(_8){
var _9=-1;
for(var i=0;i<=this.count;i++){
if(this.img[i].name==_8){
_9=i;
break;
}
}
return _9;
}
function imgStoreAdd(n,s,w,h){
var i=this.find(n);
if(i==-1){
i=++this.count;
}
this.img[i]=new imgStoreItem(n,s,parseInt(w,10),parseInt(h,10));
}
function imgStoreGetSrc(_10){
var i=this.find(_10);
var img=this.img[i];
return (i==-1)?"":img.src;
}
function imgStoreGetTag(_13,_14,_15){
var tag="";
var i=this.find(_13);
if(i<0){
return "";
}
with(this.img[i]){
if(src!=""){
tag="<img src=\""+src+"\"";
tag+=(_14!="")?" id=\""+_14+"\"":"";
tag+=" alt=\""+((_15)?_15:"")+"\" />";
}
}
return tag;
}
function TocItem(_18,_19,_1a,_1b,_1c,_1d,_1e,_1f,_20,_21){
var t=this;
this.owner=_18;
this.isLeaf=_1a;
this.type=(_1a==true)?"Document":"Folder";
this.text=_1b;
this.topicId=_1c;
this.parentTopicId=_1d;
this.depth=_21;
this.topicUrl="../"+_1c+"."+CFG_HTML_EXT;
this.thisIndex=_19;
this.prevEntry=null;
this.nextEntry=null;
this.parent=_20;
this.firstChild=null;
this.prevSibling=_1f;
this.nextSibling=_1e;
this.isopen=(_18==-1)?true:false;
this.isSelected=false;
this.isDrawn=false;
this.draw=MIDraw;
this.getDomNode=MIGetDomNode;
this.getInnerHTML=MIGetInnerHTML;
this.PMIconName=MIGetPMIconName;
this.docIconName=MIGetDocIconName;
this.setImg=MISetImage;
this.setIsOpen=MISetIsOpen;
this.setSelected=MISetSelected;
this.setIcon=MISetIcon;
this.mouseOver=MIMouseOver;
this.mouseOut=MIMouseOut;
var i=(this.owner.imgStore)?this.owner.imgStore.find(this.type):-2;
if(i==-1){
i=this.owner.imgStore.find("iconPlus");
}
this.height=(i>-1)?this.owner.imgStore.img[i].h:0;
}
function MIGetDomNode(){
return (this.isDrawn==true)?dom_getEl(document,"entryDIV"+this.thisIndex):null;
}
function MIGetInnerHTML(){
var o=this.owner;
var _25="=\"return "+o.name;
var tmp=_25+".entries["+this.thisIndex+"].";
var _27=" onmouseover"+tmp+"mouseOver('";
var _28=" onmouseout"+tmp+"mouseOut('";
var _29=o.imgStore.getTag(this.PMIconName(),"plusMinusIcon"+this.thisIndex,"");
var _2a="";
if(this.isLeaf==false){
_2a+="<a href=\"#\" onclick"+_25+".toggleExpand("+this.thisIndex+",event);\""+_27+"plusMinusIcon',this);\""+_28+"plusMinusIcon');\">"+_29+"</a>";
}else{
_2a+=_29;
}
var tip=this.text;
var _2c=o.imgStore.getTag(this.docIconName(),"docIcon"+this.thisIndex,tip)+this.text;
var _2d=o.imgStore.getTag(this.docIconName(),"docIcon"+this.thisIndex,tip);
var _2e="<span class=\""+((this.isLeaf==false)?"node":"leaf")+"\">";
var _2f="<a id=\"tocentry"+this.thisIndex+"\" ";
if(this.topicUrl!=""){
_2f+=" href=\""+this.topicUrl+"\" onclick"+_25+".itemClicked("+this.thisIndex+",event);\""+_27+"docIcon',this);\""+_28+"docIcon');\"";
}
_2f+=(tip)?" title=\""+tip+"\">":">";
_2a+=_2e+_2f+_2d;
_2a+=this.text+"</a></span>";
return _2a;
}
function MIGetPMIconName(){
var n="icon"+((this.isLeaf==false)?((this.isopen==true)?"Minus":"Plus"):"Join");
n+=(this==this.owner.firstEntry)?((this.nextSibling==null)?"Only":"Top"):((this.nextSibling==null)?"Bottom":"");
return n;
}
function MIGetDocIconName(){
var is=this.owner.imgStore;
var n=this.type;
n+=((this.isopen)&&(is.getSrc(n+"Expanded")!=""))?"Expanded":"";
n+=((this.isSelected)&&(is.getSrc(n+"Selected")!=""))?"Selected":"";
return n;
}
function MISetImage(_33,_34){
var o=this.owner;
var s=o.imgStore.getSrc(_34);
if((s!="")&&(theBrowser.canCache)&&(!o.isBusy)){
var img=dom_getEl(document,_33);
if(img&&img.src!=s){
img.src=s;
}
}
}
function MISetIsOpen(_38){
if((this.isopen!=_38)&&(this.isLeaf==false)){
this.isopen=_38;
this.setImg("plusMinusIcon"+this.thisIndex,this.PMIconName());
this.setImg("docIcon"+this.thisIndex,this.docIconName());
return true;
}else{
return false;
}
}
function MISetSelected(_39){
this.isSelected=_39;
var s=dom_getEl(document,"tocentry"+this.thisIndex);
if(s&&s.style){
if(_39){
s.style.color=(CFG_HELP_TYPE=="tablethtml")?"#a40000":"#cc0000";
s.style.fontWeight="bold";
}else{
s.style.color="#333333";
s.style.fontWeight="normal";
}
}
this.setImg("docIcon"+this.thisIndex,this.docIconName());
if((this.parent!=null)&&this.owner.selectParents){
this.parent.setSelected(_39);
}
}
function MISetIcon(_3b){
this.type=_3b;
this.setImg("docIcon"+this.thisIndex,this.docIconName());
}
function MIMouseOver(_3c,_3d){
var _3e="";
var s="";
if(_3c=="plusMinusIcon"){
_3e=this.PMIconName();
s="Click to "+((this.isopen==true)?"collapse.":"expand.");
}else{
if(_3c=="docIcon"){
_3e=this.docIconName();
}
}
if(theBrowser.canOnMouseOut){
this.setImg(_3c+this.thisIndex,_3e+"MouseOver");
}
if(this.onMouseOver){
var me=this;
eval(me.onMouseOver);
}
return true;
}
function MIMouseOut(_41){
var _42="";
if(_41=="plusMinusIcon"){
_42=this.PMIconName();
}else{
if(_41=="docIcon"){
_42=this.docIconName();
}
}
this.setImg(_41+this.thisIndex,_42);
if(this.onMouseOut){
var me=this;
eval(me.onMouseOut);
}
return true;
}
function Toc(){
this.count=-1;
this.size=0;
this.firstEntry=null;
this.autoScrolling=true;
this.toggleOnLink=false;
this.name="theToc";
this.container="leftnav";
this.contentFrame="topic";
this.selectParents=false;
this.lastPMClicked=-1;
this.selectedEntry=-1;
this.isBusy=true;
this.isVisible=true;
this.imgStore=new imgStoreObject;
this.entries=new Array();
this.getEmptyEntryIndex=TocGetEmptyEntryIndex;
this.addEntry=TocAddEntry;
this.addToc=TocAddEntry;
this.addChild=TocAddChild;
this.refresh=TocRefresh;
this.scrollTo=TocScrollTo;
this.contentDiv=null;
this.syncWithPage=TocSyncWithPage;
this.itemClicked=TocItemClicked;
this.selectEntry=TocSelectEntry;
this.setEntry=TocSetEntry;
this.findEntry=TocFindEntry;
this.toggleExpand=TocToggleExpand;
this.load=TocLoad;
this.addLevel=TocAddLevel;
initOutlineIcons(this.imgStore);
}
function TocGetEmptyEntryIndex(){
for(var i=0;i<=this.count;i++){
if(this.entries[i]==null){
break;
}
}
if(i>this.count){
this.count=i;
}
return i;
}
function TocAddEntry(_45,_46,_47,_48,_49,_4a,_4b){
var _4c=this.getEmptyEntryIndex();
var _4d=new TocItem(this,_4c,_47,_48,_49,_4a,null,_46,_45,_4b);
this.entries[_4c]=_4d;
if(_46!=null){
_46.nextSibling=_4d;
}
if(_45!=null){
if(_46==null){
_45.firstChild=_4d;
}
}else{
this.firstEntry=_4d;
}
_4d.prevEntry=(_46!=null)?_46:_45;
if(_4d.prevEntry!=null){
_4d.nextEntry=_4d.prevEntry.nextEntry;
_4d.prevEntry.nextEntry=_4d;
}
if(_4d.nextEntry!=null){
_4d.nextEntry.prevEntry=_4d;
}
_4d.draw(this.contentDiv);
this.size++;
return _4d;
}
function TocAddChild(_4e,_4f,_50,_51,_52,_53){
var _54=null;
var _55=null;
if(_4e!=null){
_55=_4e.firstChild;
if(_55!=null){
while(_55.nextSibling!=null){
_55=_55.nextSibling;
}
}
}
return this.addEntry(_4e,_55,_4f,_50,_51,_52,_53);
}
function TocSyncWithPage(_56){
if(!this.Busy){
if(_56){
var eID=this.findEntry(_56);
if(eID>=0){
this.selectEntry(eID);
this.setEntry(eID,true);
if(this.autoScrolling){
this.refresh();
this.scrollTo(eID);
}
}
}
}
}
function TocRefresh(){
if(this.Busy==true){
return;
}
var _58=17;
var _59=new simpleArray;
var _5a=this.firstEntry;
var _5b=(_5a==null)?0:1;
var _5c=true;
var _5d=1;
var _5e=dom_getEl(document,"tocTop");
var _5f=_5e?_5e.offsetHeight:0;
var _60=(CFG_HELP_TYPE=="tablethtml")?9:0;
var _61=null;
while(_5b>0){
_61=_5a.getDomNode();
if(_5c){
if(CFG_RTL_TEXT==true){
_61.style.right=(_5a.depth*_58)+_60+"px";
}else{
_61.style.left=(_5a.depth*_58)+_60+"px";
}
_61.style.top=_5f+"px";
_61.style.visibility="visible";
_5f+=getDHTMLObjHeight(_61);
_5d=_5b;
}else{
_61.style.visibility="hidden";
_61.style.top="0px";
}
if(_5a.firstChild!=null){
_5c=(_5a.isopen==true)&&_5c;
_59[_5b++]=_5a.nextSibling;
_5a=_5a.firstChild;
}else{
if(_5a.nextSibling!=null){
_5a=_5a.nextSibling;
}else{
while(_5b>0){
if(_59[--_5b]!=null){
_5a=_59[_5b];
_5c=(_5d>=_5b);
break;
}
}
}
}
}
}
function MIDraw(_62){
var _63=document.createElement("div");
_63.id="entryDIV"+this.thisIndex;
_63.className="tocItem";
_63.innerHTML=this.getInnerHTML();
if(this.nextEntry!=null&&this.nextEntry.isDrawn==true){
_62.insertBefore(_63,this.nextEntry.getDomNode());
}else{
_62.appendChild(_63);
}
this.isDrawn=true;
return _63;
}
function TocScrollTo(_64){
var e=this.entries[_64];
if(!e){
return;
}
var _66=getPixelsFromTop(document.getElementById("entryDIV"+_64).style);
var _67=_66+18;
var _68=dom_getScrollY();
var _69=_68+dom_getClientHeight();
if((_67>_69-20)||(_66<_68+30)){
var _6a=_67-_69+(_69-_68)/2;
if(_66<(_68+_6a)){
_6a=_66-_68-(_69-_68)/2;
}
window.scrollBy(0,_6a);
}
}
function TocItemClicked(_6b,_6c,_6d){
var r=true;
var e=this.entries[_6b];
var b=theBrowser;
if(!_6d){
this.selectEntry(_6b);
}
if(e.onClickFunc){
e.onClick=e.onClickFunc;
}
if(e.onClick){
var me=e;
if(eval(me.onClick)==false){
r=false;
}
}
if(r){
if(this.toggleOnLink&&(e.isLeaf==false)&&!_6d){
this.toggleExpand(_6b,_6c,true);
}
}
return (e.topicUrl!="")?r:false;
}
function TocSelectEntry(_72){
var oe=this.entries[this.selectedEntry];
if(oe){
oe.setSelected(false);
}
var e=this.entries[_72];
if(e){
e.setSelected(true);
}
this.selectedEntry=_72;
}
function TocSetEntry(_75,_76){
var cl=","+_75+",";
var e=this.entries[_75];
this.lastPMClicked=_75;
var mc=e.setIsOpen(_76);
var p=e.parent;
while(p!=null){
cl+=p.thisIndex+",";
mc|=(p.setIsOpen(true));
p=p.parent;
}
return mc;
}
function TocFindEntry(_7b,_7c,_7d,_7e){
var e;
var sf;
if(_7b==""){
return -1;
}
if(!_7c){
_7c="topicId";
}
if(!_7d){
_7d="exact";
}
if(!_7e){
_7e=0;
}
if(_7c=="URL"||_7c=="url"){
_7c="topicUrl";
}
if(_7c=="title"){
_7c="text";
}
eval("sf = cmp_"+_7d);
for(var i=_7e;i<=this.count;i++){
if(this.entries[i]){
e=this.entries[i];
if(sf(eval("e."+_7c),_7b)){
return i;
}
}
}
return -1;
}
function cmp_exact(c,s){
return (c==s);
}
function cmp_left(c,s){
var l=Math.min(c.length,s.length);
return ((c.substring(1,l)==s.substring(1,l))&&(c!=""));
}
function cmp_right(c,s){
c=c.replace(/\.\./,"");
c=c.replace(/\\/g,"/");
s=s.replace(/\\/g,"/");
var l=Math.min(c.length,s.length);
return ((c.substring(c.length-l)==s.substring(s.length-l))&&(c!=""));
}
function cmp_contains(c,s){
return (c.indexOf(s)>=0);
}
function TocToggleExpand(_8c,e,_8e,_8f){
var e=e||event;
var _90=e.shiftKey;
var _91=this.entries[_8c];
if(_91.isLeaf==false){
if(typeof (_8f)=="undefined"){
_8f=_91.isopen^1;
}
if(_91.firstChild==null&&_8f){
this.load(_91.topicId,_91.parentTopicId);
}
if(_90&&_91.firstChild){
var _92=_91.firstChild;
while(_92){
this.toggleExpand(_92.thisIndex,e,_8e,_8f);
_92=_92.nextSibling;
}
}
var chg=this.setEntry(_8c,_8f);
if(chg){
this.refresh();
}
}
return false;
}
function TocAddLevel(_94,_95){
if(!_94||_94.firstChild==null){
var _96=null;
var _97=0;
var _98=_95.attributes.getNamedItem("id").nodeValue;
if(!_94){
var _99=_95.getElementsByTagName("title");
if(_99.length){
var _9a=_99[0].childNodes[0].nodeValue;
_96=this.addChild(_96,false,_9a,_98,null,_97);
}
}else{
_96=_94;
_97=_94.depth;
}
var _9b=_95.getElementsByTagName("entry");
for(var i=0;i<_9b.length;i++){
var _9d=_9b[i];
var _9a=_9d.childNodes[0].nodeValue;
var _9e=(_9d.attributes.getNamedItem("leaf").nodeValue=="true")?true:false;
var _9f=_9d.attributes.getNamedItem("id").nodeValue;
this.addChild(_96,_9e,_9a,_9f,_98,_97+1);
}
}else{
}
}
function TocLoad(_a0,_a1,_a2){
var _a3=true;
var _a4=false;
var _a5=null;
var _a6=(_a0!=null)?this.findEntry(_a0):-1;
if(_a6==-1){
if(_a1){
_a3=this.load(_a1,null,_a2);
if(_a3==true){
_a6=this.findEntry(_a0);
_a5=this.entries[_a6];
_a4=_a5.isLeaf==false?true:false;
}
}else{
_a4=true;
}
}else{
_a5=this.entries[_a6];
if(_a5.isLeaf==false&&_a5.firstChild==null){
_a4=true;
}
}
if(_a4==true){
var _a7=dom_LoadXMLDoc("toc_"+_a0+".xml");
if(_a7==null){
_a7=dom_LoadXMLDoc("ui/toc_"+_a0+".xml");
}
if(_a7){
var _a8=_a7.getElementsByTagName("tocsection");
if(_a8.length==1){
var _a9=_a8[0];
var _aa=_a9.attributes.getNamedItem("parent");
var _ab=null;
if(_aa&&_aa.nodeValue.length){
var _ac=this.findEntry(_aa.nodeValue);
_ab=(_ac!=-1)?this.entries[_ac]:null;
}
if(_aa&&_aa.nodeValue.length&&(_ab==null||_ab.firstChild==null)){
_a3=this.load(_aa.nodeValue,null,_a2);
if(_a3==true){
_a6=this.findEntry(_a0);
_a5=this.entries[_a6];
}
}
this.addLevel(_a5,_a9);
_a3=true;
}else{
_a3=false;
}
}else{
_a3=false;
}
}
return _a3;
}
function browserInfo(){
this.code="unknown";
this.version=0;
this.platform="Win";
var ua=navigator.userAgent;
var i=ua.indexOf("WebTV");
if(i>=0){
this.code="WebTV";
i+=6;
}else{
i=ua.indexOf("Opera");
if(i>=0){
this.code="OP";
i=ua.indexOf(") ")+2;
}else{
i=ua.indexOf("MSIE");
if(i>=0){
this.code="MSIE";
i+=5;
}else{
i=ua.indexOf("Mozilla/");
if(i>=0){
this.code="NS";
i+=8;
}
}
}
}
this.version=parseFloat(ua.substring(i,i+4));
if(ua.indexOf("Mac")>=0){
this.platform="Mac";
}
if(ua.indexOf("OS/2")>=0){
this.platform="OS/2";
}
if(ua.indexOf("X11")>=0){
this.platform="UNIX";
}
var xx=navigator.appName;
var v=this.version;
var p=this.platform;
var NS=(this.code=="NS");
var IE=(this.code=="MSIE");
var WTV=(this.code=="WebTV");
var OP=(this.code=="OP");
var _b6=(OP&&(v>=3.2));
var _b7=(OP&&(v>=5));
var _b8=(IE&&(v>=4));
var _b9=(NS&&(v>=3));
var _ba=(NS&&(v>=5));
this.NWCode=((xx.indexOf("Netware")>=0)||(ua.indexOf("ICEBrowser")>=0));
this.canCache=_b9||_b8||_b6||WTV;
this.canOnMouseOut=this.canCache;
this.canOnError=_b9||_b8||_b6;
this.canJSVoid=!((NS&&!_b9)||(IE&&!_b8)||(OP&&(v<3.5)));
this.hasDHTML=((NS||IE)&&(v>=4))&&!(IE&&(p=="Mac")&&(v<4.5))||this.NWCode;
this.hasW3CDOM=(document.getElementById)?true:false;
this.DHTMLRange=IE?".all":"";
this.DHTMLStyleObj=IE?".style":"";
this.DHTMLDivHeight=IE?".offsetHeight":".clip.height";
}
function defOnError(msg,_bc,lno){
if(jsErrorMsg==""){
return false;
}else{
alert(jsErrorMsg+".\n\nError: "+msg+"\nPage: "+_bc+"\nLine: "+lno+"\nBrowser: "+navigator.userAgent);
return true;
}
}
function initOutlineIcons(_be){
var _bf=18;
var ip=CFG_BOOKUI2SHAREDUI_RELPATH+"images/";
var _c1=(CFG_HELP_TYPE=="tablethtml")?"t":"";
_be.add("iconPlusTop",ip+"plustop.png",_bf,_bf);
_be.add("iconPlus",ip+_c1+"plus.png",_bf,_bf);
_be.add("iconPlusBottom",ip+_c1+"plusbottom.png",_bf,_bf);
_be.add("iconPlusOnly",ip+"plusonly.png",_bf,_bf);
_be.add("iconMinusTop",ip+"minustop.png",_bf,_bf);
_be.add("iconMinus",ip+_c1+"minus.png",_bf,_bf);
_be.add("iconMinusBottom",ip+_c1+"minusbottom.png",_bf,_bf);
_be.add("iconMinusOnly",ip+_c1+"minusonly.png",_bf,_bf);
_be.add("iconLine",ip+"line.png",_bf,_bf);
_be.add("iconBlank",ip+"blank.png",_bf,_bf);
_be.add("iconJoinTop",ip+"jointop.png",_bf,_bf);
_be.add("iconJoin",ip+"join.png",_bf,_bf);
_be.add("iconJoinBottom",ip+"joinbottom.png",_bf,_bf);
_be.add("Folder",ip+_c1+"folder.png",_bf,_bf);
_be.add("FolderMouseOver",ip+_c1+"folder_mo.png",_bf,_bf);
_be.add("FolderExpanded",ip+_c1+"folder_ex.png",_bf,_bf);
_be.add("FolderExpandedMouseOver",ip+_c1+"folder_ex_mo.png",_bf,_bf);
_be.add("FolderExpandedSelected",ip+_c1+"folder_ex_sel.png",_bf,_bf);
_be.add("Document",ip+_c1+"doc.png",_bf,_bf);
_be.add("DocumentMouseOver",ip+_c1+"doc_mo.png",_bf,_bf);
_be.add("DocumentSelected",ip+_c1+"doc_sel.png",_bf,_bf);
}
function TocUpdate(_c2){
var _c3=false;
var _c4=null;
var _c5=null;
var _c6=top.frames[theToc.contentFrame];
if(_c6&&typeof (_c6.thisId)!="undefined"){
_c4=_c6.thisId;
_c5=_c6.parentId;
}else{
if(typeof (CFG_MAIN_TOPIC)!="undefined"){
_c4=CFG_MAIN_TOPIC.replace("."+CFG_HTML_EXT,"");
}
}
_c3=theToc.load(_c4,_c5);
if(_c3==true){
theToc.syncWithPage(_c4);
}else{
if(typeof _c2=="undefined"){
dom_getEl(document,"tocContent").innerHTML="<div class='entry0'>Contents not available.</div>"+"<div class='entry0'>Unable to load xml content.</div>";
}
}
return _c3;
}
function TocUpdateFromTopic(){
if(!theToc){
setTimeout("TocUpdateFromTopic()",200);
}
if(theToc.isVisible){
if(!theToc.isBusy){
if(TocUpdate(false)==false){
setTimeout("TocUpdate()",0);
}
}else{
setTimeout("TocUpdateFromTopic()",200);
}
}
}
function initToc(){
theToc.contentDiv=dom_getEl(document,"tocContent");
theToc.isBusy=false;
}
var theBrowser=new browserInfo;
var jsErrorMsg="A JavaScript error has occurred:";
if(theBrowser.canOnError){
self.onerror=defOnError;
}
var theToc=new Toc;

