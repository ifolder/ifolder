
// Joust Outliner Version 2.5.4
// (c) Copyright 1996-2001, MITEM (Europe) Ltd. All rights reserved.
// This code may be freely copied and distributed provided that it is accompanied by this 
// header.  For full details of the Joust license, as well as documentation and help, go 
// to http://www.ivanpeters.com/.

// Do not modify anything between here and the "End of Joust" marker unless you know what you
// are doing.  
 
function getDHTMLObj(docName, objName) {
	if 	(theBrowser.hasW3CDOM) {
	        return eval(docName + '.getElementById("' + objName + '")');
	} else {
		return eval(docName + theBrowser.DHTMLRange + '.' + objName + theBrowser.DHTMLStyleObj);
	}
}
function getDHTMLObjStyle(docName, objName) {
	if 	(theBrowser.hasW3CDOM) {
                return eval(docName + '.getElementById("' + objName + '")').style;
	} else {
		return eval(docName + theBrowser.DHTMLRange + '.' + objName + theBrowser.DHTMLStyleObj);
	}
}
function getDHTMLObjTop(theObj) {return (theBrowser.code == "MSIE") ? theObj.pixelTop : theObj.top;}
function getDHTMLObjHeight(docName, objName) {
	if 	(theBrowser.hasW3CDOM) {
		return parseInt(eval(docName + '.getElementById("' + objName + '").offsetHeight'),10);
	} else {
		return eval(docName + theBrowser.DHTMLRange + '.' + objName + theBrowser.DHTMLDivHeight);
	}
}
function getDHTMLImg(docName, objName, imgName) {
	if 	(document.layers) {
		return getDHTMLObjStyle(docName, objName).document.images[imgName];
	} else {
		return eval(docName + '.images.' + imgName);
	}
}
function simpleArray() {this.item = 0;}
function imgStoreItem(n, s, w, h) {
	this.name = n;
	this.src = s;
	this.obj = null;
	this.w = w;
	this.h = h;
	if ((theBrowser.canCache) && (s)) {
		this.obj = new Image(w, h);
		this.obj.src = s;
	}
}
function imgStoreObject() {
	this.count = -1;
	this.img = new imgStoreItem;
	this.find = imgStoreFind;
	this.add = imgStoreAdd;
	this.getSrc = imgStoreGetSrc;
	this.getTag = imgStoreGetTag;
}
function imgStoreFind(theName) {
	var foundItem = -1;
	for (var i = 0; i <= this.count; i++) {if (this.img[i].name == theName) {foundItem = i;break;}}
	return foundItem;
}
function imgStoreAdd(n, s, w, h) {
	var i = this.find(n);
	if (i == -1) {i = ++this.count;}
	this.img[i] = new imgStoreItem(n, s, parseInt(w, 10), parseInt(h, 10));
}
function imgStoreGetSrc(theName) {
	var i = this.find(theName);
	var img = this.img[i];
	return (i == -1) ? '' : ((img.obj) ? img.obj.src : img.src);
}
function imgStoreGetTag(theName, iconID, altText) {
	var i = this.find(theName);
	if (i < 0) {return ''}
	with (this.img[i]) {
		if (src == '') {return ''}
		var tag = '<img src="' + src + '" width="' + w + '" height="' + h + '" border="0" align="left" hspace="0" vspace="0"';
		tag += (iconID != '') ? ' name="' + iconID + '"' : '';
		tag += ' alt="' + ((altText)?altText:'') + '">';
	}
	return tag;
}
// The MenuItem object.  This contains the data and functions for drawing each item.
function MenuItem (owner, id, type, text, url, nSearchID, status, nItem, pItem, parent) {
	var t = this;
	this.owner = owner;
	this.id = id;
	this.type = type;
	this.text = text;
	this.url = url;
	this.status = status;
	this.target = owner.defaultTarget;
	this.nextItem = nItem;
	this.prevItem = pItem;
	this.FirstChild = -1;
	this.parent = parent
	this.isopen = (owner == -1)? true:false;
	this.isSelected = false; 
	this.isChecked = true;	
	this.isChildChecked = false; 
	this.checkedIconName = MIGetCheckedIconName;
	this.nSearchID = nSearchID;
	this.draw = MIDraw;
	this.PMIconName = MIGetPMIconName;
	this.docIconName = MIGetDocIconName;
	this.setImg = MISetImage;
	this.setIsOpen = MISetIsOpen;
	this.setIsChecked = MISetIsChecked;
	this.setIsChildChecked = MISetIsChildChecked;		
	this.setSelected = MISetSelected;
	this.setIcon = MISetIcon;
	this.mouseOver = MIMouseOver;
	this.mouseOut = MIMouseOut;
	var i = (this.owner.imgStore) ? this.owner.imgStore.find(type) : -2;
	if (i == -1) {i = this.owner.imgStore.find('iconPlus');}
	this.height = (i > -1) ? this.owner.imgStore.img[i].h : 0;
}
function MIDraw (indentStr) {
	var o = this.owner;
	var mRef = '="return ' + o.reverseRef + "." + o.name;
	var tmp = mRef + '.entry[' + this.id + '].';
	var MOver = ' onMouseOver' + tmp + 'mouseOver(\''
	var MOut = ' onMouseOut' + tmp + 'mouseOut(\''
	var iconTag = o.imgStore.getTag(this.PMIconName(), 'plusMinusIcon' + this.id, '');  
	var aLine = '<nobr>' + indentStr;
	if (!this.noOutlineImg) {
		if (this.FirstChild != -1) {
			aLine += '<A HREF="#" onClick' + mRef + '.toggleExpand(' + this.id + ');"' + MOver + 'plusMinusIcon\',this);"' + MOut + 'plusMinusIcon\');">' + iconTag + '</A>';				
		} else {
			aLine += iconTag;
		}
	}
	var tip = (o.tipText == 'text') ? this.text : ((o.tipText == 'status') ? this.status : '');
	var theEntry = o.imgStore.getTag(this.docIconName(), 'docIcon' + this.id, tip) + this.text;
	var theImg = "";
	var checkImgLink = "";
	if (useCheckImages)
       	{   
      	  var img = o.imgStore.getTag(this.checkedIconName(), "checkIcon" + this.id, tip);      
          checkImgLink = '<A ID="CheckDocNavEntry' + this.id + '" HREF="#" onClick' + mRef + '.toggleChecked(' + this.id + ')">' + img + '</A>';
	   	}    
	if (useImages) theImg = o.imgStore.getTag(this.docIconName(), 'docIcon' + this.id, tip);
	var sTxt = '<SPAN CLASS="' + ((this.CSSClass) ? this.CSSClass : ((this.FirstChild != -1) ? 'node' : 'leaf')) + '">';
	var lTxt = '<A ID="DocNavEntry' + this.id + '" ';
	var theUrl = (((this.url == '') && theBrowser.canJSVoid && o.showAllAsLinks) || o.wizardInstalled) ? 'javascript:void(0);' : this.url;
	if (theUrl != '') {
		if (this.target.charAt(1) == "_") {theUrl = "javascript:" + o.reverseRef + ".loadURLInTarget('" + theUrl + "', '" + this.target + "');";}
			lTxt += ' HREF="' + theUrl + '" TARGET="' + this.target + '" onClick' + mRef + '.itemClicked(' + this.id + ');"'
			+ MOver + 'docIcon\',this);"' + MOut + 'docIcon\');"';
	}
	lTxt += (tip) ? ' TITLE="' + tip + '">' : '>';
	aLine += sTxt + checkImgLink + lTxt + theImg;
	if (this.multiLine) {
		aLine += '</A></SPAN><TABLE BORDER="0" CELLPADDING="0" CELLSPACING="0"><TR><TD>' + sTxt + lTxt + this.text + '</A></SPAN></TD></TR></TABLE>';
	} else {
		aLine += this.text + '</A></SPAN>';
	}
	aLine += '</nobr>';
	if ((theBrowser.hasW3CDOM) && (theBrowser.hasDHTML) && (!this.multiLine))  { aLine += '<br>'; }
	return aLine
}
function MIGetPMIconName() {
	var n = 'icon' + ((this.FirstChild != -1) ? ((this.isopen == true) ? 'Minus' : 'Plus') : 'Join');
	n += (this.id == this.owner.firstEntry) ? ((this.nextItem == -1) ? 'Only' : 'Top') : ((this.nextItem == -1) ? 'Bottom' : '');
	return n;
}
function MIGetDocIconName() {
	var is = this.owner.imgStore; var n = this.type;
	n += ((this.isopen) && (is.getSrc(n + 'Expanded') != '')) ? 'Expanded' : '';
	n += ((this.isSelected) && (is.getSrc(n + 'Selected') != '')) ? 'Selected' : '';
	return n;
}
function MIGetCheckedIconName() {
	var is = this.owner.imgStore;
	var n = ""; 
  
        if((this.isChecked) && (is.getSrc('iconChecked') != ''))
          {n = 'iconChecked';}
        else if((this.isChildChecked) && (is.getSrc('iconChildChecked') != ''))
          { n = 'iconChildChecked';}
        else if((is.getSrc('iconUnChecked') != ''))
          {n = 'iconUnChecked';}
	return n;
}
function MISetImage(imgID, imgName) {
	var o = this.owner; var s = o.imgStore.getSrc(imgName);
	if ((s != '') && (theBrowser.canCache) && (!o.amBusy)) {
		var img = (theBrowser.hasDHTML) ? getDHTMLImg(o.container + '.document', 'entryDIV' + this.id, imgID) : eval(o.container).document.images[imgID];
		if (img && img.src != s) {img.src = s;}                                                                      
	}
}
function MISetIsOpen (isOpen) {
	if ((this.isopen != isOpen) && (this.FirstChild != -1)) {
		this.isopen = isOpen;
		this.setImg('plusMinusIcon' + this.id, this.PMIconName());
		this.setImg('docIcon' + this.id, this.docIconName());
		return true;
	} else {
		return false;
	}
}
function MISetIsChecked (isChecked) {
	if ((this.isChecked != isChecked)) {
		this.isChecked = isChecked;
        this.setImg('checkIcon' + this.id, this.checkedIconName());
		return true;
	} else {
		return false;
	}
}
function MISetIsChildChecked (isChildChecked) {
       	this.isChildChecked = isChildChecked;
        this.setImg('checkIcon' + this.id, this.checkedIconName());
return isChildChecked;
}
function MISetSelected (isSelected) {
	this.isSelected = isSelected;

	var d = this.owner.container + '.document';
	var s = getDHTMLObj(d, 'DocNavEntry' + this.id);
        if (s && s.style)
        {
          if (isSelected) 
          {
            s.style.color="#cc0000";
            s.style.fontWeight = "bold";
          } 
          else
          { 
            s.style.color="#333333";
            s.style.fontWeight = "normal";
          }
        }
	this.setImg('docIcon' + this.id, this.docIconName());
	if ((this.parent >= 0) && this.owner.selectParents) {this.owner.entry[this.parent].setSelected(isSelected);}
}
function MISetIcon (newType) {
	this.type = newType;
	this.setImg('docIcon' + this.id, this.docIconName());
}
function MIMouseOver(imgName, theURL) {
	eval(this.owner.container).status = '';  //Needed for setStatus to work on MSIE 3 - Go figure!?
	var newImg = '';
	var s = '';
	if (imgName == 'plusMinusIcon') {
		newImg = this.PMIconName();
		s = 'Click to ' + ((this.isopen == true) ? 'collapse.' : 'expand.');
	} else {
		if (imgName == 'docIcon') {
			newImg = this.docIconName();
			s = (this.status != null) ? this.status : theURL;
		}
	}
	setStatus(s);
	if (theBrowser.canOnMouseOut) {this.setImg(imgName + this.id, newImg + 'MouseOver');}
	if(this.onMouseOver) {var me=this;eval(me.onMouseOver);}
	return true;
}
function MIMouseOut(imgName) {
	clearStatus();
	var newImg = '';
	if (imgName == 'plusMinusIcon') {
		newImg = this.PMIconName();
	} else {
		if (imgName == 'docIcon') {newImg = this.docIconName();}
	}
	this.setImg(imgName + this.id, newImg);
	if(this.onMouseOut) {var me=this;eval(me.onMouseOut);}
	return true;
}
// The Menu object.  This is basically an array object although the data in it is a tree.
function Menu () {
	this.count = -1;
	this.size = 0;
	this.version = '2.5.4';
	this.firstEntry = -1;
	this.autoScrolling = false;
	this.modalFolders = false;
	this.linkOnExpand = false;
	this.toggleOnLink = false;
	this.showAllAsLinks = false;
	this.savePage = true;
	this.name = 'theMenu';
	this.container = 'menu';
	this.reverseRef = 'parent';
	this.contentFrame = 'text';
	this.defaultTarget = 'text';
	this.menuLoaderFrame = 'menuChildLoader';
	this.tipText = 'none';
	this.selectParents = true;
	this.lastPMClicked = -1;
	this.selectedEntry = -1;
	this.wizardInstalled = false;
	this.amBusy = true;
	this.maxHeight = 0;
	this.imgStore = new imgStoreObject;
	this.entry = new MenuItem(this, 0, '', '', '', '', '', -1, -1, -1);
	this.contentWin = MenuGetContentWin;
	this.getEmptyEntry = MenuGetEmptyEntry;
	this.addEntry = MenuAddEntry;
	this.addMenu = MenuAddEntry;
	this.addChild = MenuAddChild;
	this.rmvEntry = MenuRmvEntry;
	this.rmvChildren = MenuRmvChildren;
	this.draw = MenuDraw;
	this.drawALevel = MenuDrawALevel;
	this.refresh = MenuRefresh;
	this.reload = MenuReload;
	this.refreshDHTML = MenuRefreshDHTML;
	this.scrollTo = MenuScrollTo;
	this.itemClicked = MenuItemClicked;
	this.selectEntry = MenuSelectEntry;
	this.setEntry = MenuSetEntry;
	this.setChecked = MenuSetChecked;
	this.setEntryByURL = MenuSetEntryByURL;
	this.setAllChildren = MenuSetAllChildren;
	this.setAllChildrenChecked = MenuSetAllChildrenChecked;
	this.setParentsChecked = MenuSetParentsChecked;
	this.setAll = MenuSetAll;
	this.openAll = MenuOpenAll;
	this.closeAll = MenuCloseAll;
	this.findEntry = MenuFindEntry;
	this.toggleExpand = MenuToggleExpand;
	this.toggleChecked = MenuToggleChecked; 
	this.loadScript = MenuLoadScript;
	this.currentSubRoot = -1;
	this.footer = "";      
	this.showFooter = true;
	this.markChildrenForDelete = -1;
}
function MenuGetContentWin() {
	return eval(((myOpener != null) ? 'myOpener.' : 'self.') + this.contentFrame);
}
function MenuGetEmptyEntry() {
	for (var i = 0; i <= this.count; i++) {if (this.entry[i] == null) {break;}}
	if (i > this.count) {this.count = i};
	return i
}
function MenuAddEntry (addTo, type, text, url, nsearchid, status, insert) {
	if (!insert) {insert=false;}
	var theNI = -1;var theP = -1;var thePI = -1;
	if (addTo < 0) {
		var i = addTo = this.firstEntry;
		if (!insert) {while (i > -1) {addTo = i;i = this.entry[i].nextItem;}}
	}
	if (addTo >= 0) {
		var e = this.entry[addTo];
		if (!e) {return -1;}
		thePI = (insert)?e.prevItem:addTo;
		theNI = (insert)?addTo:e.nextItem;
		theP = e.parent;
	}
	var eNum = this.getEmptyEntry();
	if (thePI >= 0) {
		this.entry[thePI].nextItem = eNum;
	} else {
		if (theP >= 0) {
			this.entry[theP].FirstChild = eNum;
		} else {
			this.firstEntry = eNum;
		}
	}
	if (theNI >= 0) {this.entry[theNI].prevItem = eNum;}
	{
		this.entry[eNum] = new MenuItem(this, eNum, type, text, url, nsearchid, text, theNI, thePI, theP);
		if(this.entry[theP]) this.entry[eNum].isChecked = this.entry[theP].isChecked;
	} 
	this.size++;
	return eNum;
}
function MenuAddChild (addTo, type, text, url, nsearchid, status, insert) {

	if (!insert) {insert=false;}
	var eNum = -1;
	if ((this.count == -1) || (addTo < 0)) {      
	        this.rmvEntry(this.firstEntry);
                eNum = this.addEntry(-1, type, text, url, nsearchid, text, false);       
	} else {
		var e = this.entry[addTo];
		if (!e) {return -1;}
		var cID = e.FirstChild;
		if (cID < 0) {
			e.FirstChild = eNum = this.getEmptyEntry();
			this.entry[eNum] = new MenuItem(this, eNum, type, text, url, nsearchid, text, -1, -1, addTo);
			if(this.entry[addTo]) this.entry[eNum].isChecked = this.entry[addTo].isChecked;
		} else {
			while (!insert && (this.entry[cID].nextItem >= 0)) {cID = this.entry[cID].nextItem;}
			eNum = this.addEntry(cID, type, text, url, nsearchid, text, insert);
		}
	}
	return eNum;
}
function MenuRmvEntry (theEntry) {
	var e = this.entry[theEntry];
	if (e == null) {return;}
	var p = e.prevItem;
	var n = e.nextItem;
	if (e.FirstChild > -1) {this.rmvChildren(theEntry);}
	if (this.firstEntry == theEntry) {this.firstEntry = n}
	if (this.selectedEntry == theEntry) {this.selectedEntry = n}
	if (p > -1) {
		this.entry[p].nextItem = n;
	} else { 
		if (e.parent > -1) {
			this.entry[e.parent].FirstChild = n;
		} else {
			if (this.firstEntry == theEntry) {this.firstEntry = n}
		}
	} 
	if (n > -1) {this.entry[n].prevItem = p;}
	this.entry[theEntry] = null; 
	this.size--;
}
function MenuRmvChildren (theP) {
	var eNum;var e;var tmp;
	if (theP == -1) {
		eNum = this.firstEntry;
		this.firstEntry = -1;
	} else {
		eNum = this.entry[theP].FirstChild;
		this.entry[theP].FirstChild = -1;
	}
	while (eNum > -1) {
		e = this.entry[eNum];
		if (e.FirstChild > -1) {this.rmvChildren(eNum);}
		if (this.selectedEntry == eNum) {this.selectedEntry = e.parent;}
		tmp = eNum;
		eNum = e.nextItem;
		this.entry[tmp] = null; 
		this.size--;      
	}
}
function MenuDraw() {
	this.maxHeight = 0;
	var theDoc = eval(this.container + ".document");
	eval(this.container).document.writeln(this.drawALevel(this.firstEntry, '', true, theDoc));
	if (theBrowser.hasDHTML) {
		for (var i = 0; i <= this.count; i++) {
			if (this.entry[i]) {
				this.maxHeight += getDHTMLObjHeight(this.container + '.document', 'entryDIV' + i);
			}
		}
	} else {
		if ((this.lastPMClicked > 0) && theBrowser.mustMoveAfterLoad && this.autoScrolling) {
			this.scrollTo(this.lastPMClicked);
		}
	}
}
function MenuDrawALevel(firstItem, indentStr, isVisible, theDoc) {
	var currEntry = firstItem;
	var padImg = "";
	var aLine = "";
	var theLevel = "";
	var e = null;
	while (currEntry > -1) {
		e = this.entry[currEntry];
		aLine = e.draw(indentStr);
		if (theBrowser.hasDHTML) {
			aLine = '<DIV ID="entryDIV' + currEntry + '" CLASS="menuItem">' + aLine + '</DIV>';
		} else {
			aLine += '<BR CLEAR="ALL">';
		}
		theBrowser.lineByLine = true;
		if (theBrowser.lineByLine) {theDoc.writeln(aLine);} else {theLevel += aLine;}
		if ((e.FirstChild > -1) && ((theBrowser.hasDHTML || (e.isopen && isVisible)))) {
			padImg = (e.noOutlineImg) ? '' : this.imgStore.getTag((e.nextItem == -1) ? 'iconBlank' : 'iconLine', '', '');
			theLevel += this.drawALevel(e.FirstChild, indentStr + padImg, (e.isopen && isVisible), theDoc);
		}
		currEntry = e.nextItem;
	}
	return theLevel;
}
function MenuRefresh() {
	if (theBrowser.hasDHTML) {
		if (!this.amBusy) {
			this.refreshDHTML();
			if (this.autoScrolling) {this.scrollTo(this.lastPMClicked);}
		}
	} else {
		this.reload();
	}
}
function MenuReload() {
	if (!this.amBusy) {
		this.amBusy = true;
		var l = eval(this.container).location;
		var rm = theBrowser.reloadMethod;
		var newLoc = fixPath(l.pathname);
		var s = '';
		if (l.search) {s = l.search;}
		if (theBrowser.needsMenuSearch) {
			if (s == '') {
				s = '?jtoggle=1';
			} else {
				var p = s.indexOf('jtoggle=');
				if (p < 0) {
					s += '&jtoggle=1';
				} else {
					var t = (s.substring(p + 8, p + 9) == "1") ? "2" : "1";
					s = s.substring(0, p+8) + t;
				}
			}
		}
		newLoc += s;
		if (this.autoScrolling && (this.lastPMClicked > 0) && !theBrowser.mustMoveAfterLoad) {
			newLoc += "#joustEntry" + this.lastPMClicked;
		}
		if (rm == 'replace') {
			l.replace(newLoc);
		} else {
			if (rm == 'reload') {
				l.reload();
			} else {
				if (rm == 'timeout') {
					setTimeout(this.container + ".location.href ='" + newLoc + "';", 100);
				} else {
					l.href = newLoc;
				}
			}
		}
	}
}
function MenuRefreshDHTML() {
	var nextItemArray = new simpleArray;
	var currEntry = this.firstEntry;
	var level = (currEntry == -1) ? 0 : 1;
	var isVisible = true;
	var lastVisibleLevel = 1;
	var co = eval(this.container);
	var yPos = co.menuStart;
	var d = this.container + '.document';
	var e = null;
	var s = null;
	while (level > 0) {
		e = this.entry[currEntry];
		s = getDHTMLObjStyle(d, 'entryDIV' + currEntry); 
               
		if (isVisible) {
			s.top = yPos;
			s.visibility = 'visible';
			yPos += getDHTMLObjHeight(d, 'entryDIV' + currEntry);
			lastVisibleLevel = level;
		} else {
			s.visibility = 'hidden';
			s.top = 0;
		}
		if (e.FirstChild > -1) {
			isVisible = (e.isopen == true) && isVisible;
			nextItemArray[level++] = e.nextItem;
			currEntry = e.FirstChild;
		} else {
			if (e.nextItem != -1) {
				currEntry = e.nextItem;
			} else {
				while (level > 0) {
					if (nextItemArray[--level] != -1) {
						currEntry = nextItemArray[level];
						isVisible = (lastVisibleLevel >= level);
						break;
					}
				}
			}
		}
	}
	this.maxHeight = yPos;
	co.setMenuHeight(yPos);  
	if (this.markChildrenForDelete > -1)
	{   
	         
	  if (this.size > 100 && (!this.entry[this.markChildrenForDelete].isChildChecked))
	  {  
	     this.rmvChildren(this.markChildrenForDelete);
	     this.entry[this.markChildrenForDelete].FirstChild = -2;
	     this.markChildrenForDelete = -1;
	  }
	}
}
function MenuScrollTo(entryNo) {
	if (theBrowser.hasDHTML) {
		var e = this.entry[entryNo];
		if (!e) {return;}
		var co = eval(this.container);
		var d = this.container + '.document';
		var srTop = getDHTMLObjTop(getDHTMLObjStyle(d, 'entryDIV' + entryNo));
		var srBot = (e.nextItem > 0) ? getDHTMLObjTop(getDHTMLObjStyle(d, 'entryDIV' + e.nextItem)) : this.maxHeight;
		if (theBrowser.code == 'MSIE') {
			var curTop = co.document.body.scrollTop;
			var curBot = curTop + co.document.body.clientHeight;
		
		} else {
			var curTop = co.pageYOffset;
			var curBot = curTop + co.innerHeight;
		}
		if ((srBot > curBot - 20) || (srTop < curTop + 20)) {
			var scrBy = srBot - curBot + (curBot - curTop)/2;
			if (srTop < (curTop + scrBy)) { 
			  scrBy = srTop - curTop - (curBot - curTop)/2;;
		          }
			co.setTimeout('self.scrollBy(0, ' + scrBy + ');', 100);
		}
	} else {
		var l = fixPath(eval(this.container).location.pathname) + '#joustEntry' + entryNo;
		setTimeout(this.container + '.location.href = "' + l + '";', 100);
	}
}
function MenuItemClicked(entryNo, fromToggle) {
	var r = true;
	var e = this.entry[entryNo];
	var w = this.contentWin();
	var b = theBrowser;
        
	if (!fromToggle){
	   this.selectEntry(entryNo);
	}
	if (this.wizardInstalled) {w.menuItemClicked(entryNo);}
	if(e.onClickFunc) {e.onClick = e.onClickFunc;}
	if(e.onClick) {var me=e;if(eval(e.onClick) == false) {r = false;}}
	if (r) {
		if (((this.toggleOnLink) && (e.FirstChild != -1) && !(fromToggle)) || e.noOutlineImg) {
			if (b.hasDHTML) {
				this.toggleExpand(entryNo, true);
			} else {
				setTimeout(this.name + '.toggleExpand(' + entryNo + ', true);', 100);
			}
		}
	}
	return (e.url != '') ? r : false;
}
function MenuSelectEntry(entryNo) { 
        
	var oe = this.entry[this.selectedEntry];
	if (oe) {oe.setSelected(false);}
	var e = this.entry[entryNo];
	if (e) {e.setSelected(true);}
	this.selectedEntry = entryNo;
       //	this.refresh();
}
function MenuSetEntry(entryNo, state) {
	var cl = ',' + entryNo + ',';
	var e = this.entry[entryNo];
	this.lastPMClicked = entryNo;
	var mc = e.setIsOpen(state);
	var p = e.parent;
	while (p >= 0) { 
		cl += p + ',';
		e = this.entry[p];
		mc |= (e.setIsOpen(true));
		p = e.parent; 
	
	}
	if (this.modalFolders) {
		for (var i = 0; i <= this.count; i++) {
			e = this.entry[i];
			if ((cl.indexOf(',' + i + ',') < 0) && e) {mc |= e.setIsOpen(false);}
		}
	}
	return mc;
}
function MenuSetChecked(entryNo, state) {
	var cl = ',' + entryNo + ',';
	var e = this.entry[entryNo];
       	this.lastPMClicked = entryNo;
	var mc = e.setIsChecked(state); 
	if (!state) e.setIsChildChecked(state);
	return mc; 
}
function MenuSetEntryByURL(theURL, state) {
	var i = this.findEntry(theURL, 'url', 'right', 0);
	return (i != -1) ? this.setEntry(i, state) : false;
}
function MenuSetAllChildren(state, parentID) {
	var hasChanged = false;
	var currEntry = (parentID > -1) ? this.entry[parentID].FirstChild : this.firstEntry;
	while (currEntry > -1) {
		var e = this.entry[currEntry];
		hasChanged |= e.setIsOpen(state);
		if (e.FirstChild > -1) {hasChanged |= this.setAllChildren(state, currEntry);}
		currEntry = e.nextItem;
	}
	return hasChanged;
}
function MenuSetAllChildrenChecked(state, parentID) {
 	var hasChanged = false;
	var currEntry = (parentID > -1) ? this.entry[parentID].FirstChild : this.firstEntry;
	while (currEntry > -1) {
		var e = this.entry[currEntry];
		hasChanged |= e.setIsChecked(state);   
		if(!state) e.setIsChildChecked(state)
		if (e.FirstChild > -1) {hasChanged |= this.setAllChildrenChecked(state, currEntry);}
		currEntry = e.nextItem;
	}
	return hasChanged;
}
function MenuSetParentsChecked(entryNo) {
 	var hasChanged = false;
	if (entryNo > -1)
       	{
       	  var parentID = this.entry[entryNo].parent;
       	  var currEntry = (parentID > -1) ? this.entry[parentID].FirstChild : this.firstEntry;
	  var allSelected = true;
	  var children = false;
	  while (currEntry > -1) 
	  {
	  	var e = this.entry[currEntry];
                if(!e.isChecked)  allSelected = false;
                if(e.isChildChecked || e.isChecked) children = true;

		currEntry = e.nextItem;
	  }
	 
	  var e = this.entry[parentID];
	  
	  if (e)
	  {
	  if (allSelected) {  e.setIsChecked(true); hasChanged=true; }
	  else if (children){e.isChecked = false; e.setIsChildChecked(true); hasChanged=true;}
	  else { e.isChecked=false; e.setIsChildChecked(false); hasChanged=true; }
	  this.setParentsChecked(e.id);
	  }
       }
       return hasChanged;
}
function MenuSetAll(state, parentID) {
	if (theBrowser.version >= 4) {
		if (parentID == 'undefined') {parentID = -1;}
	} else {
		if (parentID == null) {parentID = -1;}
	}
	var hasChanged = false;
	if (parentID > -1) {hasChanged |= this.entry[parentID].setIsOpen(state);}
	hasChanged |= this.setAllChildren(state, parentID);
	if (hasChanged) {
		this.lastPMClicked = this.firstEntry;
		this.refresh();
	}
}
function MenuOpenAll() {this.setAll(true, -1);}
function MenuCloseAll() {this.setAll(false, -1)}
function MenuFindEntry(srchVal, srchProp, matchType, start) {
	var e;
	var sf;
	if (srchVal == "") {return -1;}
	if (!srchProp) {srchProp = "url";}
	if (!matchType) {matchType = "exact";}
	if (!start) {start = 0;}
	if (srchProp == "URL") {srchProp = "url";}
	if (srchProp == "title") {srchProp = "text";}
	eval("sf = cmp_" + matchType);
	for (var i = start; i <= this.count; i++) {
		if (this.entry[i]) {
			e = this.entry[i];
			if (sf(eval("e." + srchProp), srchVal)) {return i;}
		}		
	}
	return -1;
}
function cmp_exact(c, s) {return (c == s);}
function cmp_left(c, s) {
	var l = Math.min(c.length, s.length);
	return ((c.substring(1, l) == s.substring(1, l)) && (c != ""));
}
function cmp_right(c, s) {
        c = c.replace(/\.\./,"");
        c = c.replace(/\\/g,"/");
        s = s.replace(/\\/g,"/");
       	var l = Math.min(c.length, s.length);
	return ((c.substring(c.length-l) == s.substring(s.length-l)) && (c != ""));
}
function cmp_contains(c, s) {return (c.indexOf(s) >= 0);}
function MenuToggleExpand(entryNo, fromClicked) {
	var r = true;
	var e = this.entry[entryNo];
	if (e.onToggle) {var me=e;if(eval(e.onToggle) == false) {r = false;}}
	if (r) {
		var chg = this.setEntry(entryNo, e.isopen ^ 1);
		if (this.linkOnExpand && e.isopen) {
			if (e.url != '') {loadURLInTarget(e.url, e.target);}
			if (!fromClicked) {this.itemClicked(entryNo, true);}
		}

		if (chg) {this.refresh();}
	}
	return false;
}
function MenuToggleChecked(entryNo, fromClicked) {
	var e = this.entry[entryNo];
	var chg = this.setChecked(entryNo, e.isChecked ^ 1);
	var cchg = this.setAllChildrenChecked(e.isChecked,entryNo);
	var pchg = this.setParentsChecked(entryNo);
	if (chg || cchg || pchg) {this.refresh();}
	return false;
    
}  
function MenuLoadScript(parentPageURL, firstChildPage, fromToggle)
{   
   var chapterID = this.findEntry(parentPageURL, 'url', 'right'); 
   var e = this.entry[chapterID]; 
   if ( chapterID == -1 || e.FirstChild == -2)
   {                                        
	 	var re = /\\/g;
		var parentPath = parent.location.pathname.replace(re,'/');
//        var parentLocation = parent.location.protocol +"//"+ parent.location.host + escape(parentPath).substring(0,parentPath.lastIndexOf("/"));
        var parentLocation = parent.location.protocol +"//"+ parent.location.host + parentPath.substring(0,parentPath.lastIndexOf("/"));
       	firstChildPage = firstChildPage.substring(firstChildPage.indexOf("/")+1)    
       	
       	var absoluteLocation = parentLocation + firstChildPage.substring(firstChildPage.indexOf("/"));
       	top.frames[2].location.replace(absoluteLocation);
   } 
   else if ( e.isopen && fromToggle)
   {   
      this.markChildrenForDelete = chapterID;
   }

}



