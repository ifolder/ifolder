var theMenuRef = "parent.theMenu";
var theMenu = eval(theMenuRef);
var theBrowser = parent.theBrowser;
var belowMenu = null;
var menuStart = 0;


if (parent.theBrowser) {
	if (parent.theBrowser.canOnError) {window.onerror = parent.defOnError;}
}

if (theMenu) {
	theMenu.amBusy = true;
	if (theBrowser.hasDHTML) {
		if (document.all && !theBrowser.NWCode) {
			with (document.styleSheets['JoustStyles']) {
				addRule ("#menuTop", "position:absolute");
				addRule ("#menuBottom", "position:absolute");
				addRule ("#menuBottom", "visibility:hidden");
				addRule ("#statusMsgDiv", "position:absolute");
			}
		} else {
			if (document.layers) {
				document.ids.menuTop.position = "absolute";
				document.ids.menuBottom.position = "absolute";
				document.ids.menuBottom.visibility = "hidden";
				document.ids.statusMsgDiv.position = "absolute";
			} else {
				if (theBrowser.hasW3CDOM) {
				    try{
					var styleSheetElement = document.styleSheets[0];
			        var styleSheetLength = styleSheetElement.cssRules.length;
					styleSheetElement.insertRule("#menuTop { position:absolute } ", styleSheetLength++);
					styleSheetElement.insertRule("#menuBottom { position:absolute } ", styleSheetLength++);
					styleSheetElement.insertRule("#menuBottom { visibility:hidden } ", styleSheetLength++);
					styleSheetElement.insertRule("#statusMsgDiv { position:absolute } ", styleSheetLength++);
				    }
				    catch(error)
				    {}
				}
			}                    
		}
	}
}
function getDHTMLObj(objName) {
	if (theBrowser.hasW3CDOM) {
		return document.getElementById(objName).style;
	} else {
		return eval('document' + theBrowser.DHTMLRange + '.' + objName + theBrowser.DHTMLStyleObj);
	}
}
function getDHTMLObjHeight(objName) {
	if (theBrowser.hasW3CDOM) {
		return document.getElementById(objName).offsetHeight;
	} else {
		return eval('document' + theBrowser.DHTMLRange + '.' + objName + theBrowser.DHTMLDivHeight);
	}
}
function myVoid() { ; }
function setMenuHeight(theHeight) {
	getDHTMLObj('menuBottom').top = theHeight;
}
function drawStatusMsg() {
	if (document.layers) {
		document.ids.statusMsgDiv.top = menuStart;
	} else{
		if (document.all && !theBrowser.NWCode) {
			document.styleSheets["JoustStyles"].addRule ("#statusMsgDiv", "top:" + menuStart);
		} else {
				if (theBrowser.hasW3CDOM) {
				    try{
					var styleSheetElement = document.styleSheets[0];
			        var styleSheetLength = styleSheetElement.cssRules.length;
					styleSheetElement.insertRule("#statusMsgDiv { top:" + menuStart + "} ", styleSheetLength++);
				    }
				    catch(error)
				    {}
				}
			}                    


	}
	document.writeln('<DIV ID="statusMsgDiv"></DIV>');
}
function drawLimitMarker() {
	var b = theBrowser;
	if (theMenu && b.hasDHTML && b.needLM) {
		var limitPos = theMenu.maxHeight + menuStart + getDHTMLObjHeight('menuBottom');
		if (b.code == 'NS' && !b.NWCode) {
			document.ids.limitMarker.position = "absolute";
			document.ids.limitMarker.visibility = "hidden";
			document.ids.limitMarker.top = limitPos;
		}
		else if (b.code == 'MSIE' && !b.NWCode) {
			with (document.styleSheets["JoustStyles"]) {
				addRule ("#limitMarker", "position:absolute");
				addRule ("#limitMarker", "visibility:hidden");
				addRule ("#limitMarker", "top:" + limitPos + "px");
			}
		}
		else if (b.NWCode) {
		    try{
			var styleSheetElement = document.styleSheets[0];
	        var styleSheetLength = styleSheetElement.cssRules.length;
			styleSheetElement.insertRule("#limitMarker { position:absolute }", styleSheetLength++);
			styleSheetElement.insertRule("#limitMarker { visibility:hidden }", styleSheetLength++);
			styleSheetElement.insertRule("#limitMarker { top:" + limitPos + "px }", styleSheetLength++);
		    }
		    catch(error)
		    {}
		}

		document.writeln('<DIV ID="limitMarker">&nbsp;</DIV>');
	}
}
function setTop() {
	if (theMenu && theBrowser.hasDHTML) {
		if (getDHTMLObj('menuTop')) {
			drawStatusMsg();
			menuStart = getDHTMLObjHeight("menuTop");
		} else {
			theBrowser.hasDHTML = false;
		}
	}
}
function setBottom() {
	if (theMenu) {
		if (theBrowser.hasDHTML) {
			var mb = getDHTMLObj('menuBottom');
			if (mb) {
				drawLimitMarker();
				getDHTMLObj("statusMsgDiv").visibility = 'hidden';
				menuStart = getDHTMLObjHeight("menuTop");
				theMenu.refreshDHTML();
				if (theMenu.autoScrolling) {theMenu.scrollTo(theMenu.lastPMClicked);}
				mb.visibility = 'visible';
			} else {
				theBrowser.hasDHTML = false;
				self.location.reload();
			}
		}
		theMenu.amBusy = false;
	}
}

