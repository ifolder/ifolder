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
function x_toggle(){
x_switch(this);
}
function x_switch(_1b){
var _1c=siblingElement(_1b);
if(_1c!=null){
if(hasClass(_1b.parentNode,"expanded")==true){
replaceClass(_1b.parentNode,"expanded","collapsed");
}else{
replaceClass(_1b.parentNode,"collapsed","expanded");
}
}
}
function hasDescendantWithID(_1d,_1e){
if(_1e&&_1d){
var _1f=document.getElementById(_1e);
while(_1f){
if(_1f==_1d){
return true;
}
_1f=_1f.parentNode;
}
}
return false;
}
function autoInit_collapsed_sections(){
var _20=location.hash?location.hash.substring(1):null;
var _21=document.getElementsByTagName("span");
var _22=new Array();
for(var i=0;i<_21.length;i++){
if(hasClass(_21[i],"collapsible")==true){
_22.push(_21[i]);
}
}
for(var i=0;i<_22.length;i++){
var _24=_22[i];
var _25=_22[i].parentNode;
var _26=firstChildElement(_25);
if(_26){
addClass(_26,"exp_title");
addClass(_25,"expanded");
addFirstChildElement(_26,"span");
_26.onclick=x_toggle;
if(hasDescendantWithID(_25,_20)==false){
x_switch(_26);
}
}
}
self.focus();
}
_E_LDRS=Array();
function callExpandableLoaders(){
for(var i=0;i<_E_LDRS.length;i++){
var _28=_E_LDRS[i];
if(_28!=callExpandableLoaders){
_28();
}
}
}
function addExpandableLoader(_29){
if(window.onload&&window.onload!=callExpandableLoaders){
_E_LDRS[_E_LDRS.length]=window.onload;
}
window.onload=callExpandableLoaders;
_E_LDRS[_E_LDRS.length]=_29;
}
addExpandableLoader(autoInit_collapsed_sections);