// Other functions
function DrawMenu(m) {
	m.draw();
}
function browserInfo() {
	this.code = 'unknown';
	this.version = 0;
	this.platform = 'Win';
	var ua = navigator.userAgent;
	var i = ua.indexOf('WebTV');
	if (i >= 0) {
		this.code = 'WebTV';
		i += 6;
	} else {
		i = ua.indexOf('Opera');
		if (i >= 0) {
			this.code = 'OP';
			i = ua.indexOf(') ') + 2;
		} else {
			i = ua.indexOf('MSIE');
			if (i >= 0) {
				this.code = 'MSIE';
				i += 5;
			} else {
				i = ua.indexOf('Mozilla/');
				if (i >= 0) {
					this.code = 'NS';
					i += 8;
				}
			}
		}
	}
	this.version = parseFloat(ua.substring(i, i+4));
	if (ua.indexOf('Mac') >= 0) {this.platform = 'Mac';}
	if (ua.indexOf('OS/2') >= 0) {this.platform = 'OS/2';}
	if (ua.indexOf('X11') >= 0) {this.platform = 'UNIX';}
	var xx = navigator.appName;
	var v = this.version;
	var p = this.platform;
	var NS = (this.code == 'NS');
	var IE = (this.code == 'MSIE');
	var WTV = (this.code == 'WebTV');
	var OP = (this.code == 'OP');
	var OP32up = (OP && (v >= 3.2));
	var OP5up = (OP && (v >= 5));
	var IE4up = (IE && (v >= 4));
	var NS3up = (NS && (v >= 3));
	var NS6up = (NS && (v >= 5));
	this.NWCode = ((xx.indexOf('NetWare') >= 0) || (ua.indexOf('ICEbrowser') >= 0));
	this.canCache = NS3up || IE4up || OP32up || WTV;
	this.canOnMouseOut = this.canCache;
	this.canOnError = NS3up || IE4up || OP32up;
	this.canJSVoid = !((NS && !NS3up) || (IE && !IE4up) || (OP && (v < 3.5)));
	this.lineByLine = (v < 4);
	this.mustMoveAfterLoad = NS3up || (IE4up && (p != 'Mac')) || WTV;
	if (NS6up == true) {
		this.reloadMethod = 'reload';
	} else {
		if (NS3up || IE4up || WTV || OP5up) {
			this.reloadMethod = 'replace';
		} else {
			this.reloadMethod = (NS && (v == 2.01) && (p != 'Win')) ? 'timeout' : 'href';
		}
	}
	this.needsMenuSearch = (OP && !OP5up);
	this.canFloat = NS || (IE && !((p == 'Mac') && (v >= 4) && (v < 5)));
	this.hasDHTML = ((NS || IE) && (v >= 4)) && !(IE && (p == 'Mac') && (v < 4.5)) || this.NWCode;
	this.slowDHTML = IE4up || NS6up;
	this.hasW3CDOM = (document.getElementById) ? true : false;
	this.needLM = (!this.hasW3CDOM && NS) || (IE && (p == 'Mac') && (v >= 4.5));
	this.DHTMLRange = IE ? '.all' : '';
	this.DHTMLStyleObj = IE ? '.style' : '';
	this.DHTMLDivHeight = IE ? '.offsetHeight' : '.clip.height';
}
function getWindow() {return (floatingMode) ? myOpener : self;}
function setStatus(theText) {
	var theWindow = getWindow();
	if (theWindow) {
		theWindow.status = theText;
		if (!theBrowser.canOnMouseOut) {
			clearTimeout(statusTimeout);
			statusTimeout = setTimeout('clearStatus()', 5000);
		}
	}
	return true;
}
function clearStatus() {
	var theWindow = getWindow();
	if (theWindow) {theWindow.status = '';}
}
function unloadFloating() {
	if (myOpener) {
		if (myOpener.JoustFrameset) {myOpener.setTimeout('menuClosed();', 100);}
	}
}
function getMode() {
	var theMode = getParm(document.cookie, 'mode', ';');
	return ((theMode == "Floating") || (theMode == "NoFrames")) ? theMode : "Frames";
}
function smOnError (msg, url, lno) {
	smCallerWin.onerror = oldErrorHandler;
	if (confirm(smSecurityMsg)) {setTimeout('setMode("' + smNewMode + '");', 100);}
	return true;
}
function smSetCookie(theMode) {
	document.cookie = 'mode=' + theMode + '; path=/';
	if (getMode() != theMode) {
		alert(smCookieMsg);
		return false;
	} else {
		return true;
	}
}
function setMode(theMode, callerWin) {
	smNewMode = theMode
	smCallerWin = (theBrowser.code == 'NS') ? callerWin : self;
	var okToGo = true;
	var currentMode = getMode();
	if (theMode != currentMode) {
		if (currentMode == 'Floating') {
			if (smSetCookie(theMode)) {self.close();}
		} else {
			var dest = '';
			if (theBrowser.canFloat) {
				if ((theMenu.savePage) && (callerWin)) {
					if (theBrowser.canOnError) {
						oldErrorHandler = smCallerWin.onerror;
						smCallerWin.onerror = smOnError;
					}
					var l = theMenu.contentWin().location;
					var p = l.pathname;
					if (theBrowser.canOnError) {smCallerWin.onerror = oldErrorHandler;}
					if (p) {
						dest = fixPath(p) + l.search;
					} else {
						if (!confirm(smSecurityMsg)) {okToGo = false;}
					}
				}
			} else {
				okToGo = false;
			}
			if (okToGo && smSetCookie(theMode)) {
				if (theMode == 'NoFrames') {
					location.href = (index3 == '') ? ((dest == '') ? '/' : dest) : index3;
				} else {
					location.href = index2 + ((dest == '') ? '' : '?page=' + escape(dest));
				}
			}
		}
	}
}
function fixPath(p) {
	var i = p.indexOf('?', 0);
	if (i >= 0) {p = p.substring(0,i);}
	if (p.substring(0,2) == '/:') {p = p.substring(p.indexOf('/', 2), p.length);}
	i = p.indexOf('\\', 0);
	while (i >= 0) {
		p = p.substring(0,i) + '/' + p.substring(i+1,p.length);
		i = p.indexOf('\\', i);
	}
	return p;
}
function fileFromPath(p) {
	p = fixPath(p);
	var i = p.lastIndexOf('\\');
	if (i >= 0) {p = p.substring(i+1,p.length);}
	return p;
}
function getParm(theStr, parm, delim) {
	// returns value of parm from string
	if (theStr.length == 0) {return '';}
	var sPos = theStr.indexOf(parm + "=");
	if (sPos == -1) {return '';}
	sPos = sPos + parm.length + 1;
	var ePos = theStr.indexOf(delim, sPos);
	if (ePos == -1) {ePos = theStr.length;}
	return unescape(theStr.substring(sPos, ePos));
}
function pageFromSearch(def, m, selIt) {
	var s = self.location.search;
	if ((s == null) || (s.length <= 1)) {return def;}
	var p = getParm(s, 'page', '&');
	p = (p != '') ? fixPath(p) : def;
	if (m != null) {
		var e = m.findEntry(p, 'URL', 'exact');
		if ((e != -1) && selIt) {
			m.setEntry(e, true);
			m.selectEntry(e);
		}
	}
	return p + self.location.hash;
}
function loadURLInTarget(u, t) {
	var w = eval("self." + t);
	if (!w && myOpener) {w = eval("myOpener." + t);}
	if (!w && ("_top,_parent,_self".indexOf(t) >= 0)) {
		w = eval("getWindow()." + t.substring(1));}
	if (w) {w.location.href = u;} else {window.open(u, t);}
} 

