# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncLogicLite_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/SyncCollectionServiceLite.cs $(srcdir)/Library/SyncCollectionWorkerLite.cs $(srcdir)/Library/SyncLogicFactoryLite.cs $(srcdir)/Library/SyncPacket.cs $(srcdir)/Library/SyncNodeInfo.cs
SyncLogicLite_CSFILES_CSC := $(subst /,$(SEP),$(SyncLogicLite_CSFILES))
SyncLogicLite_INCLUDES =
SyncLogicLite_RESOURCES =
SyncLogicLite_FLAGS = $(CSC_LIBFLAG)
SyncLogicLite_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll SyncCommon.dll
SyncLogicLite_LIBPATH = Library ../Common ../SyncCommon ../StoreProvider ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../CollectionEvents

NUNIT_TESTS := SyncLogicLiteTests.dll

SyncLogicLiteTests_CSFILES = $(srcdir)/Tests/SyncLogicLiteTests.cs $(srcdir)/Tests/AssemblyInfo.cs
SyncLogicLiteTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncLogicLiteTests_CSFILES))
SyncLogicLiteTests_INCLUDES =
SyncLogicLiteTests_RESOURCES =
SyncLogicLiteTests_FLAGS = $(CSC_LIBFLAG)
SyncLogicLiteTests_LIBS = $(SyncLogicLite_LIBS) $(SYSTEM_XML) nunit.framework.dll SyncLogicLite.dll
SyncLogicLiteTests_LIBPATH = $(SyncLogicLite_LIBPATH) Tests $(NUNIT_LIBPATH)

EXTRA_DIST = $(SyncLogicLite_CSFILES) $(SyncLogicLiteTests_CSFILES)

CUR_DIR := $(shell pwd)

all: SyncLogicLite.dll

SyncLogicLite.dll: $(SyncLogicLite_CSFILES) $(SyncLogicLite_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncLogicLite_FLAGS) $(SyncLogicLite_LIBS:%=/r:%) $(SyncLogicLite_LIBPATH:%=/lib:%) $(SyncLogicLite_CSFILES_CSC) /doc:SyncLogicLite.doc.xml

SyncLogicLiteTests.dll: $(SyncLogicLiteTests_CSFILES) $(SyncLogicLiteTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncLogicLiteTests_FLAGS) $(SyncLogicLiteTests_LIBS:%=/r:%) $(SyncLogicLiteTests_LIBPATH:%=/lib:%) $(SyncLogicLiteTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = SyncLogicLite.pdb
DEBUG_CHECK_FILES = SyncLogicLiteTests.pdb
endif
endif

install-exec-local: SyncLogicLite.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncLogicLite.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SyncLogicLite.dll SyncLogicLiteTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
	
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncLogicLiteTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncLogicLite.dll $(NUNIT_TESTS) SyncLogicLite.doc.xml SyncLogicLiteTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

