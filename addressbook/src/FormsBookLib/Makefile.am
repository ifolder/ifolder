FormsBookLib_CSFILES = $(srcdir)/AddBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BooksContacts.cs $(srcdir)/ContactEditor.cs $(srcdir)/ContactPicker.cs
FormsBookLib_CSFILES_CSC := $(subst /,$(SEP),$(FormsBookLib_CSFILES))
FormsBookLib_INCLUDES = 
FormsBookLib_RESOURCES =
FormsBookLib_FLAGS = $(CSC_LIBFLAG)
FormsBookLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
FormsBookLib_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

EXTRA_DIST = $(FormsBookLib_CSFILES)

all: FormsBookLib.dll

FormsBookLib.dll: $(FormsBookLib_CSFILES) $(FormsBookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FormsBookLib_FLAGS) $(FormsBookLib_LIBS:%=/r:%) $(FormsBookLib_LIBPATH:%=/lib:%) $(FormsBookLib_CSFILES_CSC) /doc:FormsBookLib.xml

install-exec-local: FormsBookLib.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FormsBookLib.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FormsBookLib.dll
    
installcheck-local: install
	
CLEAN_FILES = FormsBookLib.dll FormsBookLib.xml

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

