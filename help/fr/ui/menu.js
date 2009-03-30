var theMenuRef="parent.theMenu";
var theMenu=eval(theMenuRef);
var theBrowser=parent.theBrowser;
var belowMenu=null;
var menuStart=0;
if(parent.theBrowser){
if(parent.theBrowser.canOnError){
window.onerror=parent.defOnError;
}
}
if(theMenu){
theMenu.amBusy=true;
if(theBrowser.hasDHTML){
if(document.all&&!theBrowser.NWCode){
with(document.styleSheets["menustyles"]){
addRule("#menuTop","position:absolute");
addRule("#menuBottom","position:absolute");
addRule("#menuBottom","visibility:hidden");
addRule("#statusMsgDiv","position:absolute");
}
}else{
if(document.layers){
document.ids.menuTop.position="absolute";
document.ids.menuBottom.position="absolute";
document.ids.menuBottom.visibility="hidden";
document.ids.statusMsgDiv.position="absolute";
}else{
if(theBrowser.hasW3CDOM){
try{
var styleSheetElement=document.styleSheets[0];
var styleSheetLength=styleSheetElement.cssRules.length;
styleSheetElement.insertRule("#menuTop { position:absolute } ",styleSheetLength++);
styleSheetElement.insertRule("#menuBottom { position:absolute } ",styleSheetLength++);
styleSheetElement.insertRule("#menuBottom { visibility:hidden } ",styleSheetLength++);
styleSheetElement.insertRule("#statusMsgDiv { position:absolute } ",styleSheetLength++);
}
catch(error){
}
}
}
}
}
}
function getDHTMLObj(_1){
if(theBrowser.hasW3CDOM){
return document.getElementById(_1).style;
}else{
return eval("document"+theBrowser.DHTMLRange+"."+_1+theBrowser.DHTMLStyleObj);
}
}
function getDHTMLObjHeight(_2){
if(theBrowser.hasW3CDOM){
return document.getElementById(_2).offsetHeight;
}else{
return eval("document"+theBrowser.DHTMLRange+"."+_2+theBrowser.DHTMLDivHeight);
}
}
function myVoid(){
}
function setMenuHeight(_3){
getDHTMLObj("menuBottom").top=_3;
}
function drawStatusMsg(){
if(document.layers){
document.ids.statusMsgDiv.top=menuStart;
}else{
if(document.all&&!theBrowser.NWCode){
document.styleSheets["menustyles"].addRule("#statusMsgDiv","top:"+menuStart);
}else{
if(theBrowser.hasW3CDOM){
try{
var _4=document.styleSheets[0];
var _5=_4.cssRules.length;
_4.insertRule("#statusMsgDiv { top:"+menuStart+"} ",_5++);
}
catch(error){
}
}
}
}
document.writeln("<div id=\"statusMsgDiv\"></div>");
}
function drawLimitMarker(){
var b=theBrowser;
if(theMenu&&b.hasDHTML&&b.needLM){
var _7=theMenu.maxHeight+menuStart+getDHTMLObjHeight("menuBottom");
if(b.code=="NS"&&!b.NWCode){
document.ids.limitMarker.position="absolute";
document.ids.limitMarker.visibility="hidden";
document.ids.limitMarker.top=_7;
}else{
if(b.code=="MSIE"&&!b.NWCode){
with(document.styleSheets["menustyles"]){
addRule("#limitMarker","position:absolute");
addRule("#limitMarker","visibility:hidden");
addRule("#limitMarker","top:"+_7+"px");
}
}else{
if(b.NWCode){
try{
var _8=document.styleSheets[0];
var _9=_8.cssRules.length;
_8.insertRule("#limitMarker { position:absolute }",_9++);
_8.insertRule("#limitMarker { visibility:hidden }",_9++);
_8.insertRule("#limitMarker { top:"+_7+"px }",_9++);
}
catch(error){
}
}
}
}
document.writeln("<div id=\"limitMarker\">&nbsp;</div>");
}
}
function setTop(){
if(theMenu&&theBrowser.hasDHTML){
if(getDHTMLObj("menuTop")){
drawStatusMsg();
menuStart=getDHTMLObjHeight("menuTop");
}else{
theBrowser.hasDHTML=false;
}
}
}
function setBottom(){
if(theMenu){
if(theBrowser.hasDHTML){
var mb=getDHTMLObj("menuBottom");
if(mb){
drawLimitMarker();
getDHTMLObj("statusMsgDiv").visibility="hidden";
menuStart=getDHTMLObjHeight("menuTop");
theMenu.refreshDHTML();
if(theMenu.autoScrolling){
theMenu.scrollTo(theMenu.lastPMClicked);
}
mb.visibility="visible";
}else{
theBrowser.hasDHTML=false;
self.location.reload();
}
}
theMenu.amBusy=false;
}
}
function frameResized(){
if($("toc").style.display=="block"&&theBrowser.hasDHTML){
theMenu.refreshDHTML();
}
}
function syncTheToc(){
if(parent.theMenu.amBusy==false){
var _b=parent.frames["text"];
if(_b&&(typeof _b.syncTocPage!="undefined")){
_b.syncTocPage();
}
}
}
if(self.name!="menu"){
self.location.href="index.html";
}
function tabSelect(_c){
if(_c.className=="tab-unselected"){
_c.className="tab-selected";
if(_c.id=="toctab"){
theMenu.amBusy=false;
$("searchtab").className="tab-unselected";
$("toc").style.display="block";
$("search").style.display="none";
syncTheToc();
}else{
theMenu.amBusy=true;
$("toctab").className="tab-unselected";
$("search").style.display="block";
$("toc").style.display="none";
$("query").focus();
}
}
}
function doTheSearch(){
tip_init(Form.serialize("searchform"));
tip_query();
tip_header();
results.push("<br /><br />");
tip_out();
$("output").innerHTML=results.join("");
}
function doSearch(){
$("output").innerHTML="<p>&nbsp;&nbsp;Searching...</p>";
self.setTimeout("doTheSearch()",1);
}

