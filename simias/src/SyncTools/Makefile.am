# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncPing_CSFILES = $(srcdir)/Ping/AssemblyInfo.cs $(srcdir)/Ping/SyncPinger.cs
SyncPing_CSFILES_CSC := $(subst /,$(SEP),$(SyncPing_CSFILES))
SyncPing_INCLUDES =
SyncPing_RESOURCES = $(srcdir)/Ping/App$(ICON_EXT)
SyncPing_FLAGS = $(CSC_EXEFLAG)
SyncPing_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll SyncCommon.dll SyncManager.dll
SyncPing_LIBPATH = Library ../Common ../SyncCommon ../StoreProvider ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../CollectionEvents ../SyncManager

EXTRA_DIST = $(SyncPing_CSFILES) $(SyncPing_RESOURCES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SyncPing.exe

SyncPing.exe: $(SyncPing_CSFILES) $(SyncPing_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncPing_FLAGS) $(SyncPing_LIBS:%=/r:%) $(SyncPing_LIBPATH:%=/lib:%) $(SyncPing_RESOURCES:%=$(ICON_FLAG)%) $(SyncPing_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = SyncPing.pdb
endif
endif

install-exec-local: SyncPing.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncPing.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SyncPing.exe $(NUNIT_TESTS) $(DEBUG_FILES)

installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncPingTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncPing.exe $(NUNIT_TESTS) $(DOC_FILE_CLEAN) SyncPingTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

