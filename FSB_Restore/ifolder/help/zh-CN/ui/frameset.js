function getParm(_1,_2,_3){
if(_1.length==0){
return "";
}
var _4=_1.indexOf(_2+"=");
if(_4==-1){
return "";
}
_4=_4+_2.length+1;
var _5=_1.indexOf(_3,_4);
if(_5==-1){
_5=_1.length;
}
return unescape(_1.substring(_4,_5));
}
function getFilename(_6,_7){
_7=(typeof (_7)=="undefined")?false:_7;
var f=/[\/\\]([^\/\\]+)$/.exec(_6)||[];
f=f[1]||_6;
return (_7==true)?f.substring(0,f.lastIndexOf(".")):f;
}
var recentHash="";
function setHash(h){
recentHash=h;
var _a=window.location.hash;
var _b=window.location.href;
var s=window.location.search;
if(s&&s!=""){
_b=_b.replace(s,"");
}
var _d=_a!=""?_b.replace(_a,h):(_b+h);
window.location.replace(_d);
return false;
}
function pageFromSearch(_e){
var s=window.location.search;
var h=window.location.hash;
var _11;
if(s){
h=null;
if(s.indexOf("?page=")==0){
var p=getParm(s,"page","&");
h=(p!="")?("#"+getFilename(p,true)):null;
}
}
if(h){
if(h.indexOf("#id=")==0){
var tid=getParm(s,"id","&");
_11=tIdMap[tid];
}else{
if(h.indexOf("#hid=")==0){
var hid=getParm(s,"hid","&");
var tid=hIdMap[hid];
_11=tIdMap[tid];
}else{
_11=tIdMap[h.replace("#","")];
}
}
}
return (_11==undefined)?_e:_11;
}
function pollHash(){
if(window.location.hash==recentHash){
return;
}
recentHash=window.location.hash;
loadTopic();
}
function loadTopic(){
var _15=pageFromSearch(CFG_MAIN_TOPIC);
if(_15!=getFilename(window.topic.location)){
window.topic.location.replace(_15);
}
}
function onFrameSetLoad(){
loadTopic();
setInterval(pollHash,1000);
}
window.onload=onFrameSetLoad;

