Book_CSFILES = $(srcdir)/Main.cs
Book_CSFILES_CSC := $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES =
Book_FLAGS = $(CSC_EXEFLAG)
Book_LIBS = $(BookLib_LIBS) BookLib.dll
Book_LIBPATH = $(BookLib_LIBPATH)

BookLib_CSFILES = $(srcdir)/AddrBookWindow.cs $(srcdir)/ContactEditor.cs $(srcdir)/BookEditor.cs $(srcdir)/ContactPicker.cs
BookLib_CSFILES_CSC := $(subst /,$(SEP),$(BookLib_CSFILES))
BookLib_INCLUDES = 
BookLib_RESOURCES = $(srcdir)/addressbook.glade
BookLib_FLAGS = $(CSC_LIBFLAG)
BookLib_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll pango-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
BookLib_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

DATA_FILES = vcardimport.png vcardexport.png book.png briefcase.png contact.png contact_me.png cellphone.png connect_to_url-16.xpm galeon.png globe.png house.png ico-mail.png blankhead.png addressbook.glade

EXTRA_DIST = $(Book_CSFILES) $(BookLib_CSFILES) $(Book_RESOURCES) $(BookLib_RESOURCES) $(DATA_FILES)

all: Book.exe BookLib.dll

Book.exe: $(Book_CSFILES) $(Book_RESOURCES) BookLib.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:Book.xml

BookLib.dll: $(BookLib_CSFILES) $(BookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(BookLib_FLAGS) $(BookLib_LIBS:%=/r:%) $(BookLib_LIBPATH:%=/lib:%) $(BookLib_RESOURCES:%=$(ICON_FLAG)%) $(BookLib_CSFILES_CSC) /doc:BookLib.xml

install-exec-local: Book.exe BookLib.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Book.exe BookLib.dll $(DATA_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Book.exe BookLib.dll $(DATA_FILES) $(BookLib_RESOURCES)
    
installcheck-local: install
	
CLEAN_FILES = Book.exe Book.xml BookLib.dll BookLib.xml

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

