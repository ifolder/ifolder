function getDHTMLObj(_1,_2){
if(theBrowser.hasW3CDOM){
return eval(_1+".getElementById(\""+_2+"\")");
}else{
return eval(_1+theBrowser.DHTMLRange+"."+_2+theBrowser.DHTMLStyleObj);
}
}
function getDHTMLObjStyle(_3,_4){
if(theBrowser.hasW3CDOM){
return eval(_3+".getElementById(\""+_4+"\")").style;
}else{
return eval(_3+theBrowser.DHTMLRange+"."+_4+theBrowser.DHTMLStyleObj);
}
}
function getDHTMLObjTop(_5){
return (theBrowser.code=="MSIE")?_5.pixelTop:_5.top;
}
function getDHTMLObjHeight(_6,_7){
if(theBrowser.hasW3CDOM){
return parseInt(eval(_6+".getElementById(\""+_7+"\").offsetHeight"),10);
}else{
return eval(_6+theBrowser.DHTMLRange+"."+_7+theBrowser.DHTMLDivHeight);
}
}
function getDHTMLImg(_8,_9,_a){
if(document.layers){
return getDHTMLObjStyle(_8,_9).document.images[_a];
}else{
return eval(_8+".images."+_a);
}
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
function imgStoreFind(_f){
var _10=-1;
for(var i=0;i<=this.count;i++){
if(this.img[i].name==_f){
_10=i;
break;
}
}
return _10;
}
function imgStoreAdd(n,s,w,h){
var i=this.find(n);
if(i==-1){
i=++this.count;
}
this.img[i]=new imgStoreItem(n,s,parseInt(w,10),parseInt(h,10));
}
function imgStoreGetSrc(_17){
var i=this.find(_17);
var img=this.img[i];
return (i==-1)?"":img.src;
}
function imgStoreGetTag(_1a,_1b,_1c){
var i=this.find(_1a);
if(i<0){
return "";
}
with(this.img[i]){
if(src==""){
return "";
}
var tag="<img src=\""+src+"\" width=\""+w+"\" height=\""+h+"\" border=\"0\" align=\"left\" hspace=\"0\" vspace=\"0\"";
tag+=(_1b!="")?" name=\""+_1b+"\"":"";
tag+=" alt=\""+((_1c)?_1c:"")+"\">";
}
return tag;
}
function MenuItem(_1f,id,_21,_22,url,_24,_25,_26,_27,_28){
var t=this;
this.owner=_1f;
this.id=id;
this.type=_21;
this.text=_22;
this.url=url;
this.status=_25;
this.target=_1f.defaultTarget;
this.nextItem=_26;
this.prevItem=_27;
this.FirstChild=-1;
this.parent=_28;
this.isopen=(_1f==-1)?true:false;
this.isSelected=false;
this.nSearchID=_24;
this.draw=MIDraw;
this.PMIconName=MIGetPMIconName;
this.docIconName=MIGetDocIconName;
this.setImg=MISetImage;
this.setIsOpen=MISetIsOpen;
this.setSelected=MISetSelected;
this.setIcon=MISetIcon;
this.mouseOver=MIMouseOver;
this.mouseOut=MIMouseOut;
var i=(this.owner.imgStore)?this.owner.imgStore.find(_21):-2;
if(i==-1){
i=this.owner.imgStore.find("iconPlus");
}
this.height=(i>-1)?this.owner.imgStore.img[i].h:0;
}
function MIDraw(_2b){
var o=this.owner;
var _2d="=\"return "+o.reverseRef+"."+o.name;
var tmp=_2d+".entry["+this.id+"].";
var _2f=" onMouseOver"+tmp+"mouseOver('";
var _30=" onMouseOut"+tmp+"mouseOut('";
var _31=o.imgStore.getTag(this.PMIconName(),"plusMinusIcon"+this.id,"");
var _32="<nobr>"+_2b;
if(!this.noOutlineImg){
if(this.FirstChild!=-1){
_32+="<a href=\"#\" onClick"+_2d+".toggleExpand("+this.id+");\""+_2f+"plusMinusIcon',this);\""+_30+"plusMinusIcon');\">"+_31+"</a>";
}else{
_32+=_31;
}
}
var tip=(o.tipText=="text")?this.text:((o.tipText=="status")?this.status:"");
var _34=o.imgStore.getTag(this.docIconName(),"docIcon"+this.id,tip)+this.text;
var _35="";
var _36="";
if(useImages){
_35=o.imgStore.getTag(this.docIconName(),"docIcon"+this.id,tip);
}
var _37="<SPAN CLASS=\""+((this.CSSClass)?this.CSSClass:((this.FirstChild!=-1)?"node":"leaf"))+"\">";
var _38="<A ID=\"DocNavEntry"+this.id+"\" ";
var _39=(((this.url=="")&&theBrowser.canJSVoid&&o.showAllAsLinks)||o.wizardInstalled)?"javascript:void(0);":this.url;
if(_39!=""){
if(this.target.charAt(1)=="_"){
_39="javascript:"+o.reverseRef+".loadURLInTarget('"+_39+"', '"+this.target+"');";
}
_38+=" href=\""+_39+"\" target=\""+this.target+"\" onClick"+_2d+".itemClicked("+this.id+");\""+_2f+"docIcon',this);\""+_30+"docIcon');\"";
}
_38+=(tip)?" title=\""+tip+"\">":">";
_32+=_37+_36+_38+_35;
if(this.multiLine){
_32+="</a></span><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td>"+_37+_38+this.text+"</a></span></td></tr></table>";
}else{
_32+=this.text+"</a></span>";
}
_32+="</nobr>";
if((theBrowser.hasW3CDOM)&&(theBrowser.hasDHTML)&&(!this.multiLine)){
_32+="<br>";
}
return _32;
}
function MIGetPMIconName(){
var n="icon"+((this.FirstChild!=-1)?((this.isopen==true)?"Minus":"Plus"):"Join");
n+=(this.id==this.owner.firstEntry)?((this.nextItem==-1)?"Only":"Top"):((this.nextItem==-1)?"Bottom":"");
return n;
}
function MIGetDocIconName(){
var is=this.owner.imgStore;
var n=this.type;
n+=((this.isopen)&&(is.getSrc(n+"Expanded")!=""))?"Expanded":"";
n+=((this.isSelected)&&(is.getSrc(n+"Selected")!=""))?"Selected":"";
return n;
}
function MISetImage(_3d,_3e){
var o=this.owner;
var s=o.imgStore.getSrc(_3e);
if((s!="")&&(theBrowser.canCache)&&(!o.amBusy)){
var img=(theBrowser.hasDHTML)?getDHTMLImg(o.container+".document","entryDIV"+this.id,_3d):eval(o.container).document.images[_3d];
if(img&&img.src!=s){
img.src=s;
}
}
}
function MISetIsOpen(_42){
if((this.isopen!=_42)&&(this.FirstChild!=-1)){
this.isopen=_42;
this.setImg("plusMinusIcon"+this.id,this.PMIconName());
this.setImg("docIcon"+this.id,this.docIconName());
return true;
}else{
return false;
}
}
function MISetSelected(_43){
this.isSelected=_43;
var d=this.owner.container+".document";
var s=getDHTMLObj(d,"DocNavEntry"+this.id);
if(s&&s.style){
if(_43){
s.style.color="#cc0000";
s.style.fontWeight="bold";
}else{
s.style.color="#333333";
s.style.fontWeight="normal";
}
}
this.setImg("docIcon"+this.id,this.docIconName());
if((this.parent>=0)&&this.owner.selectParents){
this.owner.entry[this.parent].setSelected(_43);
}
}
function MISetIcon(_46){
this.type=_46;
this.setImg("docIcon"+this.id,this.docIconName());
}
function MIMouseOver(_47,_48){
eval(this.owner.container).status="";
var _49="";
var s="";
if(_47=="plusMinusIcon"){
_49=this.PMIconName();
s="Click to "+((this.isopen==true)?"collapse.":"expand.");
}else{
if(_47=="docIcon"){
_49=this.docIconName();
s=(this.status!=null)?this.status:_48;
}
}
setStatus(s);
if(theBrowser.canOnMouseOut){
this.setImg(_47+this.id,_49+"MouseOver");
}
if(this.onMouseOver){
var me=this;
eval(me.onMouseOver);
}
return true;
}
function MIMouseOut(_4c){
clearStatus();
var _4d="";
if(_4c=="plusMinusIcon"){
_4d=this.PMIconName();
}else{
if(_4c=="docIcon"){
_4d=this.docIconName();
}
}
this.setImg(_4c+this.id,_4d);
if(this.onMouseOut){
var me=this;
eval(me.onMouseOut);
}
return true;
}
function Menu(){
this.count=-1;
this.size=0;
this.version="2.5.4";
this.firstEntry=-1;
this.autoScrolling=false;
this.modalFolders=false;
this.linkOnExpand=false;
this.toggleOnLink=false;
this.showAllAsLinks=false;
this.savePage=true;
this.name="theMenu";
this.container="menu";
this.reverseRef="parent";
this.contentFrame="text";
this.defaultTarget="text";
this.menuLoaderFrame="menuChildLoader";
this.tipText="none";
this.selectParents=true;
this.lastPMClicked=-1;
this.selectedEntry=-1;
this.wizardInstalled=false;
this.amBusy=true;
this.maxHeight=0;
this.imgStore=new imgStoreObject;
this.entry=new MenuItem(this,0,"","","","","",-1,-1,-1);
this.contentWin=MenuGetContentWin;
this.getEmptyEntry=MenuGetEmptyEntry;
this.addEntry=MenuAddEntry;
this.addMenu=MenuAddEntry;
this.addChild=MenuAddChild;
this.rmvEntry=MenuRmvEntry;
this.rmvChildren=MenuRmvChildren;
this.draw=MenuDraw;
this.drawALevel=MenuDrawALevel;
this.refresh=MenuRefresh;
this.reload=MenuReload;
this.refreshDHTML=MenuRefreshDHTML;
this.scrollTo=MenuScrollTo;
this.itemClicked=MenuItemClicked;
this.selectEntry=MenuSelectEntry;
this.setEntry=MenuSetEntry;
this.setEntryByURL=MenuSetEntryByURL;
this.setAllChildren=MenuSetAllChildren;
this.setAll=MenuSetAll;
this.openAll=MenuOpenAll;
this.closeAll=MenuCloseAll;
this.findEntry=MenuFindEntry;
this.toggleExpand=MenuToggleExpand;
this.loadScript=MenuLoadScript;
this.currentSubRoot=-1;
this.footer="";
this.showFooter=true;
this.markChildrenForDelete=-1;
}
function MenuGetContentWin(){
return eval(((myOpener!=null)?"myOpener.":"self.")+this.contentFrame);
}
function MenuGetEmptyEntry(){
for(var i=0;i<=this.count;i++){
if(this.entry[i]==null){
break;
}
}
if(i>this.count){
this.count=i;
}
return i;
}
function MenuAddEntry(_50,_51,_52,url,_54,_55,_56){
if(!_56){
_56=false;
}
var _57=-1;
var _58=-1;
var _59=-1;
if(_50<0){
var i=_50=this.firstEntry;
if(!_56){
while(i>-1){
_50=i;
i=this.entry[i].nextItem;
}
}
}
if(_50>=0){
var e=this.entry[_50];
if(!e){
return -1;
}
_59=(_56)?e.prevItem:_50;
_57=(_56)?_50:e.nextItem;
_58=e.parent;
}
var _5c=this.getEmptyEntry();
if(_59>=0){
this.entry[_59].nextItem=_5c;
}else{
if(_58>=0){
this.entry[_58].FirstChild=_5c;
}else{
this.firstEntry=_5c;
}
}
if(_57>=0){
this.entry[_57].prevItem=_5c;
}
this.entry[_5c]=new MenuItem(this,_5c,_51,_52,url,_54,_52,_57,_59,_58);
this.size++;
return _5c;
}
function MenuAddChild(_5d,_5e,_5f,url,_61,_62,_63){
if(!_63){
_63=false;
}
var _64=-1;
if((this.count==-1)||(_5d<0)){
this.rmvEntry(this.firstEntry);
_64=this.addEntry(-1,_5e,_5f,url,_61,_5f,false);
}else{
var e=this.entry[_5d];
if(!e){
return -1;
}
var cID=e.FirstChild;
if(cID<0){
e.FirstChild=_64=this.getEmptyEntry();
this.entry[_64]=new MenuItem(this,_64,_5e,_5f,url,_61,_5f,-1,-1,_5d);
}else{
while(!_63&&(this.entry[cID].nextItem>=0)){
cID=this.entry[cID].nextItem;
}
_64=this.addEntry(cID,_5e,_5f,url,_61,_5f,_63);
}
}
return _64;
}
function MenuRmvEntry(_67){
var e=this.entry[_67];
if(e==null){
return;
}
var p=e.prevItem;
var n=e.nextItem;
if(e.FirstChild>-1){
this.rmvChildren(_67);
}
if(this.firstEntry==_67){
this.firstEntry=n;
}
if(this.selectedEntry==_67){
this.selectedEntry=n;
}
if(p>-1){
this.entry[p].nextItem=n;
}else{
if(e.parent>-1){
this.entry[e.parent].FirstChild=n;
}else{
if(this.firstEntry==_67){
this.firstEntry=n;
}
}
}
if(n>-1){
this.entry[n].prevItem=p;
}
this.entry[_67]=null;
this.size--;
}
function MenuRmvChildren(_6b){
var _6c;
var e;
var tmp;
if(_6b==-1){
_6c=this.firstEntry;
this.firstEntry=-1;
}else{
_6c=this.entry[_6b].FirstChild;
this.entry[_6b].FirstChild=-1;
}
while(_6c>-1){
e=this.entry[_6c];
if(e.FirstChild>-1){
this.rmvChildren(_6c);
}
if(this.selectedEntry==_6c){
this.selectedEntry=e.parent;
}
tmp=_6c;
_6c=e.nextItem;
this.entry[tmp]=null;
this.size--;
}
}
function MenuDraw(){
this.maxHeight=0;
var _6f=eval(this.container+".document");
eval(this.container).document.writeln(this.drawALevel(this.firstEntry,"",true,_6f));
if(theBrowser.hasDHTML){
for(var i=0;i<=this.count;i++){
if(this.entry[i]){
this.maxHeight+=getDHTMLObjHeight(this.container+".document","entryDIV"+i);
}
}
}else{
if((this.lastPMClicked>0)&&theBrowser.mustMoveAfterLoad&&this.autoScrolling){
this.scrollTo(this.lastPMClicked);
}
}
}
function MenuDrawALevel(_71,_72,_73,_74){
var _75=_71;
var _76="";
var _77="";
var _78="";
var e=null;
while(_75>-1){
e=this.entry[_75];
_77=e.draw(_72);
if(theBrowser.hasDHTML){
_77="<div id=\"entryDIV"+_75+"\" class=\"menuItem\">"+_77+"</div>";
}else{
_77+="<br clear=\"all\" />";
}
theBrowser.lineByLine=true;
if(theBrowser.lineByLine){
_74.writeln(_77);
}else{
_78+=_77;
}
if((e.FirstChild>-1)&&((theBrowser.hasDHTML||(e.isopen&&_73)))){
_76=(e.noOutlineImg)?"":this.imgStore.getTag((e.nextItem==-1)?"iconBlank":"iconLine","","");
_78+=this.drawALevel(e.FirstChild,_72+_76,(e.isopen&&_73),_74);
}
_75=e.nextItem;
}
return _78;
}
function MenuRefresh(){
if(theBrowser.hasDHTML){
if(!this.amBusy){
this.refreshDHTML();
if(this.autoScrolling){
this.scrollTo(this.lastPMClicked);
}
}
}else{
this.reload();
}
}
function MenuReload(){
if(!this.amBusy){
this.amBusy=true;
var l=eval(this.container).location;
var rm=theBrowser.reloadMethod;
var _7c=l.href;
if(this.autoScrolling&&(this.lastPMClicked>0)&&!theBrowser.mustMoveAfterLoad){
_7c+="#joustEntry"+this.lastPMClicked;
}
if(rm=="replace"){
l.replace(_7c);
}else{
if(rm=="reload"){
l.reload();
}else{
if(rm=="timeout"){
setTimeout(this.container+".location.href ='"+_7c+"';",100);
}else{
l.href=_7c;
}
}
}
}
}
function MenuRefreshDHTML(){
var _7d=new simpleArray;
var _7e=this.firstEntry;
var _7f=(_7e==-1)?0:1;
var _80=true;
var _81=1;
var co=eval(this.container);
var _83=co.menuStart;
var d=this.container+".document";
var e=null;
var s=null;
while(_7f>0){
e=this.entry[_7e];
s=getDHTMLObjStyle(d,"entryDIV"+_7e);
if(_80){
s.top=_83;
s.visibility="visible";
_83+=getDHTMLObjHeight(d,"entryDIV"+_7e);
_81=_7f;
}else{
s.visibility="hidden";
s.top=0;
}
if(e.FirstChild>-1){
_80=(e.isopen==true)&&_80;
_7d[_7f++]=e.nextItem;
_7e=e.FirstChild;
}else{
if(e.nextItem!=-1){
_7e=e.nextItem;
}else{
while(_7f>0){
if(_7d[--_7f]!=-1){
_7e=_7d[_7f];
_80=(_81>=_7f);
break;
}
}
}
}
}
this.maxHeight=_83;
co.setMenuHeight(_83);
if(this.markChildrenForDelete>-1){
if(this.size>100){
this.rmvChildren(this.markChildrenForDelete);
this.entry[this.markChildrenForDelete].FirstChild=-2;
this.markChildrenForDelete=-1;
}
}
}
function MenuScrollTo(_87){
if(theBrowser.hasDHTML){
var e=this.entry[_87];
if(!e){
return;
}
var co=eval(this.container);
var d=this.container+".document";
var _8b=getDHTMLObjTop(getDHTMLObjStyle(d,"entryDIV"+_87));
var _8c=(e.nextItem>0)?getDHTMLObjTop(getDHTMLObjStyle(d,"entryDIV"+e.nextItem)):this.maxHeight;
if(theBrowser.code=="MSIE"){
var _8d=co.document.body.scrollTop;
var _8e=_8d+co.document.body.clientHeight;
}else{
var _8d=co.pageYOffset;
var _8e=_8d+co.innerHeight;
}
if((_8c>_8e-20)||(_8b<_8d+20)){
var _8f=_8c-_8e+(_8e-_8d)/2;
if(_8b<(_8d+_8f)){
_8f=_8b-_8d-(_8e-_8d)/2;
}
co.setTimeout("self.scrollBy(0, "+_8f+");",100);
}
}else{
var l=fixPath(eval(this.container).location.pathname)+"#joustEntry"+_87;
setTimeout(this.container+".location.href = \""+l+"\";",100);
}
}
function MenuItemClicked(_91,_92){
var r=true;
var e=this.entry[_91];
var w=this.contentWin();
var b=theBrowser;
if(!_92){
this.selectEntry(_91);
}
if(this.wizardInstalled){
w.menuItemClicked(_91);
}
if(e.onClickFunc){
e.onClick=e.onClickFunc;
}
if(e.onClick){
var me=e;
if(eval(e.onClick)==false){
r=false;
}
}
if(r){
if(((this.toggleOnLink)&&(e.FirstChild!=-1)&&!(_92))||e.noOutlineImg){
if(b.hasDHTML){
this.toggleExpand(_91,true);
}else{
setTimeout(this.name+".toggleExpand("+_91+", true);",100);
}
}
}
return (e.url!="")?r:false;
}
function MenuSelectEntry(_98){
var oe=this.entry[this.selectedEntry];
if(oe){
oe.setSelected(false);
}
var e=this.entry[_98];
if(e){
e.setSelected(true);
}
this.selectedEntry=_98;
}
function MenuSetEntry(_9b,_9c){
var cl=","+_9b+",";
var e=this.entry[_9b];
this.lastPMClicked=_9b;
var mc=e.setIsOpen(_9c);
var p=e.parent;
while(p>=0){
cl+=p+",";
e=this.entry[p];
mc|=(e.setIsOpen(true));
p=e.parent;
}
if(this.modalFolders){
for(var i=0;i<=this.count;i++){
e=this.entry[i];
if((cl.indexOf(","+i+",")<0)&&e){
mc|=e.setIsOpen(false);
}
}
}
return mc;
}
function MenuSetEntryByURL(_a2,_a3){
var i=this.findEntry(_a2,"url","right",0);
return (i!=-1)?this.setEntry(i,_a3):false;
}
function MenuSetAllChildren(_a5,_a6){
var _a7=false;
var _a8=(_a6>-1)?this.entry[_a6].FirstChild:this.firstEntry;
while(_a8>-1){
var e=this.entry[_a8];
_a7|=e.setIsOpen(_a5);
if(e.FirstChild>-1){
_a7|=this.setAllChildren(_a5,_a8);
}
_a8=e.nextItem;
}
return _a7;
}
function MenuSetAll(_aa,_ab){
if(theBrowser.version>=4){
if(_ab=="undefined"){
_ab=-1;
}
}else{
if(_ab==null){
_ab=-1;
}
}
var _ac=false;
if(_ab>-1){
_ac|=this.entry[_ab].setIsOpen(_aa);
}
_ac|=this.setAllChildren(_aa,_ab);
if(_ac){
this.lastPMClicked=this.firstEntry;
this.refresh();
}
}
function MenuOpenAll(){
this.setAll(true,-1);
}
function MenuCloseAll(){
this.setAll(false,-1);
}
function MenuFindEntry(_ad,_ae,_af,_b0){
var e;
var sf;
if(_ad==""){
return -1;
}
if(!_ae){
_ae="url";
}
if(!_af){
_af="exact";
}
if(!_b0){
_b0=0;
}
if(_ae=="URL"){
_ae="url";
}
if(_ae=="title"){
_ae="text";
}
eval("sf = cmp_"+_af);
for(var i=_b0;i<=this.count;i++){
if(this.entry[i]){
e=this.entry[i];
if(sf(eval("e."+_ae),_ad)){
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
function MenuToggleExpand(_be,_bf){
var r=true;
var e=this.entry[_be];
if(e.onToggle){
var me=e;
if(eval(e.onToggle)==false){
r=false;
}
}
if(r){
var chg=this.setEntry(_be,e.isopen^1);
if(this.linkOnExpand&&e.isopen){
if(e.url!=""){
loadURLInTarget(e.url,e.target);
}
if(!_bf){
this.itemClicked(_be,true);
}
}
if(chg){
this.refresh();
}
}
return false;
}
function MenuLoadScript(_c4,_c5,_c6){
var _c7=this.findEntry(_c4,"url","right");
var e=this.entry[_c7];
if(_c7==-1||e.FirstChild==-2){
var re=/\\/g;
var _ca=parent.location.pathname.replace(re,"/");
var _cb=parent.location.protocol!="jar:"?parent.location.protocol:"";
var _cc=_cb+"//"+parent.location.host+_ca.substring(0,_ca.lastIndexOf("/"));
_c5=_c5.substring(_c5.indexOf("/")+1);
var _cd=_cc+"/"+_c5;
top.frames[2].location.replace(_cd);
}else{
if(e.isopen&&_c6){
this.markChildrenForDelete=_c7;
}
}
}
function DrawMenu(m){
m.draw();
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
var _d8=(OP&&(v>=3.2));
var _d9=(OP&&(v>=5));
var _da=(IE&&(v>=4));
var _db=(NS&&(v>=3));
var _dc=(NS&&(v>=5));
this.NWCode=((xx.indexOf("Netware")>=0)||(ua.indexOf("ICEBrowser")>=0));
this.canCache=_db||_da||_d8||WTV;
this.canOnMouseOut=this.canCache;
this.canOnError=_db||_da||_d8;
this.canJSVoid=!((NS&&!_db)||(IE&&!_da)||(OP&&(v<3.5)));
this.lineByLine=(v<4);
this.mustMoveAfterLoad=_db||(_da&&(p!="Mac"))||WTV;
if(_dc==true){
this.reloadMethod="reload";
}else{
if(_db||_da||WTV||_d9){
this.reloadMethod="replace";
}else{
this.reloadMethod=(NS&&(v==2.01)&&(p!="Win"))?"timeout":"href";
}
}
this.canFloat=NS||(IE&&!((p=="Mac")&&(v>=4)&&(v<5)));
this.hasDHTML=((NS||IE)&&(v>=4))&&!(IE&&(p=="Mac")&&(v<4.5))||this.NWCode;
this.slowDHTML=_da||_dc;
this.hasW3CDOM=(document.getElementById)?true:false;
this.needLM=(!this.hasW3CDOM&&NS)||(IE&&(p=="Mac")&&(v>=4.5));
this.DHTMLRange=IE?".all":"";
this.DHTMLStyleObj=IE?".style":"";
this.DHTMLDivHeight=IE?".offsetHeight":".clip.height";
}
function getWindow(){
return (floatingMode)?myOpener:self;
}
function setStatus(_dd){
var _de=getWindow();
if(_de){
_de.status=_dd;
if(!theBrowser.canOnMouseOut){
clearTimeout(statusTimeout);
statusTimeout=setTimeout("clearStatus()",5000);
}
}
return true;
}
function clearStatus(){
var _df=getWindow();
if(_df){
_df.status="";
}
}
function unloadFloating(){
if(myOpener){
if(myOpener.JoustFrameset){
myOpener.setTimeout("menuClosed();",100);
}
}
}
function getMode(){
var _e0=getParm(document.cookie,"mode",";");
return ((_e0=="Floating")||(_e0=="NoFrames"))?_e0:"Frames";
}
function smOnError(msg,url,lno){
smCallerWin.onerror=oldErrorHandler;
if(confirm(smSecurityMsg)){
setTimeout("setMode(\""+smNewMode+"\");",100);
}
return true;
}
function smSetCookie(_e4){
document.cookie="mode="+_e4+"; path=/";
if(getMode()!=_e4){
alert(smCookieMsg);
return false;
}else{
return true;
}
}
function setMode(_e5,_e6){
smNewMode=_e5;
smCallerWin=(theBrowser.code=="NS")?_e6:self;
var _e7=true;
var _e8=getMode();
if(_e5!=_e8){
if(_e8=="Floating"){
if(smSetCookie(_e5)){
self.close();
}
}else{
var _e9="";
if(theBrowser.canFloat){
if((theMenu.savePage)&&(_e6)){
if(theBrowser.canOnError){
oldErrorHandler=smCallerWin.onerror;
smCallerWin.onerror=smOnError;
}
var l=theMenu.contentWin().location;
var p=l.pathname;
if(theBrowser.canOnError){
smCallerWin.onerror=oldErrorHandler;
}
if(p){
_e9=fixPath(p)+l.search;
}else{
if(!confirm(smSecurityMsg)){
_e7=false;
}
}
}
}else{
_e7=false;
}
if(_e7&&smSetCookie(_e5)){
if(_e5=="NoFrames"){
location.href=(index3=="")?((_e9=="")?"/":_e9):index3;
}else{
location.href=index2+((_e9=="")?"":"?page="+escape(_e9));
}
}
}
}
}
function fixPath(p){
var i=p.indexOf("?",0);
if(i>=0){
p=p.substring(0,i);
}
if(p.substring(0,2)=="/:"){
p=p.substring(p.indexOf("/",2),p.length);
}
i=p.indexOf("\\",0);
while(i>=0){
p=p.substring(0,i)+"/"+p.substring(i+1,p.length);
i=p.indexOf("\\",i);
}
return p;
}
function fileFromPath(p){
p=fixPath(p);
var i=p.lastIndexOf("\\");
if(i>=0){
p=p.substring(i+1,p.length);
}
return p;
}
function getParm(_f0,_f1,_f2){
if(_f0.length==0){
return "";
}
var _f3=_f0.indexOf(_f1+"=");
if(_f3==-1){
return "";
}
_f3=_f3+_f1.length+1;
var _f4=_f0.indexOf(_f2,_f3);
if(_f4==-1){
_f4=_f0.length;
}
return unescape(_f0.substring(_f3,_f4));
}
function pageFromSearch(def,m,_f7){
var s=self.location.search;
if((s==null)||(s.length<=1)||(s.indexOf("?page=")!=0)){
return def;
}
var p=getParm(s,"page","&");
p=(p!="")?fixPath(p):def;
if(m!=null){
var e=m.findEntry(p,"URL","exact");
if((e!=-1)&&_f7){
m.setEntry(e,true);
m.selectEntry(e);
}
}
return p+self.location.hash;
}
function loadURLInTarget(u,t){
var w=eval("self."+t);
if(!w&&myOpener){
w=eval("myOpener."+t);
}
if(!w&&("_top,_parent,_self".indexOf(t)>=0)){
w=eval("getWindow()."+t.substring(1));
}
if(w){
w.location.href=u;
}else{
window.open(u,t);
}
}
function defOnError(msg,url,lno){
if(jsErrorMsg==""){
return false;
}else{
alert(jsErrorMsg+".\n\nError: "+msg+"\nPage: "+url+"\nLine: "+lno+"\nBrowser: "+navigator.userAgent);
return true;
}
}
function defaultResizeHandler(){
if((theBrowser.code=="NS")&&theBrowser.hasDHTML&&(self.frames.length!=0)){
if(!eval(theMenu.container+".document.menuBottom")){
theMenu.reload();
}
}
}
var theBrowser=new browserInfo;
var jsErrorMsg="A JavaScript error has occurred on this page!  Please note down the ";
jsErrorMsg+="following information and pass it on to the Webmaster.";
if(theBrowser.canOnError){
self.onerror=defOnError;
}
var theMenu=new Menu;
var JoustFrameset=true;
var statusTimeout=0;
var index1="index.htm";
var useImages=true;
var smCallerWin;
var smNewMode;
var oldErrorHandler;
var smNoFloat="Sorry, your browser does not support this feature!";
var smCookieMsg="You must have Cookies enabled to change the display mode!";
var smSecurityMsg="Due to security restrictions imposed by your browser, I cannot ";
smSecurityMsg+="change modes while a page from another server is being displayed. ";
smSecurityMsg+="The default home page for this site will be displayed instead.";
var floatingMode=(getMode()=="Floating");
var myOpener=null;
if(floatingMode==true){
if(self.opener){
myOpener=self.opener;
if(myOpener.JoustFrameset){
myOpener.setTimeout("setGlobals();",100);
}
}else{
document.cookie="mode=Frames; path=/";
floatingMode=false;
}
}else{
if(getMode()!="Frames"){
document.cookie="mode=Frames; path=/";
}
}
function initOutlineIcons(_101){
var ip="images/";
_101.add("iconPlusTop",ip+"plustop.gif",18,18);
_101.add("iconPlus",ip+"plus.gif",18,18);
_101.add("iconPlusBottom",ip+"plusbottom.gif",18,18);
_101.add("iconPlusOnly",ip+"plusonly.gif",18,18);
_101.add("iconMinusTop",ip+"minustop.gif",18,18);
_101.add("iconMinus",ip+"minus.gif",18,18);
_101.add("iconMinusBottom",ip+"minusbottom.gif",18,18);
_101.add("iconMinusOnly",ip+"minusonly.gif",18,18);
_101.add("iconLine",ip+"line.gif",18,18);
_101.add("iconBlank",ip+"blank.gif",18,18);
_101.add("iconJoinTop",ip+"jointop.gif",18,18);
_101.add("iconJoin",ip+"join.gif",18,18);
_101.add("iconJoinBottom",ip+"joinbottom.gif",18,18);
_101.add("Folder",ip+"folder.gif",18,18);
_101.add("FolderMouseOver",ip+"folder_mo.gif",18,18);
_101.add("FolderExpanded",ip+"folder_ex.gif",18,18);
_101.add("FolderExpandedMouseOver",ip+"folder_ex_mo.gif",18,18);
_101.add("FolderExpandedSelected",ip+"folder_ex_sel.gif",18,18);
_101.add("Document",ip+"doc.gif",18,18);
_101.add("DocumentMouseOver",ip+"doc_mo.gif",18,18);
_101.add("DocumentSelected",ip+"doc_sel.gif",18,18);
}
function loadMenu(){
if((theMenu)&&(theMenu.amBusy==false)&&frames[2]){
theMenu.loadScript(text.parentfile,text.parenttoc);
}else{
setTimeout("loadMenu()",200);
}
}

