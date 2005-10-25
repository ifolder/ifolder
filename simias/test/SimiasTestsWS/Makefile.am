# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasTestsWS_CSFILES = $(srcdir)/SimiasTests.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/DomainService.cs
SimiasTestsWS_CSFILES_CSC := $(subst /,$(SEP),$(SimiasTestsWS_CSFILES))
SimiasTestsWS_INCLUDES =
SimiasTestsWS_RESOURCES =
SimiasTestsWS_FLAGS = $(CSC_LIBFLAG)
SimiasTestsWS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll nunit.framework.dll
SimiasTestsWS_LIBPATH = ../../src/SimiasLib.dll $(NUNIT_LIBPATH)

EXTRA_DIST = SimiasTests.asmx $(SimiasTestsWS_CSFILES)

NUNIT_TESTS := SimiasTests.Web.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SimiasTests.Web.dll: $(SimiasTestsWS_CSFILES) $(SimiasTestsWS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasTestsWS_FLAGS) $(SimiasTestsWS_LIBS:%=/r:%) $(SimiasTestsWS_LIBPATH:%=/lib:%) $(SimiasTestsWS_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SimiasTests.Web.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimiasTests.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(webdir); rm -f  SimiasTests.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SimiasTests.doc.xml SimiasTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

