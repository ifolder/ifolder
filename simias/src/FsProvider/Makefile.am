# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

FsProvider_CSFILES := $(srcdir)/AssemblyInfo.cs $(srcdir)/FsProvider.cs $(srcdir)/FsResultSet.cs
FsProvider_CSFILES_CSC := $(subst /,$(SEP),$(FsProvider_CSFILES))
FsProvider_INCLUDES =
FsProvider_RESOURCES =
FsProvider_FLAGS = $(CSC_LIBFLAG)
FsProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Storage.dll StoreProvider.dll Common.dll
FsProvider_LIBPATH = ../Storage ../StoreProvider ../Common

NUNIT_TESTS := FsProviderTests.dll

FsProviderTests_CSFILES = $(srcdir)/FsProviderTests/AssemblyInfo.cs $(srcdir)/FsProviderTests/FsTests1.cs
FsProviderTests_CSFILES_CSC := $(subst /,$(SEP),$(FsProviderTests_CSFILES))
FsProviderTests_INCLUDES =
FsProviderTests_RESOURCES =
FsProviderTests_FLAGS = $(CSC_LIBFLAG)
FsProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll StoreProvider.dll ProviderTest.dll
FsProviderTests_LIBPATH = $(NUNIT_LIBPATH) ../StoreProvider

EXTRA_DIST = $(FsProvider_CSFILES) $(FsProviderTests_CSFILES) AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: FsProvider.dll

FsProvider.dll: $(FsProvider_CSFILES) $(FsProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FsProvider_FLAGS) $(FsProvider_LIBS:%=/r:%) $(FsProvider_LIBPATH:%=/lib:%) $(FsProvider_CSFILES_CSC) $(DOC_FILE_CMD)

FsProviderTests.dll: $(FsProviderTests_CSFILES) $(FsProviderTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FsProviderTests_FLAGS) $(FsProviderTests_LIBS:%=/r:%) $(FsProviderTests_LIBPATH:%=/lib:%) $(FsProviderTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = FsProvider.pdb
DEBUG_CHECK_FILES = FsProviderTests.pdb
endif
endif

install-exec-local: FsProvider.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FsProvider.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FsProvider.dll FsProviderTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:FsProviderTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = FsProvider.dll $(NUNIT_TESTS) $(DOC_FILE_CLEAN) FsProviderTests.test.xml AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