function defOnError(msg, url, lno) {
	if (jsErrorMsg == '') {
		return false;
	} else {
		alert(jsErrorMsg + '.\n\nError: ' + msg + '\nPage: ' + url + '\nLine: ' + lno + '\nBrowser: ' + navigator.userAgent);
		return true;
	}
}
function defaultResizeHandler() {
	if ((theBrowser.code == "NS") && theBrowser.hasDHTML && (self.frames.length != 0)) {
		if (!eval(theMenu.container + ".document.menuBottom")) {
			theMenu.reload();
		}
	}
}
// Declare global variables
var theBrowser = new browserInfo;

var jsErrorMsg = 'A JavaScript error has occurred on this page!  Please note down the ';
jsErrorMsg += 'following information and pass it on to the Webmaster.';
if (theBrowser.canOnError) {self.onerror = defOnError;}

var theMenu = new Menu;
var JoustFrameset = true;
var statusTimeout = 0;
var index1 = 'index.htm';
var useImages = false;    
var useCheckImages = true;
var smCallerWin;
var smNewMode;
var oldErrorHandler;
var smNoFloat = 'Sorry, your browser does not support this feature!';
var smCookieMsg = 'You must have Cookies enabled to change the display mode!';
var smSecurityMsg = 'Due to security restrictions imposed by your browser, I cannot ';
smSecurityMsg += 'change modes while a page from another server is being displayed. ';
smSecurityMsg += 'The default home page for this site will be displayed instead.';

