# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

MiniShare_CSFILES = $(srcdir)/Application/AssemblyInfo.cs $(srcdir)/Application/MiniShare.cs
MiniShare_CSFILES_CSC := $(subst /,$(SEP),$(MiniShare_CSFILES))
MiniShare_INCLUDES = 
MiniShare_RESOURCES =
MiniShare_FLAGS = $(CSC_EXEFLAG)
MiniShare_LIBS = System.dll $(SYSTEM_XML) Common.dll CollectionStore.dll StoreProvider.dll SyncManager.dll CollectionWatcher.dll SyncCommon.dll SyncLogicLite.dll Sync.dll
MiniShare_LIBPATH = ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../StoreProvider ../StoreWatcher ../CollectionWatcher ../AddressBook ../Common ../SyncCommon ../SyncManager ../SyncLogicLite ../Sync ../Common ../CollectionEvents

NUNIT_TESTS :=

EXTRA_DIST = $(MiniShare_CSFILES)

CUR_DIR := $(shell pwd)

all: MiniShare.exe

MiniShare.exe: $(MiniShare_CSFILES) $(MiniShare_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(MiniShare_FLAGS) $(MiniShare_LIBS:%=/r:%) $(MiniShare_LIBPATH:%=/lib:%) $(MiniShare_CSFILES_CSC) /doc:MiniShare.doc.xml

echopath:
	echo $(MONO_PATH)
	
if DEBUG
if WINDOWS
DEBUG_FILES = MiniShare.pdb
endif
endif

install-exec-local: MiniShare.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) MiniShare.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f MiniShare.exe $(NUNIT_TESTS) $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:MiniShareTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = MiniShare.exe $(NUNIT_TESTS) MiniShare.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

