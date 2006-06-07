# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

iFolderComponentTests_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderComponentTests.cs
iFolderComponentTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponentTests_CSFILES))
iFolderComponentTests_INCLUDES = 
iFolderComponentTests_RESOURCES =
iFolderComponentTests_FLAGS = $(CSC_LIBFLAG)
iFolderComponentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolderComponent.dll nunit.framework.dll nunit.extensions.dll
iFolderComponentTests_LIBPATH = $(iFolderComponent_LIBPATH) $(NUNIT_LIBPATH)

EXTRA_DIST = $(iFolderComponentTests_CSFILES) $(iFolderComponentTests_RESOURCES)

NUNIT_TESTS := iFolderComponentTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

iFolderComponentTests.dll: $(iFolderComponentTests_CSFILES) $(iFolderComponentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(iFolderComponentTests_FLAGS) $(iFolderComponentTests_LIBS:%=/r:%) $(iFolderComponentTests_LIBPATH:%=/lib:%) $(iFolderComponentTests_CSFILES_CSC)

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = iFolderComponentTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderComponentTests.exe iFolderComponentTests.test.xml $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:iFolderComponentTests.test.xml $(NUNIT_TESTS)

.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) iFolderComponentTests.doc.xml iFolderComponentTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

