# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

CommonTests_CSFILES = $(srcdir)/CommonTests.cs $(srcdir)/AssemblyInfo.cs
CommonTests_CSFILES_CSC := $(subst /,$(SEP),$(CommonTests_CSFILES))
CommonTests_INCLUDES =
CommonTests_RESOURCES =
CommonTests_FLAGS = $(CSC_LIBFLAG)
CommonTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) SimiasClient.dll SimiasLib.dll nunit.framework.dll
CommonTests_LIBPATH = ../../src/SimiasLib.dll ../../src/SimiasClient $(NUNIT_LIBPATH)

EXTRA_DIST = $(CommonTests_CSFILES)

NUNIT_TESTS = CommonTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

CommonTests.dll: $(CommonTests_CSFILES) $(CommonTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonTests_FLAGS) $(CommonTests_LIBS:%=/r:%) $(CommonTests_LIBPATH:%=/lib:%) $(CommonTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = CommonTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:CommonTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) CommonTests.doc.xml CommonTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

