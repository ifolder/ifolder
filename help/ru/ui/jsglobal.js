var html2ui="ui/";
var html2idx="./";
var prevstr="previous";
var nextstr="next";
var synched=false;
function syncTocPage(){
if(synched===false&&parent.theMenu.amBusy===false){
var _1=parent.theMenu.findEntry(location.pathname,"url","right",0);
if((_1>=0)&&(parent.theMenu.entry[_1].FirstChild!=-2)){
synched=true;
parent.theMenu.selectEntry(_1);
if(parent.theMenu.setEntry(_1,true)){
parent.theMenu.refresh();
}
}
}
}
function syncToc(){
if((self==top)||(!parent.theMenu)){
var _2=self.location.pathname+self.location.hash;
if(navigator.appVersion.indexOf("MSIE")!=-1&&self.location.protocol=="file:"&&_2.indexOf("/")==0&&_2.indexOf(":")==2){
_2=_2.substr(1);
}
self.location.replace(html2idx+"index.html?page="+_2);
}
if(parent&&parent.theMenu){
syncTocPage();
}
}
syncToc();
top.document.title=document.title;

