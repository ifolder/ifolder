# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

FsWatcher_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FsWatcher.cs
FsWatcher_CSFILES_CSC := $(subst /,$(SEP),$(FsWatcher_CSFILES))
FsWatcher_INCLUDES =
FsWatcher_RESOURCES =
FsWatcher_FLAGS = $(CSC_LIBFLAG)
FsWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll Simias.Service.dll
FsWatcher_LIBPATH = ../Common ../StoreProvider ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../CollectionEvents ../Service

NUNIT_TESTS :=

FsWatcherTests_CSFILES =
FsWatcherTests_CSFILES_CSC := $(subst /,$(SEP),$(FsWatcherTests_CSFILES))
FsWatcherTests_INCLUDES =
FsWatcherTests_RESOURCES =
FsWatcherTests_FLAGS =
FsWatcherTests_LIBS =
FsWatcherTests_LIBPATH = $(NUNIT_LIBPATH)

EXTRA_DIST = $(FsWatcher_CSFILES) $(FsWatcherTests_CSFILES) App$(ICON_EXT)

CUR_DIR := $(shell pwd)

all: FsWatcher.dll

FsWatcher.dll: $(FsWatcher_CSFILES) $(FsWatcher_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FsWatcher_FLAGS) $(FsWatcher_LIBS:%=/r:%) $(FsWatcher_LIBPATH:%=/lib:%) $(FsWatcher_CSFILES_CSC) /doc:FsWatcher.doc.xml

#FsWatcherTests.dll: $(FsWatcherTests_CSFILES) $(FsWatcherTests_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(FsWatcherTests_FLAGS) $(FsWatcherTests_LIBS:%=/r:%) $(FsWatcherTests_LIBPATH:%=/lib:%) $(FsWatcherTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = FsWatcher.pdb
endif
endif

install-exec-local: FsWatcher.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FsWatcher.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FsWatcher.dll FsWatcherTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES)

installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:FsWatcherTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = FsWatcher.dll $(NUNIT_TESTS) FsWatcher.doc.xml FsWatcherTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

