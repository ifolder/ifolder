# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncManager_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/SyncChannel.cs $(srcdir)/Library/SyncChannelFactory.cs $(srcdir)/Library/SyncCollectionManager.cs $(srcdir)/Library/SyncCredentialCache.cs $(srcdir)/Library/SyncManager.cs $(srcdir)/Library/SyncPing.cs $(srcdir)/Library/SyncStoreManager.cs $(srcdir)/Library/SyncStoreService.cs 
SyncManager_CSFILES_CSC := $(subst /,$(SEP),$(SyncManager_CSFILES))
SyncManager_INCLUDES =
SyncManager_RESOURCES =
SyncManager_FLAGS = $(CSC_LIBFLAG)
SyncManager_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Agent.dll SyncCommon.dll CollectionWatcher.dll System.Runtime.Remoting.dll StoreWatcher.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll RemotingSnifferLibrary.dll
SyncManager_LIBPATH = Library ../SyncCommon ../CollectionEvents ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../Configuration ../StoreProvider ../Common ../CollectionWatcher ../Agent ../StoreWatcher ../RemotingSniffer

NUNIT_TESTS := SyncManagerTests.dll

SyncManagerTests_CSFILES = $(srcdir)/Tests/SyncManagerTests.cs $(srcdir)/Tests/AssemblyInfo.cs
SyncManagerTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncManagerTests_CSFILES))
SyncManagerTests_INCLUDES =
SyncManagerTests_RESOURCES =
SyncManagerTests_FLAGS = $(CSC_LIBFLAG)
SyncManagerTests_LIBS = $(SyncManager_LIBS) $(SYSTEM_XML) nunit.framework.dll SyncManager.dll
SyncManagerTests_LIBPATH = $(SyncManager_LIBPATH) Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(SyncManager_CSFILES) $(SyncManagerTests_CSFILES) $(srcdir)/remoting.config

CUR_DIR := $(shell pwd)

all: SyncManager.dll

SyncManager.dll: $(SyncManager_CSFILES) $(SyncManager_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncManager_FLAGS) $(SyncManager_LIBS:%=/r:%) $(SyncManager_LIBPATH:%=/lib:%) $(SyncManager_CSFILES_CSC) /doc:SyncManager.doc.xml

SyncManagerTests.dll: $(SyncManagerTests_CSFILES) $(SyncManagerTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncManagerTests_FLAGS) $(SyncManagerTests_LIBS:%=/r:%) $(SyncManagerTests_LIBPATH:%=/lib:%) $(SyncManagerTests_CSFILES_CSC)
	
install-exec-local: SyncManager.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncManager.dll remoting.config $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); \
		rm -f SyncManager.dll remoting.config SyncManagerTests.test.xml $(NUNIT_TESTS); \
		rm -rf manager1

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncManagerTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncManager.dll $(NUNIT_TESTS) SyncManager.doc.xml SyncManagerTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

