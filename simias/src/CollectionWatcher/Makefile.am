CollectionWatcher_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/CollectionWatcher.cs
CollectionWatcher_CSFILES_CSC := $(subst /,$(SEP),$(CollectionWatcher_CSFILES))
CollectionWatcher_INCLUDES =
CollectionWatcher_RESOURCES =
CollectionWatcher_FLAGS = $(CSC_LIBFLAG)
CollectionWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Common.dll Agent.dll SyncCommon.dll
CollectionWatcher_LIBPATH = Library ../CollectionStore  ../../external/SecureChannelSinks ../StoreProvider ../Common ../SyncCommon ../Configuration ../Common ../CollectionEvents ../Agent

NUNIT_TESTS := CollectionWatcherTests.dll

CollectionWatcherTests_CSFILES = $(srcdir)/Tests/CollectionWatcherTests.cs $(srcdir)/Tests/AssemblyInfo.cs
CollectionWatcherTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionWatcherTests_CSFILES))
CollectionWatcherTests_INCLUDES =
CollectionWatcherTests_RESOURCES =
CollectionWatcherTests_FLAGS = $(CSC_LIBFLAG)
CollectionWatcherTests_LIBS = $(CollectionWatcher_LIBS) $(SYSTEM_XML) nunit.framework.dll CollectionWatcher.dll
CollectionWatcherTests_LIBPATH = $(CollectionWatcher_LIBPATH) Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(CollectionWatcher_CSFILES) $(CollectionWatcherTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CollectionWatcher.dll

CollectionWatcher.dll: $(CollectionWatcher_CSFILES) $(CollectionWatcher_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionWatcher_FLAGS) $(CollectionWatcher_LIBS:%=/r:%) $(CollectionWatcher_LIBPATH:%=/lib:%) $(CollectionWatcher_CSFILES_CSC) /doc:CollectionWatcher.doc.xml

CollectionWatcherTests.dll: $(CollectionWatcherTests_CSFILES) $(CollectionWatcherTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionWatcherTests_FLAGS) $(CollectionWatcherTests_LIBS:%=/r:%) $(CollectionWatcherTests_LIBPATH:%=/lib:%) $(CollectionWatcherTests_CSFILES_CSC)
	
install-exec-local: CollectionWatcher.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CollectionWatcher.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); \
		rm -f CollectionWatcher.dll CollectionWatcherTests.test.xml $(NUNIT_TESTS); \
		rm -rf cwatcher

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionWatcherTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CollectionWatcher.dll $(NUNIT_TESTS) CollectionWatcher.doc.xml CollectionWatcherTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

