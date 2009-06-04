function dom_ReplaceHtml(el,_2){
var _3=el;
var _4=_3.cloneNode(false);
_4.innerHTML=_2;
_3.parentNode.replaceChild(_4,_3);
return _4;
}
function dom_LoadXMLDoc(_5){
var _6=null;
try{
_6=new ActiveXObject("Microsoft.XMLDOM");
}
catch(e){
try{
var _7=new XMLHttpRequest();
_7.open("GET",_5,false);
_7.send(null);
_6=_7.responseXML;
return _6;
}
catch(e){
return (null);
}
}
try{
_6.async=false;
_6.load(_5);
return (_6);
}
catch(e){
}
return (null);
}
function dom_getEl(_8,_9){
return _8.getElementById(_9);
}
function dom_firstChildElement(_a){
var _b=_a.firstChild;
while(_b&&_b.nodeType!=1){
_b=_b.nextSibling;
}
return _b;
}
function dom_firstChildElementOfClass(_c,_d){
var _e=dom_firstChildElement(_c);
if(_e){
if(_e.className!=_d){
_e=dom_nextSiblingElementOfClass(_e,_d);
}
}
return _e;
}
function dom_nextSiblingElement(_f){
var _10=_f.nextSibling;
while(_10&&_10.nodeType!=1){
_10=_10.nextSibling;
}
return _10;
}
function dom_prevSiblingElement(_11){
var _12=_11.previousSibling;
while(_12&&_12.nodeType!=1){
_12=_12.previousSibling;
}
return _12;
}
function dom_nextSiblingElementOfClass(_13,_14){
var _15=dom_nextSiblingElement(_13);
while(_15&&(_15.className!=_14)){
_15=dom_nextSiblingElement(_15);
}
if(_15&&(_15.className!=_14)){
_15=null;
}
return _15;
}
function dom_prevSiblingElementOfClass(_16,_17){
var _18=dom_prevSiblingElement(_16);
while(_18&&(_18.className!=_17)){
_18=dom_prevSiblingElement(_18);
}
if(_18&&(_18.className!=_17)){
_18=null;
}
return _18;
}
function dom_getChildElementsNamed(_19,_1a){
var _1b=new Array();
var _1c=dom_firstChildElement(_19);
while(_1c){
if(_1c.nodeName==_1a){
_1b.push(_1c);
}
_1c=dom_nextSiblingElement(_1c);
}
return _1b;
}
function dom_getScrollY(){
var _1d=0;
if(typeof (window.pageYOffset)=="number"){
_1d=window.pageYOffset;
}else{
if(document.body&&document.body.scrollTop){
_1d=document.body.scrollTop;
}else{
if(document.documentElement&&document.documentElement.scrollTop){
_1d=document.documentElement.scrollTop;
}
}
}
return _1d;
}
function dom_getClientHeight(){
var _1e=0;
if(typeof (window.innerWidth)=="number"){
_1e=window.innerHeight;
}else{
if(document.documentElement&&document.documentElement.clientHeight){
_1e=document.documentElement.clientHeight;
}else{
if(document.body&&document.body.clientHeight){
_1e=document.body.clientHeight;
}
}
}
return _1e;
}
function dom_getOffsetFromLeft(_1f){
var _20=_1f.offsetLeft;
while(_1f.offsetParent){
_1f=_1f.offsetParent;
_20+=_1f.offsetLeft;
}
return _20;
}

