SyncLogicLite_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/SyncCollectionServiceLite.cs $(srcdir)/Library/SyncCollectionWorkerLite.cs $(srcdir)/Library/SyncLogicFactoryLite.cs $(srcdir)/Library/SyncPacket.cs $(srcdir)/Library/SyncPacketData.cs $(srcdir)/Library/SyncPacketHeader.cs
SyncLogicLite_CSFILES_CSC := $(subst /,$(SEP),$(SyncLogicLite_CSFILES))
SyncLogicLite_INCLUDES = 
SyncLogicLite_RESOURCES =
SyncLogicLite_FLAGS = $(CSC_LIBFLAG)
SyncLogicLite_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Identity.dll Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll Agent.dll SyncCommon.dll
SyncLogicLite_LIBPATH = Library ../Common ../Identity ../SyncCommon ../StoreProvider ../CollectionStore ../Configuration ../CollectionEvents ../Agent

NUNIT_TESTS := SyncLogicLiteTests.dll

SyncLogicLiteTests_CSFILES = $(srcdir)/Tests/SyncLogicLiteTests.cs $(srcdir)/Tests/AssemblyInfo.cs
SyncLogicLiteTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncLogicLiteTests_CSFILES))
SyncLogicLiteTests_INCLUDES = 
SyncLogicLiteTests_RESOURCES =
SyncLogicLiteTests_FLAGS = $(CSC_LIBFLAG)
SyncLogicLiteTests_LIBS = $(SyncLogicLite_LIBS) $(SYSTEM_XML) nunit.framework.dll SyncLogicLite.dll 
SyncLogicLiteTests_LIBPATH = $(SyncLogicLite_LIBPATH) Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(SyncLogicLite_CSFILES) $(SyncLogicLiteTests_CSFILES)

CUR_DIR := $(shell pwd)

all: SyncLogicLite.dll

SyncLogicLite.dll: $(SyncLogicLite_CSFILES) $(SyncLogicLite_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncLogicLite_FLAGS) $(SyncLogicLite_LIBS:%=/r:%) $(SyncLogicLite_LIBPATH:%=/lib:%) $(SyncLogicLite_CSFILES_CSC) /doc:SyncLogicLite.xml

SyncLogicLiteTests.dll: $(SyncLogicLiteTests_CSFILES) $(SyncLogicLiteTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncLogicLiteTests_FLAGS) $(SyncLogicLiteTests_LIBS:%=/r:%) $(SyncLogicLiteTests_LIBPATH:%=/lib:%) $(SyncLogicLiteTests_CSFILES_CSC)
	
install-exec-local: SyncLogicLite.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SyncLogicLite.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); \
		rm -f SyncLogicLite.dll SyncLogicLiteTests.xml $(NUNIT_TESTS); \
		rm -rf cwatcher
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SyncLogicLiteTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SyncLogicLite.dll $(NUNIT_TESTS) SyncLogicLite.xml SyncLogicLiteTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

