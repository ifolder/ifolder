# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

NUNIT_TESTS := AcceptanceTests.dll

AcceptanceTests_CSFILES = $(srcdir)/SyncTests.cs $(srcdir)/AssemblyInfo.cs
AcceptanceTests_CSFILES_CSC := $(subst /,$(SEP),$(AcceptanceTests_CSFILES))
AcceptanceTests_INCLUDES =
AcceptanceTests_RESOURCES =
AcceptanceTests_FLAGS = $(CSC_LIBFLAG)
AcceptanceTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Agent.dll SyncCommon.dll CollectionWatcher.dll SyncManager.dll Sync.dll SyncLogicLite.dll System.Runtime.Remoting.dll StoreWatcher.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll RemotingSnifferLibrary.dll nunit.framework.dll
AcceptanceTests_LIBPATH = ../SyncCommon ../SyncManager ../SyncLogicLite ../Sync ../CollectionEvents ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../Configuration ../StoreProvider ../Common ../CollectionWatcher ../Agent ../StoreWatcher ../RemotingSniffer $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(AcceptanceTests_CSFILES)

CUR_DIR := $(shell pwd)

all: AcceptanceTests.dll

AcceptanceTests.dll: $(SyncManagerTests_CSFILES) $(SyncManagerTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AcceptanceTests_FLAGS) $(AcceptanceTests_LIBS:%=/r:%) $(AcceptanceTests_LIBPATH:%=/lib:%) $(AcceptanceTests_CSFILES_CSC)
	
install-exec-local: AcceptanceTests.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) AcceptanceTests.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); \
		rm -f AcceptanceTests.test.xml $(NUNIT_TESTS); \
		rm -rf storeA storeB storeC

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AcceptanceTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = $(NUNIT_TESTS) AcceptanceTests.doc.xml AcceptanceTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

