# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Book_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsAddrBook.cs
Book_CSFILES_CSC := $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES = $(srcdir)/App$(ICON_EXT)
Book_FLAGS = $(CSC_WINEXEFLAG)
Book_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) FormsBookLib.dll  Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
Book_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../FormsBookLib

DATA_FILES = address_app.ico

EXTRA_DIST = $(Book_CSFILES) $(DATA_FILES)

all: ContactBrowser.exe

ContactBrowser.exe: $(Book_CSFILES) $(Book_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:ContactBrowser.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = ContactBrowser.pdb
endif
endif

install-exec-local: ContactBrowser.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ContactBrowser.exe $(DATA_FILES) $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ContactBrowser.exe $(DATA_FILES) $(DEBUG_FILES)
    
installcheck-local: install
	
CLEAN_FILES = ContactBrowser.exe ContactBrowser.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

