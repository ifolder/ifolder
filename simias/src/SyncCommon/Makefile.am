# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncCommon_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/SyncChannelSinks.cs $(srcdir)/Library/SyncCollection.cs $(srcdir)/Library/SyncCollectionInfo.cs $(srcdir)/Library/SyncCollectionRoles.cs $(srcdir)/Library/SyncCollectionService.cs $(srcdir)/Library/SyncCollectionWorker.cs $(srcdir)/Library/SyncLogicFactory.cs $(srcdir)/Library/SyncNode.cs $(srcdir)/Library/SyncNodeInfo.cs $(srcdir)/Library/SyncNodeStream.cs $(srcdir)/Library/SyncNodeStreamInfo.cs $(srcdir)/Library/SyncProperties.cs $(srcdir)/Library/SyncStore.cs $(srcdir)/Library/SyncStoreInfo.cs $(srcdir)/Library/Invitation.cs
SyncCommon_CSFILES_CSC := $(subst /,$(SEP),$(SyncCommon_CSFILES))
SyncCommon_INCLUDES =
SyncCommon_RESOURCES =
SyncCommon_FLAGS = $(CSC_LIBFLAG)
SyncCommon_LIBS = System.dll System.Data.dll $(SYSTEM_XML) StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Common.dll System.Runtime.Remoting.dll
SyncCommon_LIBPATH = Library ../StoreProvider ../Common ../CollectionEvents ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../Common

NUNIT_TESTS := SyncCommonTests.dll

SyncCommonTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/SyncCommonTests.cs
SyncCommonTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncCommonTests_CSFILES))
SyncCommonTests_INCLUDES =
SyncCommonTests_RESOURCES =
SyncCommonTests_FLAGS = $(CSC_LIBFLAG)
SyncCommonTests_LIBS = $(SyncCommon_LIBS) $(SYSTEM_XML) SyncCommon.dll CollectionStore.dll Common.dll nunit.framework.dll StoreProvider.dll
SyncCommonTests_LIBPATH = $(SyncCommon_LIBPATH) Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(SyncCommon_CSFILES) $(SyncCommonTests_CSFILES)

CUR_DIR := $(shell pwd)

all: SyncCommon.dll

SyncCommon.dll: $(SyncCommon_CSFILES) $(SyncCommon_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncCommon_FLAGS) $(SyncCommon_LIBS:%=/r:%) $(SyncCommon_LIBPATH:%=/lib:%) $(SyncCommon_CSFILES_CSC) /doc:SyncCommon.doc.xml

SyncCommonTests.dll: $(SyncCommonTests_CSFILES) $(SyncCommonTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncCommonTests_FLAGS) $(SyncCommonTests_LIBS:%=/r:%) $(SyncCommonTests_LIBPATH:%=/lib:%) $(SyncCommonTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = SyncCommon.pdb
DEBUG_CHECK_FILES = SyncCommonTests.pdb
endif
endif

install-exec-local: SyncCommon.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncCommon.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SyncCommon.dll SyncCommonTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncCommonTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncCommon.dll $(NUNIT_TESTS) SyncCommon.doc.xml SyncCommonTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