var floatingMode = (getMode() == 'Floating');
var myOpener = null;
if (floatingMode == true) {
	if (self.opener) {
		myOpener = self.opener;
		if (myOpener.JoustFrameset) {myOpener.setTimeout('setGlobals();', 100);}
	} else {
		document.cookie = 'mode=Frames; path=/';
		floatingMode = false;
	}
} else {
	if (getMode() != 'Frames') {document.cookie = 'mode=Frames; path=/';}
}

//	############################   End of Joust   ############################

function initOutlineIcons(imgStore) {
	var ip = '../docui2002/images/menu/';
     //	ip += (theBrowser.platform == 'Mac') ? 'mac/' : ((theBrowser.platform == 'OS/2') ? 'os2/' : 'win/');
	
	imgStore.add('iconPlusTop', ip + 'plustop.gif', 18, 18);
	imgStore.add('iconPlus', ip + 'plus.gif', 18, 18);
	imgStore.add('iconPlusBottom', ip + 'plusbottom.gif', 18, 18);
	imgStore.add('iconPlusOnly', ip + 'plusonly.gif', 18, 18);
	imgStore.add('iconMinusTop', ip + 'minustop.gif', 18, 18);
	imgStore.add('iconMinus', ip + 'minus.gif', 18, 18);
	imgStore.add('iconMinusBottom', ip + 'minusbottom.gif', 18, 18);
	imgStore.add('iconMinusOnly', ip + 'minusonly.gif', 18, 18);
	imgStore.add('iconLine', ip + 'line.gif', 18, 18);
	imgStore.add('iconBlank', ip + 'blank.gif', 18, 18);
	imgStore.add('iconJoinTop', ip + 'jointop.gif', 18, 18);
	imgStore.add('iconJoin', ip + 'join.gif', 18, 18);
	imgStore.add('iconJoinBottom', ip + 'joinbottom.gif', 18, 18);  
	imgStore.add('iconChecked', ip + 'checked.gif', 18, 18);
	imgStore.add('iconUnChecked', ip + 'uchecked.gif', 18, 18);
	imgStore.add('iconChildChecked', ip + 'gchecked.gif', 18, 18);

	//Add folder and document images to the imgStore.
	imgStore.add('Folder', ip + 'folderclosed.gif', 18, 18);
	
	var di = '../docui2002/images/menu/';
	if ((theBrowser.code == 'NS') || (theBrowser.code == 'MSIE')) {
		di += theBrowser.code.toLowerCase() + '_doc';
		imgStore.add('Document', di + '.gif', 18, 18);
		imgStore.add('DocumentMouseOver', di + '_mo.gif', 18, 18);
		imgStore.add('DocumentSelected', di + '_sel.gif', 18, 18);
	} else {
		imgStore.add('Document', di + 'doc.gif', 18, 18);
	}
}  


