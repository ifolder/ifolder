Sync_CSFILES = $(srcdir)/Synker.cs $(srcdir)/SyncPoint.cs $(srcdir)/SyncServ.cs $(srcdir)/SyncPass.cs $(srcdir)/SyncOps.cs $(srcdir)/Dredge.cs $(srcdir)/SyncLog.cs
Sync_CSFILES_CSC := $(subst /,$(SEP),$(Sync_CSFILES))
Sync_INCLUDES = 
Sync_RESOURCES =
Sync_FLAGS = $(CSC_LIBFLAG)
Sync_LIBS = Common.dll CollectionEvents.dll CollectionStore.dll Identity.dll Configuration.dll SyncCommon.dll Agent.dll System.dll System.Data.dll System.Runtime.Remoting.dll
Sync_LIBPATH = ../Common ../SyncCommon ../Agent ../CollectionEvents ../CollectionStore ../StoreProvider ../Identity ../FsProvider ../WorkGroupIdentity ../Configuration ../SqliteProvider ../../external/sqlite/linux

SyncCmd_CSFILES = $(srcdir)/SyncCmd.cs
SyncCmd_CSFILES_CSC := $(subst /,$(SEP),$(SyncCmd_CSFILES))
SyncCmd_INCLUDES = 
SyncCmd_RESOURCES =
SyncCmd_FLAGS = $(CSC_EXEFLAG)
SyncCmd_LIBS = Sync.dll $(Sync_LIBS)
SyncCmd_LIBPATH = $(Sync_LIBPATH)

NUNIT_TESTS := TestSync.exe

TestSync_CSFILES = $(srcdir)/TestSync.cs
TestSync_CSFILES_CSC := $(subst /,$(SEP),$(TestSync_CSFILES))
TestSync_INCLUDES = 
TestSync_RESOURCES =
TestSync_FLAGS = $(CSC_LIBFLAG)
TestSync_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Sync.dll CollectionStore.dll Identity.dll
TestSync_LIBPATH = $(Sync_LIBPATH) ../../tools/NUnit/bin $(srcdir)/../../tools/NUnit/bin

SyncTests_CSFILES = $(srcdir)/SyncTestStore.cs $(srcdir)/SyncTestCollection.cs $(srcdir)/Sync01Tests.cs $(srcdir)/Sync02Tests.cs
SyncTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncTests_CSFILES))
SyncTests_INCLUDES = 
SyncTests_RESOURCES =
SyncTests_FLAGS = $(CSC_LIBFLAG)
SyncTests_LIBS = Sync.dll System.dll System.Data.dll CollectionStore.dll Identity.dll System.Runtime.Remoting.dll nunit.util.dll nunit.framework.dll
SyncTests_LIBPATH = $(Sync_LIBPATH) $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Sync_CSFILES) $(SyncCmd_CSFILES) $(TestSync_CSFILES) $(SyncTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Sync.dll SyncCmd.exe

Sync.dll: $(Sync_CSFILES) $(Sync_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Sync_FLAGS) $(Sync_LIBS:%=/r:%) $(Sync_LIBPATH:%=/lib:%) $(Sync_CSFILES_CSC) /doc:Sync.xml

SyncCmd.exe: $(SyncCmd_CSFILES) $(SyncCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncCmd_FLAGS) $(SyncCmd_LIBS:%=/r:%) $(SyncCmd_LIBPATH:%=/lib:%) $(SyncCmd_CSFILES_CSC) /doc:SyncCmd.xml

TestSync.exe: $(TestSync_CSFILES) $(TestSync_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TestSync_FLAGS) $(TestSync_LIBS:%=/r:%) $(TestSync_LIBPATH:%=/lib:%) $(TestSync_CSFILES_CSC)
	
SyncTests.exe: $(SyncTests_CSFILES) $(SyncTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncTests_FLAGS) $(SyncTests_LIBS:%=/r:%) $(SyncTests_LIBPATH:%=/lib:%) $(SyncTests_CSFILES_CSC)
	
install-exec-local: Sync.dll SyncCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Sync.dll SyncCmd.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Sync.dll SyncCmd.exe TestSync.xml SyncTests.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:TestSync.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Sync.dll SyncCmd.exe TestSync.exe $(NUNIT_TESTS) Sync.xml SyncCmd.xml TestSync.xml SyncTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

