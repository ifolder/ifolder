function addFirstChildElement(_1,_2){
var _3=_1.firstChild;
var _4=document.createElement(_2);
_3?_1.insertBefore(_4,_3):_1.appendChild(_4);
return _4;
}
function hasClass(_5,_6,_7){
var _8=false;
if(_5.className){
var _9=_5.className.split(" ");
for(var i=0;i<_9.length;i++){
if(_9[i].toLowerCase()==_6.toLowerCase()){
if(_7==true){
var _b="";
_9.splice(i,1);
if(_9.length>0){
_b=_9[0];
for(i=1;i<_9.length;i++){
_b+=" "+_9[i];
}
}
_5.className=_b;
}
_8=true;
}
}
}
return _8;
}
function removeClass(_c,_d){
hasClass(_c,_d,true);
}
function replaceClass(_e,_f,_10){
removeClass(_e,_f);
addClass(_e,_10);
}
function addClass(_11,_12){
if(!_11.className){
_11.className=_12;
}else{
if(hasClass(_11,_12)==false){
var _13=_11.className;
_13+=" ";
_13+=_12;
_11.className=_13;
}
}
}
function firstChildElementNamed(_14,_15){
var _16=_14.firstChild;
while(_16!=null){
if(_16.nodeType==1&&_16.tagName.toLowerCase()==_15){
break;
}
_16=_16.nextSibling;
}
return _16;
}
function firstChildElement(_17){
var _18=_17.firstChild;
while(_18!=null){
if(_18.nodeType==1){
break;
}
_18=_18.nextSibling;
}
return _18;
}
function siblingElement(_19){
var _1a=_19.nextSibling;
while(_1a!=null){
if(_1a.nodeType==1){
break;
}
_1a=_1a.nextSibling;
}
return _1a;
}
function processExpandableSection(_1b){
var _1c=firstChildElement(_1b.parentNode);
if(_1c){
addClass(_1c,"expanded");
var _1d=addFirstChildElement(_1c,"span");
_1c.onclick=x_toggle;
x_switch(_1c);
}
}
function x_toggle(){
x_switch(this);
}
function x_switch(_1e){
var _1f=siblingElement(_1e);
if(_1f!=null){
if(hasClass(_1e,"expanded")==true){
replaceClass(_1e,"expanded","collapsed");
_1f.style.display="none";
}else{
replaceClass(_1e,"collapsed","expanded");
_1f.style.display="block";
}
}
}
function autoInit_collapsed_sections(){
var _20=document.getElementsByTagName("span");
var _21=new Array();
for(var i=0;i<_20.length;i++){
_21[i]=_20[i];
}
for(var i=0;i<_21.length;i++){
if(hasClass(_21[i],"collapsible")==true){
processExpandableSection(_21[i]);
}
}
}
_LOADERS=Array();
function callAllLoaders(){
var i,_24;
for(i=0;i<_LOADERS.length;i++){
_24=_LOADERS[i];
if(_24!=callAllLoaders){
_24();
}
}
}
function appendLoader(_25){
if(window.onload&&window.onload!=callAllLoaders){
_LOADERS[_LOADERS.length]=window.onload;
}
window.onload=callAllLoaders;
_LOADERS[_LOADERS.length]=_25;
}
appendLoader(autoInit_collapsed_sections);

