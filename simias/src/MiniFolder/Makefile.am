# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

MiniFolder_CSFILES = $(srcdir)/Application/AssemblyInfo.cs $(srcdir)/Application/MiniFolder.cs
MiniFolder_CSFILES_CSC := $(subst /,$(SEP),$(MiniFolder_CSFILES))
MiniFolder_INCLUDES = 
MiniFolder_RESOURCES =
MiniFolder_FLAGS = $(CSC_EXEFLAG)
MiniFolder_LIBS = System.dll $(SYSTEM_XML) Common.dll CollectionStore.dll StoreProvider.dll SyncManager.dll CollectionWatcher.dll SyncCommon.dll SyncLogicLite.dll Sync.dll
MiniFolder_LIBPATH = ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../StoreProvider ../StoreWatcher ../CollectionWatcher ../Agent ../AddressBook ../Common ../SyncCommon ../SyncManager ../SyncLogicLite ../Sync ../Configuration ../Common ../CollectionEvents

NUNIT_TESTS :=

EXTRA_DIST = $(MiniFolder_CSFILES)

CUR_DIR := $(shell pwd)

all: MiniFolder.exe

MiniFolder.exe: $(MiniFolder_CSFILES) $(MiniFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(MiniFolder_FLAGS) $(MiniFolder_LIBS:%=/r:%) $(MiniFolder_LIBPATH:%=/lib:%) $(MiniFolder_CSFILES_CSC) /doc:MiniFolder.doc.xml

echopath:
	echo $(MONO_PATH)
	
if DEBUG
if WINDOWS
DEBUG_FILES = MiniFolder.pdb
endif
endif

install-exec-local: MiniFolder.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) MiniFolder.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f MiniFolder.exe $(NUNIT_TESTS) $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:MiniFolderTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = MiniFolder.exe $(NUNIT_TESTS) MiniFolder.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

