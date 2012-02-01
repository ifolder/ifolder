var loadedIndex=false;
var topicsfound=new Object;
var indexScroll=0;
var searchScroll=0;
function load_index(_1){
var _2=false;
var _3=dom_LoadXMLDoc(_1);
if(_3){
var _4=new Array();
if(generateIndexHtml(_3,_4)==true){
dom_getEl(document,"indexContent").innerHTML=_4.join("\n");
_2=true;
}
}
if(_2==false){
dom_getEl(document,"indexform").className="inactive";
dom_getEl(document,"indexContent").innerHTML="<div class='entry0'>Index not available.</div>"+"<div class='entry0'>Unable to load xml content.</div>";
}
return _2;
}
function generateIndexHtml(_5,_6){
var _7=_5.getElementsByTagName("entry");
var _8=null;
for(var i=0;i<_7.length;i++){
var _a=_7[i];
var _b=0;
if(_a.parentNode.nodeName=="entry"){
_b=(_a.parentNode.parentNode.nodeName=="entry")?2:1;
}
var _c=dom_firstChildElement(_a);
var _d=dom_getChildElementsNamed(_a,"target");
var _e="<a id='a"+i+"' href='#' onclick='indexClick("+i+",true);return false;'>";
if(_d.length==1){
_8=null;
_e="<a id='a"+i+"' href='../"+_d[0].attributes.getNamedItem("href").nodeValue+"' onclick='indexClick("+i+",false);'>";
}else{
_8="<div id='t"+i+"' class='inactive'><ul class='listbullet'>";
for(var j=0;j<_d.length;j++){
var _10=_d[j].firstChild.nodeValue;
_8+="<li class='listitem'><p class='listitem'>";
_8+="<a href='../"+_d[j].attributes.getNamedItem("href").nodeValue+"'>";
_8+=_10;
_8+="</a></p></li>";
}
_8+="</ul></div>";
}
if(!_c||_c.nodeName!="term"){
return false;
}
var _11=_c.firstChild.nodeValue;
_6.push("<div id='e"+i+"' class='entry"+_b+"'>"+"<span class='node'>"+_e+_11+"</a></span>"+(_8?_8:"")+"</div>");
}
return (_7.length)?true:false;
}
function indexClick(num,_13){
var _14=dom_getEl(document,"e"+num);
var _15=null;
var _16=null;
if(_14.className=="entry2"){
_16=dom_getEl(document,"a"+num).innerHTML;
_14=dom_prevSiblingElementOfClass(_14,"entry1");
}
if(_14.className=="entry1"){
_15=dom_getEl(document,_14.id.replace("e","a")).innerHTML;
_14=dom_prevSiblingElementOfClass(_14,"entry0");
}
var _17=dom_getEl(document,_14.id.replace("e","a")).innerHTML;
var _18=_17+(_15?(", "+_15):"")+(_16?(", "+_16):"");
if(_13==true){
topicsfound.term=_18;
topicsfound.list=dom_getEl(document,"t"+num).innerHTML;
var _19=top.frames["topic"];
if(_19){
_19.location.href="topicsfound.html";
}
}
dom_getEl(document,"isearchfld").value=_18;
}
function searchIndex(val){
var _1b=new String(val).toLocaleLowerCase();
var _1c=dom_getEl(document,"index");
var _1d=dom_getEl(document,"indexContent");
var _1e=_1d;
var _1f=dom_firstChildElementOfClass(_1d,"entry0");
while(_1f){
_1e=_1f;
var _20=new String(dom_firstChildElement(dom_firstChildElement(_1f)).childNodes[0].nodeValue).toLocaleLowerCase();
var cmp=_1b.localeCompare(_20);
_1f=(cmp>0)?dom_nextSiblingElementOfClass(_1f,"entry0"):null;
}
var _22=150;
var y=_1d.offsetTop+_1e.offsetTop-_22;
window.scrollTo(0,y);
}
function tabSelect(_24){
if(_24.className=="tab-unselected"){
_24.className="tab-selected";
if(_24.id!="toctab"){
theToc.isVisible=false;
dom_getEl(document,"toctab").className="tab-unselected";
dom_getEl(document,"toc").className="inactive";
}
if(CFG_HAS_SEARCH&&_24.id!="searchtab"){
var _25=dom_getEl(document,"search");
if(_25.className=="active"){
searchScroll=dom_getScrollY();
}
dom_getEl(document,"searchtab").className="tab-unselected";
_25.className="inactive";
dom_getEl(document,"searchform").className="inactive";
}
if(CFG_HAS_INDEX&&_24.id!="indextab"){
var _26=dom_getEl(document,"index");
if(_26.className=="active"){
indexScroll=dom_getScrollY();
}
dom_getEl(document,"indextab").className="tab-unselected";
_26.className="inactive";
dom_getEl(document,"indexform").className="inactive";
}
if(_24.id=="toctab"){
theToc.isVisible=true;
dom_getEl(document,"toc").className="active";
TocUpdateFromTopic();
}else{
if(_24.id=="searchtab"){
dom_getEl(document,"search").className="active";
dom_getEl(document,"searchform").className="active";
window.scrollTo(0,searchScroll);
dom_getEl(document,"query").focus();
}else{
if(loadedIndex==false){
loadedIndex=load_index("index.xml");
}
dom_getEl(document,"index").className="active";
dom_getEl(document,"indexform").className=loadedIndex?"active":"inactive";
if(loadedIndex){
window.scrollTo(0,indexScroll);
dom_getEl(document,"isearchfld").focus();
}
}
}
}
}
function doTheSearch(){
var _27=dom_getEl(document,"query");
if(tip_init(_27.name+"="+encodeURIComponent(_27.value))==true){
tip_query();
tip_header();
results.push("<br /><br />");
tip_out();
dom_getEl(document,"searchContent").innerHTML=results.join("");
}else{
dom_getEl(document,"searchContent").innerHTML="<div class='entry0'>Search not available.</div>"+"<div class='entry0'>Unable to load xml content.</div>";
}
}
function doSearch(){
dom_getEl(document,"searchContent").innerHTML="<p>&nbsp;&nbsp;Searching...</p>";
self.setTimeout("doTheSearch()",1);
}
function getTabWidth(){
var _28=dom_getEl(document,"toctab");
var _29=dom_getEl(document,"indextab");
var _2a=dom_getEl(document,"searchtab");
return (_28?_28.scrollWidth:0)+(_29?_29.scrollWidth:0)+(_2a?_2a.scrollWidth:0);
}
function onPageLoad(){
initToc();
dom_getEl(document,"toctab").innerHTML=RES_CONTENTS_STR;
if(CFG_HAS_SEARCH){
dom_getEl(document,"searchtab").innerHTML=RES_SEARCH_STR;
dom_getEl(document,"searchbtn").value=RES_SEARCH_STR;
dom_getEl(document,"searchtab").className="tab-unselected";
}
if(CFG_HAS_INDEX){
dom_getEl(document,"indextab").innerHTML=RES_INDEX_STR;
dom_getEl(document,"indextab").className="tab-unselected";
}
if(document.all){
dom_getEl(document,"tabs").style.whiteSpace="normal";
}
if(CFG_RTL_TEXT==false&&(CFG_HELP_TYPE!="tablethtml")){
var _2b=getTabWidth();
if(_2b>230){
var _2c=dom_getEl(top.document,"frameset");
if(_2c){
_2c.cols=_2b+22+",*";
}
}
}
}
window.onload=onPageLoad;

