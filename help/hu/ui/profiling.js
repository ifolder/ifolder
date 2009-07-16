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
function processProfiling(){
profileNode(document.body);
}
_P_LDRS=Array();
function callProfilingLoaders(){
for(var i=0;i<_P_LDRS.length;i++){
var _b=_P_LDRS[i];
if(_b!=callProfilingLoaders){
_b();
}
}
}
function addProfilingLoader(_c){
if(window.onload&&window.onload!=callProfilingLoaders){
_P_LDRS[_P_LDRS.length]=window.onload;
}
window.onload=callProfilingLoaders;
_P_LDRS[_P_LDRS.length]=_c;
}
if(typeof (profileArray)!="undefined"&&profileArray.length){
addProfilingLoader(processProfiling);
}

