StoreProvider_CSFILES := $(srcdir)/AssemblyInfo.cs $(srcdir)/CSPException.cs $(srcdir)/IProvider.cs $(srcdir)/IResultSet.cs $(srcdir)/Provider.cs $(srcdir)/Query.cs $(srcdir)/Record.cs
StoreProvider_CSFILES_CSC := $(subst /,$(SEP),$(StoreProvider_CSFILES))
StoreProvider_INCLUDES = 
StoreProvider_RESOURCES =
StoreProvider_FLAGS = $(CSC_LIBFLAG)
StoreProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll
StoreProvider_LIBPATH = ../Configuration

NUNIT_TESTS := ProviderTest.dll

ProviderTest_CSFILES = $(srcdir)/Test/AssemblyInfo.cs $(srcdir)/Test/ProviderTests1.cs
ProviderTest_CSFILES_CSC := $(subst /,$(SEP),$(ProviderTest_CSFILES))
ProviderTest_INCLUDES = 
ProviderTest_RESOURCES =
ProviderTest_FLAGS = $(CSC_LIBFLAG)
ProviderTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll nunit.framework.dll
ProviderTest_LIBPATH = Test $(srcdir)/../../tools/NUnit/bin ../Configuration

EXTRA_DIST = $(StoreProvider_CSFILES) $(ProviderTest_CSFILES)

CUR_DIR := $(shell pwd)

all: StoreProvider.dll

StoreProvider.dll: $(StoreProvider_CSFILES) $(StoreProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(StoreProvider_FLAGS) $(StoreProvider_LIBS:%=/r:%) $(StoreProvider_LIBPATH:%=/lib:%) $(StoreProvider_CSFILES_CSC) /doc:StoreProvider.xml

ProviderTest.dll: $(ProviderTest_CSFILES) $(ProviderTest_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ProviderTest_FLAGS) $(ProviderTest_LIBS:%=/r:%) $(ProviderTest_LIBPATH:%=/lib:%) $(ProviderTest_CSFILES_CSC)
	
install-exec-local: StoreProvider.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) StoreProvider.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f StoreProvider.dll $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ConfTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = StoreProvider.dll $(NUNIT_TESTS) StoreProvider.xml ConfTests.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

