SyncPing_CSFILES = $(srcdir)/Ping/AssemblyInfo.cs $(srcdir)/Ping/SyncPinger.cs
SyncPing_CSFILES_CSC := $(subst /,$(SEP),$(SyncPing_CSFILES))
SyncPing_INCLUDES = 
SyncPing_RESOURCES = $(srcdir)/Ping/App$(ICON_EXT)
SyncPing_FLAGS = $(CSC_EXEFLAG)
SyncPing_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Identity.dll Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll Agent.dll SyncCommon.dll CollectionWatcher.dll StoreWatcher.dll SyncManager.dll
SyncPing_LIBPATH = Library ../Common ../Identity ../SyncCommon ../StoreProvider ../CollectionStore ../Configuration ../CollectionEvents ../Agent ../SyncManager ../CollectionWatcher ../StoreWatcher

EXTRA_DIST = $(SyncPing_CSFILES) $(SyncPing_RESOURCES)

all: SyncPing.exe

SyncPing.exe: $(SyncPing_CSFILES) $(SyncPing_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncPing_FLAGS) $(SyncPing_LIBS:%=/r:%) $(SyncPing_LIBPATH:%=/lib:%) $(SyncPing_RESOURCES:%=$(ICON_FLAG)%) $(SyncPing_CSFILES_CSC) /doc:SyncPing.xml

install-exec-local: SyncPing.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncPing.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SyncPing.exe $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncPingTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncPing.exe $(NUNIT_TESTS) SyncPing.xml SyncPingTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