function frameResized() {if (theBrowser.hasDHTML) {theMenu.refreshDHTML();}}

//	############################   End of Joust   ############################

if (self.name != 'menu') { self.location.href = 'index.html'; }        


function getNextChecked(item)
{
  // case that root of tree is selected
  if (item)
  {
     if (item.isChecked)
     {           
   		top.printList +=  (item.nSearchID.length + 3) + "_" + item.nSearchID.replace(/\s/g,"+") + "+en";
		top.printCount ++;
     }
     else if (item.FirstChild > 0)
     {
   	var hasChanged = false;
	var currEntry = (item.id > -1) ? item.FirstChild : item.firstEntry;
	while (currEntry > -1) 
	{        
	    var e = parent.theMenu.entry[currEntry];
	    if (e.isChecked)
		{
		   top.printList +=  (e.nSearchID.length + 3) + "_" + e.nSearchID.replace(/\s/g,"+") + "+en";
		   top.printCount ++;
		}
		else
		{
		    getNextChecked(e);
		}
	    currEntry = e.nextItem;
	}
      }     
  }
  else
  {
 // no items to search
  }
}

function getPrintUrl(menu)
{  

 top.printList = "";
 top.printCount = 0;
  
  getNextChecked(parent.theMenu.entry[0]);
  if (top.printCount > 0)
  {
    top.printList = top.printCount + "_" + top.printList;
  }
  else
  { 
      // whole book:
      // top.printList = "1_" + (parent.theMenu.entry[0].nSearchID.length + 3) + "_" + parent.theMenu.entry[0].nSearchID.replace(/\s/g,"+") + "+en";
      // nothing:
         top.printList = "";
  }

  return top.printList;
}
	


// Get a URL to pass checked topics to the Print Servlet
function printSelectedTopics()
{                

 if ((parent.theMenu) && (parent.theMenu.amBusy == false))  
 {       
    var printSearchId =  getPrintUrl(parent.theMenu);
    if (printSearchId && printSearchId != "")
    {      
          var collection = "";     
	  var path = document.location.pathname;
	  if (path.indexOf("/documentation/lg") >= 0)
	    collection = "&collection=documentation";
	  if (path.indexOf("/documentation/beta") >= 0)
	    collection = "&collection=doc_beta";
	  if (path.indexOf("/ndk/doc") >= 0)
	    collection = "&collection=dev_ndk";  
	       
 	var url = "http://search.novell.com/NSearch/PrintServlet?query0=&id0=" + printSearchId + "&numhits=10000&lang=en&encoding=iso-8859-1&synflag=false" + collection;
 	var newWindow = window.open("","", 'location, menubar, toolbar, resizable, scrollbars, width=640, height=480' );  
    	newWindow.location = url;
    	newWindow.skipTocSync = true;
    	
    }
    else 
    {   
       alert("There are no items checked!");
    }
 }  
 else
 {
    alert("There is no book to print.");
 }  
}

function searchSelectedTopics()
{
	 if ((parent.theMenu) && (parent.theMenu.amBusy == false))  
	 {       
		var printSearchId =  getPrintUrl(parent.theMenu);
		var url = "";
	    var query = "";   
	    
	    if (document.all)
	    {
	       query = document.all.query0.value;
	    }
	    else if (document.getElementById("query0").value)
	    {
	       query = document.getElementById("query0").value;
	    }
	    else
	    {
	      query = document.getElementById("query0");
	        
	    }

		if (query == "")
		{
		   alert("No search parameter entered!");   
		}
		else
		{
	     	var collection = "";     
		     var path = document.location.pathname;
		     if (path.indexOf("/documentation/lg") >= 0)
		       collection = "&collection=documentation";
		     if (path.indexOf("/documentation/beta") >= 0)
		       collection = "&collection=doc_beta";
		     if (path.indexOf("/ndk/doc") >= 0)
		       collection = "&collection=dev_ndk";	       
			   
		   if (printSearchId && printSearchId != "")
		   {   
			     url = "http://search.novell.com/NSearch/SearchServlet?query0=" + query + "&id0=" + printSearchId + "&lang=en&encoding=iso-8859-1&synflag=false" + collection;
			     top.document.location = url;
		   }
		   else
		   {
				//See if tree title file is loaded page 
				var link =  parent.frames[3].document.location.pathname;
				if(link)
			  	{	//If loaded page is tree title then search the book
			    	if (link.indexOf("treetitl.html") >=0 && top.SequenceNum)
			      	{
						var IDs = top.SequenceNum.split(";");
						top.printList = "" + IDs.length + "_";
						for (var i=0; i < IDs.length; i++) 
							top.printList +=  (IDs[i].length + 3) + "_" + IDs[i].replace(/\s/g,"+") + "+en";

					    url = "http://search.novell.com/NSearch/SearchServlet?query0=" + query + "&id0=" + top.printList + "&lang=en&encoding=iso-8859-1&synflag=false" + collection;
					    top.document.location = url;
						return;
					}
				}

			    alert("There are no items checked!");
			}
		}
	 }  
	 else
	 {
	    alert("There is no book to search.");
	 }  
}
 

function checkEnter(event)
{ 	
	var code = 0;
	
	if ((document.layers) ? true : false)
		code = event.which;
	else
		code = event.keyCode;
	if (code==13)
	searchSelectedTopics();
}
