StoreWatcher_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/StoreWatcher.cs
StoreWatcher_CSFILES_CSC := $(subst /,$(SEP),$(StoreWatcher_CSFILES))
StoreWatcher_INCLUDES =
StoreWatcher_RESOURCES =
StoreWatcher_FLAGS = $(CSC_LIBFLAG)
StoreWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll SqliteProvider.dll SqliteWrapper.dll Common.dll
StoreWatcher_LIBPATH = Library ../Configuration ../Common ../CollectionEvents ../CollectionStore ../StoreProvider ../SqliteProvider ../Common

NUNIT_TESTS := StoreWatcherTests.dll

StoreWatcherTests_CSFILES = $(srcdir)/Tests/StoreWatcherTests.cs $(srcdir)/Tests/AssemblyInfo.cs
StoreWatcherTests_CSFILES_CSC := $(subst /,$(SEP),$(StoreWatcherTests_CSFILES))
StoreWatcherTests_INCLUDES =
StoreWatcherTests_RESOURCES =
StoreWatcherTests_FLAGS = $(CSC_LIBFLAG)
StoreWatcherTests_LIBS = $(StoreWatcher_LIBS) $(SYSTEM_XML) nunit.framework.dll StoreWatcher.dll
StoreWatcherTests_LIBPATH = $(StoreWatcher_LIBPATH) Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(StoreWatcher_CSFILES) $(StoreWatcherTests_CSFILES)

CUR_DIR := $(shell pwd)

all: StoreWatcher.dll

StoreWatcher.dll: $(StoreWatcher_CSFILES) $(StoreWatcher_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(StoreWatcher_FLAGS) $(StoreWatcher_LIBS:%=/r:%) $(StoreWatcher_LIBPATH:%=/lib:%) $(StoreWatcher_CSFILES_CSC) /doc:StoreWatcher.xml

StoreWatcherTests.dll: $(StoreWatcherTests_CSFILES) $(StoreWatcherTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(StoreWatcherTests_FLAGS) $(StoreWatcherTests_LIBS:%=/r:%) $(StoreWatcherTests_LIBPATH:%=/lib:%) $(StoreWatcherTests_CSFILES_CSC)
	
install-exec-local: StoreWatcher.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) StoreWatcher.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f StoreWatcher.dll StoreWatcherTests.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:StoreWatcherTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = StoreWatcher.dll $(NUNIT_TESTS) StoreWatcher.xml StoreWatcherTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

