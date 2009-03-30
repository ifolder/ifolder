function get_search_regex(_1){
if(WORDSEPREGEX&&WORDSEPREGEX.length>0){
_1=_1.replace(new RegExp("^"+WORDSEPREGEX+"+"),"");
_1=_1.replace(new RegExp(WORDSEPREGEX+"+$"),"");
return new RegExp(WORDSEPREGEX+_1+WORDSEPREGEX,"i");
}else{
return new RegExp(_1,"i");
}
}
function tip_init(_2){
results=new Array();
var rd=_2.indexOf("?d=");
rn=_2.indexOf("&n=");
if(rn==-1){
theQuery=_2.substring(rd+3);
tn=0;
}else{
theQuery=_2.substring(rd+3,rn);
tn=parseInt(_2.substring(rn+3));
}
theQuery=theQuery.replace(/\+/g," ");
theQuery=decodeURIComponent(theQuery);
theQuery=theQuery.replace(/\s+/g," ");
od=theQuery;
hd=theQuery;
theQuery=theQuery.replace(/ and /gi," ");
theQuery=theQuery.replace(/- /gi,"-");
theQuery=theQuery.replace(/\s+/g," ");
nr=per_page;
r_l=results_location;
b_q=bold_query;
b_t=bold_title;
b_f=bold_footer;
ct=context;
ct_l=descriptive_length;
c_w=COMMONWORDS;
tr=new Array();
co=0;
nd=0;
nc=0;
sp_l="";
cw_l="";
xmlPageArray=new Array();
if(window.ActiveXObject){
xmldoc=new ActiveXObject("Microsoft.XMLDOM");
xmldoc.async=false;
xmldoc.onreadystatechange=function(){
if(xmldoc.readyState==4){
get_xml();
}
};
}else{
if(document.implementation&&document.implementation.createDocument){
xmldoc=document.implementation.createDocument("","",null);
xmldoc.async=false;
xmldoc.onload=get_xml;
}else{
}
}
xmldoc.load(data);
theQuery=theQuery.replace(/\*/g,"");
theQuery=theQuery.replace(/\(/g,"");
theQuery=theQuery.replace(/\)/g,"");
theQuery=theQuery.replace(/\^/g,"");
theQuery=theQuery.replace(/^\s+/,"");
theQuery=theQuery.replace(/\s+$/,"");
if(seed<1){
seed=1;
}
if(seed>9){
seed=10;
}
emptyQuery=(theQuery==""||theQuery==" ")?true:false;
phraseSearch=(theQuery.charAt(0)=="\""&&theQuery.charAt(theQuery.length-1)=="\"")?true:false;
if(phraseSearch==false&&emptyQuery==false){
if(c_w.length>0){
cw=c_w.split(" ");
for(i=0;i<cw.length;i++){
pat=new RegExp("\\b"+cw[i]+"\\b","gi");
rn=theQuery.search(pat);
if(rn!=-1){
pat_1=new RegExp("\\+"+cw[i]+"\\b","gi");
pat_2=new RegExp("\\-"+cw[i]+"\\b","gi");
rn_1=theQuery.search(pat_1);
rn_2=theQuery.search(pat_2);
if(rn_1==-1&&rn_2==-1){
cw_l+="<b>"+cw[i]+"</b>, ";
theQuery=theQuery.replace(pat,"");
}
}
}
if(cw_l.length>0){
cw_l=cw_l.replace(/\s+$/,"");
if(cw_l.charAt(cw_l.length-1)==","){
cw_l=cw_l.substr(0,cw_l.length-1);
}
theQuery=theQuery.replace(/\s+/g," ");
theQuery=theQuery.replace(/^\s+/,"");
theQuery=theQuery.replace(/\s+$/,"");
if(theQuery==""||theQuery==" "){
emptyQuery=true;
}
hd=theQuery;
}
}
if(SPELLING.length>0){
cw=SPELLING.split(" ");
for(i=0;i<cw.length;i++){
wt=cw[i].split("^");
pat=new RegExp("\\b"+wt[0]+"\\b","i");
rn=theQuery.search(pat);
if(rn!=-1){
if(sp_l.length<1){
sp_l=theQuery;
}
pat=new RegExp(wt[0],"i");
sp_l=sp_l.replace(pat,wt[1]);
}
}
}
if(STEMMING.length>0){
cw=STEMMING.split(" ");
for(i=0;i<cw.length;i++){
wt=cw[i].split("^");
pat=new RegExp("\\b"+wt[0]+"\\b","i");
rn=theQuery.search(pat);
if(rn!=-1){
theQuery=theQuery.replace(pat,wt[0]+"~"+wt[1]);
}
}
}
theQuery=theQuery.replace(/ or /gi,"~");
theQuery=theQuery.replace(/\"/gi,"");
ct_d=0;
w_in=new Array();
wt=theQuery.split(" ");
for(i=0;i<wt.length;i++){
w_in[i]=0;
if(wt[i].charAt(0)=="-"){
w_in[i]=1;
}
pat=new RegExp("\\~","i");
rn=wt[i].search(pat);
if(rn!=-1){
w_in[i]=2;
}
wt[i]=wt[i].replace(/^\-|^\+/gi,"");
}
a=0;
for(c=0;c<xmlPageArray.length;c++){
xmlPageArray[c]=xmlPageArray[c].replace(/&apos;/gi,"'");
xmlPageArray[c]=xmlPageArray[c].replace(/&amp;/gi,"&");
xmlPageArray[c]=xmlPageArray[c].replace(/&quot;/gi,"\"");
xmlPageArray[c]=xmlPageArray[c].replace(/&gt;/gi,">");
xmlPageArray[c]=xmlPageArray[c].replace(/&lt;/gi,"<");
es=xmlPageArray[c].split("^");
rk=1000;
if(parseInt(es[5])>10){
es[5]="10";
}
pa=0;
nh=0;
for(i=0;i<w_in.length;i++){
if(w_in[i]==0){
nh++;
nt=0;
pat=get_search_regex(wt[i]);
rn=es[0].search(pat);
if(rn!=-1){
rk-=seed*3;
rk-=parseInt(es[5]);
nt=1;
if(ct==1){
ct_d=1;
}
}
rn=es[2].search(pat);
if(rn!=-1){
rk-=seed;
rk-=parseInt(es[5]);
nt=1;
}
rn=es[3].search(pat);
if(rn!=-1){
rk-=seed;
rk-=parseInt(es[5]);
nt=1;
}
if(nt==1){
pa++;
}
}
if(w_in[i]==1){
pat=get_search_regex(wt[i]);
rn=es[0].search(pat);
if(rn!=-1){
pa=0;
}
rn=es[2].search(pat);
if(rn!=-1){
pa=0;
}
rn=es[3].search(pat);
if(rn!=-1){
pa=0;
}
}
if(w_in[i]==2){
nh++;
nt=0;
w_o=wt[i].split("~");
pat=get_search_regex(w_o[0]);
pat_2=get_search_regex(w_o[1]);
rn=es[0].search(pat);
rn_2=es[0].search(pat_2);
if(rn!=-1||rn_2!=-1){
rk-=seed/2;
rk-=parseInt(es[5]);
nt=1;
if(ct==1){
ct_d=1;
}
}
rn=es[2].search(pat);
rn_2=es[2].search(pat_2);
if(rn!=-1||rn_2!=-1){
rk-=seed/2;
rk-=parseInt(es[5]);
nt=1;
}
rn=es[3].search(pat);
rn_2=es[3].search(pat_2);
if(rn!=-1||rn_2!=-1){
rk-=seed/2;
rk-=parseInt(es[5]);
nt=1;
}
if(nt==1){
pa++;
}
}
}
if(pa==nh&&nh!=0){
if(ct==1&&ct_d==0){
pat=get_search_regex(wt[0]);
rn=es[3].search(pat);
if(rn>50){
t_1=es[3].substr(rn-49);
rn=t_1.indexOf(". ");
if(rn!=-1){
t_1=t_1.substr(rn+1);
t_2=t_1.split(" ");
if(t_2.length>ct_l){
es[2]="";
for(i=1;i<ct_l+1;i++){
es[2]+=" "+t_2[i];
}
if(es[2].charAt(es[2].length-1)=="."||es[2].charAt(es[2].length-1)==","){
es[2]=es[2].substr(0,es[2].length-1);
}
es[2]+=" ...";
}
}
}
}
tr[a]=rk+"^"+es[0]+"^"+es[1]+"^"+es[2]+"^"+es[3]+"^"+es[4]+"^"+es[5];
a++;
}
}
tr.sort();
co=a;
}
if(phraseSearch==true&&emptyQuery==false){
theQuery=theQuery.replace(/"/gi,"");
a=0;
ct_d=0;
pat=new RegExp(theQuery,"i");
for(c=0;c<xmlPageArray.length;c++){
xmlPageArray[c]=xmlPageArray[c].replace(/&apos;/gi,"'");
xmlPageArray[c]=xmlPageArray[c].replace(/&amp;/gi,"&");
xmlPageArray[c]=xmlPageArray[c].replace(/&quot;/gi,"\"");
xmlPageArray[c]=xmlPageArray[c].replace(/&gt;/gi,">");
xmlPageArray[c]=xmlPageArray[c].replace(/&lt;/gi,"<");
es=xmlPageArray[c].split("^");
rk=1000;
if(parseInt(es[5])>10){
es[5]="10";
}
rn=es[0].search(pat);
if(rn!=-1){
rk-=seed*3;
rk-=parseInt(es[5]);
ct_d=1;
}
rn=es[2].search(pat);
if(rn!=-1){
rk-=seed;
rk-=parseInt(es[5]);
}
rn=es[3].search(pat);
if(rn!=-1){
rk-=seed;
rk-=parseInt(es[5]);
}
if(rk<1000){
if(ct==1&&ct_d==0){
rn=es[3].search(pat);
if(rn>50){
t_1=es[3].substr(rn-49);
rn=t_1.indexOf(". ");
if(rn!=-1){
t_1=t_1.substr(rn+1);
t_2=t_1.split(" ");
if(t_2.length>ct_l){
es[2]="";
for(i=1;i<ct_l+1;i++){
es[2]+=" "+t_2[i];
}
if(es[2].charAt(es[2].length-1)=="."||es[2].charAt(es[2].length-1)==","){
es[2]=es[2].substr(0,es[2].length-1);
}
es[2]+=" ...";
}
}
}
}
tr[a]=rk+"^"+es[0]+"^"+es[1]+"^"+es[2]+"^"+es[3]+"^"+es[4]+"^"+es[5];
a++;
}
}
tr.sort();
co=a;
}
if(emptyQuery==true){
co=0;
}
}
function get_xml(){
if(document.implementation&&document.implementation.createDocument){
xmldoc.normalize();
}
pages=xmldoc.getElementsByTagName(xml_pages);
for(c=0;c<pages.length;c++){
rs=pages[c];
es_0=rs.getElementsByTagName(xml_title)[0].firstChild.data;
es_0=es_0.replace(/\^|\~/g,"");
es_1=rs.getElementsByTagName(xml_url)[0].firstChild.data;
es_1=es_1.replace(/\^|\~/g,"");
es_3=rs.getElementsByTagName(xml_content)[0].firstChild.data;
es_3=es_3.replace(/\^|\~/g,"");
es_2="";
ci_e=es_3.split(" ");
if(ci_e.length<ct_l){
es_2=es_3;
}else{
for(i=0;i<ct_l;i++){
es_2+=ci_e[i]+" ";
}
}
es_2=es_2.replace(/^\s*|\s*$/g,"");
if(es_2.charAt(es_2.length-1)=="."||es_2.charAt(es_2.length-1)==","){
es_2=es_2.substr(0,es_2.length-1);
}
es_2+=" ...";
if(rs.getElementsByTagName("open").length>0){
es_4=rs.getElementsByTagName("open")[0].firstChild.data;
}else{
es_4="0";
}
if(rs.getElementsByTagName("rank").length>0){
es_5=rs.getElementsByTagName("rank")[0].firstChild.data;
}else{
es_5="0";
}
xmlPageArray[c]=es_0+"^"+es_1+"^"+es_2+"^"+es_3+"^"+es_4+"^"+es_5;
}
}
function tip_query(){
if(od!="undefined"&&od!=null){
document.tipue.d.value=od;
}
}
function tip_header(){
if(co>0){
ne=nr+tn;
if(ne>co){
ne=co;
}
results.push(ne," ",MSGRESULTS);
}else{
results.push(MSGNOTFOUND);
}
}
function tip_out(){
if(co==0){
return;
}
if(cw_l.length>0){
}
if(tn+nr>co){
nd=co;
}else{
nd=tn+nr;
}
for(a=tn;a<nd;a++){
os=tr[a].split("^");
if(b_q==1&&phraseSearch==false){
for(i=0;i<wt.length;i++){
pat=new RegExp("\\~","i");
rn=wt[i].search(pat);
if(rn!=-1){
tw=wt[i].split("~");
for(c=0;c<tw.length;c++){
lw=tw[c].length;
pat=new RegExp(tw[c],"i");
rn=os[3].search(pat);
if(rn!=-1){
o1=os[3].slice(0,rn);
o2=os[3].slice(rn,rn+lw);
o3=os[3].slice(rn+lw);
os[3]=o1+"<b>"+o2+"</b>"+o3;
}
}
}else{
lw=wt[i].length;
pat=new RegExp(wt[i],"i");
rn=os[3].search(pat);
if(rn!=-1){
o1=os[3].slice(0,rn);
o2=os[3].slice(rn,rn+lw);
o3=os[3].slice(rn+lw);
os[3]=o1+"<b>"+o2+"</b>"+o3;
}
}
}
}
if(b_q==1&&phraseSearch==true){
lw=theQuery.length;
tw=new RegExp(theQuery,"i");
rn=os[3].search(tw);
if(rn!=-1){
o1=os[3].slice(0,rn);
o2=os[3].slice(rn,rn+lw);
o3=os[3].slice(rn+lw);
os[3]=o1+"<b>"+o2+"</b>"+o3;
}
}
if(include_num==1){
results.push(a+1,". ");
}
if(os[5]=="0"){
if(b_t==1){
results.push("<a href=\"",os[2],"\"><b>",os[1],"</b></a>");
}else{
results.push("<a href=\"",os[2],"\">",os[1],"</a>");
}
}
if(os[5]=="1"){
if(b_t==1){
results.push("<a href=\"",os[2],"\" target=\"_blank\"><b>",os[1],"</b></a>");
}else{
results.push("<a href=\"",os[2],"\" target=\"_blank\">",os[1],"</a>");
}
}
if(os[5]!="0"&&os[5]!="1"){
if(b_t==1){
results.push("<a href=\"",os[2],"\" target=\"",os[5],"\"><b>",os[1],"</b></a>");
}else{
results.push("<a href=\"",os[2],"\" target=\"",os[5],"\">",os[1],"</a>");
}
}
if(os[3].length>1){
results.push("<br>",os[3]);
}
if(include_url==1){
if(os[5]=="0"){
results.push("<br><a href=\"",os[2],"\">",os[2],"</a><p>");
}
if(os[5]=="1"){
results.push("<br><a href=\"",os[2],"\" target=\"_blank\">",os[2],"</a><p>");
}
if(os[5]!="0"&&os[5]!="1"){
results.push("<br><a href=\"",os[2],"\" target=\"",os[5],"\">",os[2],"</a><p>");
}
}else{
results.push("<p>");
}
}
}

