Book_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsAddrBook.cs
Book_CSFILES_CSC := $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES = $(srcdir)/App$(ICON_EXT)
Book_FLAGS = $(CSC_EXEFLAG)
Book_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) FormsBookLib.dll  Configuration.dll StoreProvider.dll Identity.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
Book_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

EXTRA_DIST = $(Book_CSFILES)

all: Book.exe

Book.exe: $(Book_CSFILES) $(Book_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:Book.xml

install-exec-local: Book.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Book.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Book.exe
    
installcheck-local: install
	
CLEAN_FILES = Book.exe Book.xml

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

