# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Book_CSFILES = $(srcdir)/Main.cs
Book_CSFILES_CSC := $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES =
Book_FLAGS = $(CSC_EXEFLAG)
Book_LIBS = $(BookLib_LIBS) Novell.AddressBook.UI.gtk.dll
Book_LIBPATH = $(BookLib_LIBPATH)


IA_CSFILES = $(srcdir)/InviteApp.cs
IA_CSFILES_CSC := $(subst /,$(SEP),$(IA_CSFILES))
IA_INCLUDES = 
IA_RESOURCES =
IA_FLAGS = $(CSC_EXEFLAG)
IA_LIBS = Novell.AddressBook.dll Novell.AddressBook.UI.gtk.dll
IA_LIBPATH = ../AddressBook

BookLib_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Util.cs $(srcdir)/ContactBrowser.cs $(srcdir)/OptionButton.cs $(srcdir)/ContactEditor.cs $(srcdir)/../FormsBookLib/FullNameParser.cs $(srcdir)/BookEditor.cs $(srcdir)/NameEditor.cs $(srcdir)/ContactPicker.cs $(srcdir)/CollectionGeneralPage.cs $(srcdir)/ContactCollectionSharingPage.cs $(srcdir)/CollectionPropertiesPage.cs $(srcdir)/CollectionProperties.cs $(srcdir)/InvitationAssistant.cs

BookLib_CSFILES_CSC := $(subst /,$(SEP),$(BookLib_CSFILES))
BookLib_INCLUDES = 
BookLib_RESOURCES = $(srcdir)/contact-browser.glade contact-picker.glade name-editor.glade contact-editor.glade
BookLib_FLAGS = $(CSC_LIBFLAG)
BookLib_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.AddressBook.dll
BookLib_LIBPATH = ../AddressBook

GLADE_FILES = invitation-assistant.glade invitation.png collection-properties.glade vcardimport.png vcardexport.png book.png contact.png blankhead.png contact-browser.glade contact-picker.glade name-editor.glade contact-editor.glade

IMAGE_FILES = property.png invited-contact.png vcardimport.png vcardexport.png book.png contact.png contact_me.png blankhead.png

EXTRA_DIST = $(Book_CSFILES) $(BookLib_CSFILES) $(Book_RESOURCES) $(BookLib_RESOURCES) $(GLADE_FILES) $(IMAGE_FILES)

all: InvitationAssistant.exe ContactBrowser.exe Novell.AddressBook.UI.gtk.dll

ContactBrowser.exe: $(Book_CSFILES) $(Book_RESOURCES) Novell.AddressBook.UI.gtk.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS)  $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:ContactBrowser.doc.xml

InvitationAssistant.exe: $(IA_CSFILES) $(IA_RESOURCES) Novell.AddressBook.UI.gtk.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(IA_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS)  $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(IA_LIBS:%=/r:%) $(IA_LIBPATH:%=/lib:%) $(IA_RESOURCES:%=$(ICON_FLAG)%) $(IA_CSFILES_CSC) /doc:InvitationAssistant.doc.xml

Novell.AddressBook.UI.gtk.dll: $(BookLib_CSFILES) $(BookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(BookLib_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(BookLib_LIBS:%=/r:%) $(BookLib_LIBPATH:%=/lib:%) $(BookLib_RESOURCES:%=$(ICON_FLAG)%) $(BookLib_CSFILES_CSC) /doc:Novell.AddressBook.UI.gtk.doc.xml

install-exec-local: all contactbrowser invitationassistant $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir) $(DESTDIR)$(datadir)/ifolder/glade $(DESTDIR)$(datadir)/ifolder/images
	$(INSTALL_PROGRAM) Novell.AddressBook.UI.gtk.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/ifolder/glade
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder/images
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) InvitationAssistant.exe ContactBrowser.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(srcdir)/contactbrowser $(srcdir)/invitationassistant $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.AddressBook.UI.gtk.dll $(BookLib_RESOURCES)
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(datadir)/ifolder/images; rm -f $(IMAGE_FILES)
	cd $(DESTDIR)$(bindir); rm -f ContactBrowser.exe contactbrowser
    
installcheck-local: install
	
CLEAN_FILES = ContactBrowser.exe ContactBrowser.doc.xml Novell.AddressBook.UI.gtk.dll Novell.AddressBook.UI.gtk.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

