FsWatcher_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FsWatcher.cs
FsWatcher_CSFILES_CSC := $(subst /,$(SEP),$(FsWatcher_CSFILES))
FsWatcher_INCLUDES =
FsWatcher_RESOURCES =
FsWatcher_FLAGS = $(CSC_EXEFLAG) $(ICON_FLAG)$(srcdir)/App$(ICON_EXT)
FsWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll UserService.dll
FsWatcher_LIBPATH = ../Configuration ../Common ../StoreProvider ../CollectionStore ../CollectionEvents ../UserService

NUNIT_TESTS :=

FsWatcherTests_CSFILES =
FsWatcherTests_CSFILES_CSC := $(subst /,$(SEP),$(FsWatcherTests_CSFILES))
FsWatcherTests_INCLUDES =
FsWatcherTests_RESOURCES =
FsWatcherTests_FLAGS =
FsWatcherTests_LIBS =
FsWatcherTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(FsWatcher_CSFILES) $(FsWatcherTests_CSFILES) App$(ICON_EXT)

CUR_DIR := $(shell pwd)

all: FsWatcher.exe

FsWatcher.exe: $(FsWatcher_CSFILES) $(FsWatcher_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FsWatcher_FLAGS) $(FsWatcher_LIBS:%=/r:%) $(FsWatcher_LIBPATH:%=/lib:%) $(FsWatcher_CSFILES_CSC) /doc:FsWatcher.xml

#FsWatcherTests.dll: $(FsWatcherTests_CSFILES) $(FsWatcherTests_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(FsWatcherTests_FLAGS) $(FsWatcherTests_LIBS:%=/r:%) $(FsWatcherTests_LIBPATH:%=/lib:%) $(FsWatcherTests_CSFILES_CSC)
	
install-exec-local: FsWatcher.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FsWatcher.exe $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FsWatcher.exe FsWatcherTests.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:FsWatcherTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = FsWatcher.exe $(NUNIT_TESTS) FsWatcher.xml FsWatcherTests.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

