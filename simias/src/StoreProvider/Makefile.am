# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

StoreProvider_CSFILES := $(srcdir)/AssemblyInfo.cs $(srcdir)/IProvider.cs $(srcdir)/IResultSet.cs $(srcdir)/Provider.cs $(srcdir)/Query.cs $(srcdir)/Record.cs
StoreProvider_CSFILES_CSC := $(subst /,$(SEP),$(StoreProvider_CSFILES))
StoreProvider_INCLUDES =
StoreProvider_RESOURCES =
StoreProvider_FLAGS = $(CSC_LIBFLAG)
StoreProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Storage.dll
StoreProvider_LIBPATH = ../Common ../Storage

NUNIT_TESTS := ProviderTest.dll

ProviderTest_CSFILES = $(srcdir)/Test/AssemblyInfo.cs $(srcdir)/Test/ProviderTests1.cs
ProviderTest_CSFILES_CSC := $(subst /,$(SEP),$(ProviderTest_CSFILES))
ProviderTest_INCLUDES =
ProviderTest_RESOURCES =
ProviderTest_FLAGS = $(CSC_LIBFLAG)
ProviderTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Storage.dll StoreProvider.dll nunit.framework.dll
ProviderTest_LIBPATH = ../Common Test $(NUNIT_LIBPATH) ../Storage

EXTRA_DIST = $(StoreProvider_CSFILES) $(ProviderTest_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: StoreProvider.dll

StoreProvider.dll: $(StoreProvider_CSFILES) $(StoreProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(StoreProvider_FLAGS) $(StoreProvider_LIBS:%=/r:%) $(StoreProvider_LIBPATH:%=/lib:%) $(StoreProvider_CSFILES_CSC) $(DOC_FILE_CMD)

ProviderTest.dll: $(ProviderTest_CSFILES) $(ProviderTest_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ProviderTest_FLAGS) $(ProviderTest_LIBS:%=/r:%) $(ProviderTest_LIBPATH:%=/lib:%) $(ProviderTest_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = StoreProvider.pdb
DEBUG_CHECK_FILES = ProviderTest.pdb
endif
endif

install-exec-local: StoreProvider.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) StoreProvider.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f StoreProvider.dll $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ConfTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = StoreProvider.dll $(NUNIT_TESTS) $(DOC_FILE_CLEAN) ConfTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

