# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Book_CSFILES = $(srcdir)/Main.cs
Book_CSFILES_CSC := $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES =
Book_FLAGS = $(CSC_EXEFLAG)
Book_LIBS = $(BookLib_LIBS) Novell.AddressBook.UI.gtk.dll
Book_LIBPATH = $(BookLib_LIBPATH)

BookLib_CSFILES = $(srcdir)/ContactBrowser.cs $(srcdir)/OptionButton.cs $(srcdir)/ContactEditor.cs $(srcdir)/../FormsBookLib/FullNameParser.cs $(srcdir)/BookEditor.cs $(srcdir)/NameEditor.cs $(srcdir)/ContactPicker.cs
BookLib_CSFILES_CSC := $(subst /,$(SEP),$(BookLib_CSFILES))
BookLib_INCLUDES = 
BookLib_RESOURCES = $(srcdir)/contact-browser.glade contact-picker.glade name-editor.glade contact-editor.glade
BookLib_FLAGS = $(CSC_LIBFLAG)
BookLib_LIBS = System.dll System.Drawing.dll System.Data.dll glade-sharp.dll gnome-sharp.dll
BookLib_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib

DATA_FILES = vcardimport.png vcardexport.png book.png briefcase.png contact.png contact_me.png cellphone.png connect_to_url-16.xpm galeon.png globe.png house.png ico-mail.png blankhead.png contact-browser.glade contact-picker.glade name-editor.glade contact-editor.glade

EXTRA_DIST = $(Book_CSFILES) $(BookLib_CSFILES) $(Book_RESOURCES) $(BookLib_RESOURCES) $(DATA_FILES) $(INSTALL_FILES)

all: ContactBrowser.exe Novell.AddressBook.UI.gtk.dll

ContactBrowser.exe: $(Book_CSFILES) $(Book_RESOURCES) Novell.AddressBook.UI.gtk.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:ContactBrowser.doc.xml

Novell.AddressBook.UI.gtk.dll: $(BookLib_CSFILES) $(BookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(BookLib_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(BookLib_LIBS:%=/r:%) $(BookLib_LIBPATH:%=/lib:%) $(BookLib_RESOURCES:%=$(ICON_FLAG)%) $(BookLib_CSFILES_CSC) /doc:Novell.AddressBook.UI.gtk.doc.xml

install-exec-local: ContactBrowser.exe Novell.AddressBook.UI.gtk.dll contactbrowser $(DATA_FILES) 
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ContactBrowser.exe Novell.AddressBook.UI.gtk.dll $(DATA_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/contactbrowser $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f ContactBrowser.exe Novell.AddressBook.UI.gtk.dll $(DATA_FILES) $(BookLib_RESOURCES)
	cd $(DESTDIR)$(bindir); rm -f contactbrowser
    
installcheck-local: install
	
CLEAN_FILES = ContactBrowser.exe ContactBrowser.doc.xml Novell.AddressBook.UI.gtk.dll Novell.AddressBook.UI.gtk.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

