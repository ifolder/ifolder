function profileNode(_1){
var _2=false;
if(_1.className&&_1.className.length){
var _3=_1.className.split(" ");
if(_3.length>1){
for(var i=1;i<_3.length;i++){
if(_3[i].indexOf("profiles",0)==0){
_2=true;
var _5=_3[i].split("_");
for(var j=1;j<_5.length&&_2==true;j++){
for(var k=0;k<profileArray.length&&_2==true;k++){
if(_5[j]==profileArray[k]){
_2=false;
}
}
}
break;
}
}
}
}
if(_2==true){
_1.parentNode.removeChild(_1);
}else{
var _8=_1.childNodes;
var _9=_8.length;
for(var i=_9-1;i>=0;i--){
if(_8[i].nodeType==1){
profileNode(_8[i]);
}
}
}
}
function addProfileParameterToXrefs(_a){
var _b=document.links;
for(var i=0;i<_b.length;i++){
var _d=_b.item(i);
var _e=_d.attributes.getNamedItem("class");
if(_e!=null&&_e.nodeValue.indexOf("xref")!=-1){
_d.search=setQueryParam("profile",_a,_d.search);
}
}
}
function updateDocumentTitle(){
var _f=document.getElementsByTagName("h1");
if(_f.length){
document.title=_f.item(0).innerHTML.replace(/<[^>]+>/ig,"");
}
}
function processProfiling(){
profileNode(document.body);
updateDocumentTitle();
if(profileParam.length){
addProfileParameterToXrefs(profileParam);
}
}
_P_LDRS=Array();
function callProfilingLoaders(){
for(var i=0;i<_P_LDRS.length;i++){
var _11=_P_LDRS[i];
if(_11!=callProfilingLoaders){
_11();
}
}
}
function addProfilingLoader(_12){
if(window.onload&&window.onload!=callProfilingLoaders){
_P_LDRS[_P_LDRS.length]=window.onload;
}
window.onload=callProfilingLoaders;
_P_LDRS[_P_LDRS.length]=_12;
}
if(typeof (profileArray)!="undefined"&&profileArray.length){
addProfilingLoader(processProfiling);
}

