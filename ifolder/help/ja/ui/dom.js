function dom_ReplaceHtml(el,_2){
var _3=el;
var _4=_3.cloneNode(false);
_4.innerHTML=_2;
_3.parentNode.replaceChild(_4,_3);
return _4;
}
function dom_LoadXMLDoc(_5){
var _6=null;
if(location.protocol=="file:"){
if(!_6){
try{
_6=new ActiveXObject("MSXML2.XMLHTTP");
}
catch(e){
_6=null;
}
}
if(!_6){
try{
_6=new ActiveXObject("Microsoft.XMLHTTP");
}
catch(e){
_6=null;
}
}
}
if(!_6){
try{
_6=new XMLHttpRequest();
}
catch(e){
_6=null;
}
}
if(typeof ActiveXObject!="undefined"){
if(!_6){
try{
_6=new ActiveXObject("MSXML2.XMLHTTP");
}
catch(e){
_6=null;
}
}
if(!_6){
try{
_6=new ActiveXObject("Microsoft.XMLHTTP");
}
catch(e){
_6=null;
}
}
}
if(!_6){
try{
_6=createRequest();
}
catch(e){
_6=null;
}
}
if(_6){
try{
_6.open("GET",_5,false);
_6.send(null);
if(!_6.responseXML.documentElement&&_6.responseStream){
_6.responseXML.load(_6.responseStream);
}
}
catch(e){
return null;
}
return _6.responseXML;
}else{
return null;
}
}
function dom_getEl(_7,_8){
return _7.getElementById(_8);
}
function dom_firstChildElement(_9){
var _a=_9.firstChild;
while(_a&&_a.nodeType!=1){
_a=_a.nextSibling;
}
return _a;
}
function dom_firstChildElementOfClass(_b,_c){
var _d=dom_firstChildElement(_b);
if(_d){
if(_d.className!=_c){
_d=dom_nextSiblingElementOfClass(_d,_c);
}
}
return _d;
}
function dom_nextSiblingElement(_e){
var _f=_e.nextSibling;
while(_f&&_f.nodeType!=1){
_f=_f.nextSibling;
}
return _f;
}
function dom_prevSiblingElement(_10){
var _11=_10.previousSibling;
while(_11&&_11.nodeType!=1){
_11=_11.previousSibling;
}
return _11;
}
function dom_nextSiblingElementOfClass(_12,_13){
var _14=dom_nextSiblingElement(_12);
while(_14&&(_14.className!=_13)){
_14=dom_nextSiblingElement(_14);
}
if(_14&&(_14.className!=_13)){
_14=null;
}
return _14;
}
function dom_prevSiblingElementOfClass(_15,_16){
var _17=dom_prevSiblingElement(_15);
while(_17&&(_17.className!=_16)){
_17=dom_prevSiblingElement(_17);
}
if(_17&&(_17.className!=_16)){
_17=null;
}
return _17;
}
function dom_getChildElementsNamed(_18,_19){
var _1a=new Array();
var _1b=dom_firstChildElement(_18);
while(_1b){
if(_1b.nodeName==_19){
_1a.push(_1b);
}
_1b=dom_nextSiblingElement(_1b);
}
return _1a;
}
function dom_getScrollY(){
var _1c=0;
if(typeof (window.pageYOffset)=="number"){
_1c=window.pageYOffset;
}else{
if(document.body&&document.body.scrollTop){
_1c=document.body.scrollTop;
}else{
if(document.documentElement&&document.documentElement.scrollTop){
_1c=document.documentElement.scrollTop;
}
}
}
return _1c;
}
function dom_getClientHeight(){
var _1d=0;
if(typeof (window.innerWidth)=="number"){
_1d=window.innerHeight;
}else{
if(document.documentElement&&document.documentElement.clientHeight){
_1d=document.documentElement.clientHeight;
}else{
if(document.body&&document.body.clientHeight){
_1d=document.body.clientHeight;
}
}
}
return _1d;
}
function dom_getOffsetFromLeft(_1e){
var _1f=_1e.offsetLeft;
while(_1e.offsetParent){
_1e=_1e.offsetParent;
_1f+=_1e.offsetLeft;
}
return _1f;
}

