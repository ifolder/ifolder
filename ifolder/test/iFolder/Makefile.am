# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

iFolderTests_CSFILES = $(srcdir)/iFolderTests.cs
iFolderTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderTests_CSFILES))
iFolderTests_INCLUDES =
iFolderTests_RESOURCES =
iFolderTests_FLAGS = $(CSC_EXEFLAG)
iFolderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Novell.iFolder.dll
iFolderTests_LIBPATH = ../../src/iFolder

EXTRA_DIST = $(iFolderTests_CSFILES) $(iFolderTests_RESOURCES)

NUNIT_TESTS = iFolderTests.exe

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

iFolderTests.exe: $(iFolderTests_CSFILES)
	$(CSC) /out:$@ $(CSCFLAGS) $(IFOLDER_LIBS) $(iFolderTests_FLAGS) $(iFolderTests_LIBS:%=/r:%) $(iFolderTests_LIBPATH:%=/lib:%) $(iFolderTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = iFolderTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderTests.exe iFolderTests.test.xml $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:iFolderTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) iFolderTests.doc.xml iFolderTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

