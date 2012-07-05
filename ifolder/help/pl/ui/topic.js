function processUlinks(){
if(!document.getElementsByTagName){
return;
}
var _1=document.getElementsByTagName("a");
for(var i=0;i<_1.length;i++){
var _3=_1[i];
if(_3.getAttribute("href")&&_3.className=="ulink"){
_3.target="_blank";
}
}
}
function getFilename(_4,_5){
_5=(typeof (_5)=="undefined")?false:_5;
var f=/[\/\\]([^\/\\]+)$/.exec(_4)||[];
f=f[1]||_4;
return (_5==true)?f.substring(0,f.lastIndexOf(".")):f;
}
function createSpecialHash(){
return location.hash?location.hash:("#"+getFilename(location.pathname,true));
}
function onPageOpen(){
var _7=top.frames["leftnav"];
if(self==top||!_7){
self.location.replace(CFG_HTML2IDX_RELPATH+"index.html"+createSpecialHash());
}else{
if(_7.TocUpdateFromTopic){
_7.TocUpdateFromTopic();
}
window.onload=onPageLoad;
}
}
function onPageLoad(){
top.document.title=document.title;
if(typeof (window.history.replaceState)=="function"){
top.history.replaceState(null,window.location.href,window.location.href);
}else{
var _8=createSpecialHash();
if(top.location.hash!=_8){
top.setHash(_8);
}
}
processUlinks();
}
onPageOpen();

