FormsBookLib_CSFILES = $(srcdir)/AddBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BooksContacts.cs $(srcdir)/ContactEditor.cs $(srcdir)/ContactPicker.cs
FormsBookLib_CSFILES_CSC := $(subst /,$(SEP),$(FormsBookLib_CSFILES))
FormsBookLib_INCLUDES = 
FormsBookLib_RESOURCES =
FormsBookLib_FLAGS = $(CSC_LIBFLAG)
FormsBookLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
FormsBookLib_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

DATA_FILES = ifolder_add_bk.ico ifolder_contact_card.ico ifolder_me_card.ico

EXTRA_DIST = $(FormsBookLib_CSFILES) $(DATA_FILES)

all: FormsBookLib.dll

FormsBookLib.dll: $(FormsBookLib_CSFILES) $(FormsBookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FormsBookLib_FLAGS) $(FormsBookLib_LIBS:%=/r:%) $(FormsBookLib_LIBPATH:%=/lib:%) $(FormsBookLib_CSFILES_CSC) /doc:FormsBookLib.doc.xml

install-exec-local: FormsBookLib.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FormsBookLib.dll $(DATA_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FormsBookLib.dll $(DATA_FILES)
    
installcheck-local: install
	
CLEAN_FILES = FormsBookLib.dll FormsBookLib.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

