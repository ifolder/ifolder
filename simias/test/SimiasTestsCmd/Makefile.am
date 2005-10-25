# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasTestsCmd_CSFILES = $(srcdir)/SimiasTestsCmd.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/SimiasTestsProxy.cs
SimiasTestsCmd_CSFILES_CSC := $(subst /,$(SEP),$(SimiasTestsCmd_CSFILES))
SimiasTestsCmd_INCLUDES =
SimiasTestsCmd_RESOURCES =
SimiasTestsCmd_FLAGS = $(CSC_LIBFLAG)
SimiasTestsCmd_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll nunit.framework.dll
SimiasTestsCmd_LIBPATH = ../../src/SimiasLib.dll $(NUNIT_LIBPATH)

EXTRA_DIST = $(SimiasTestsCmd_CSFILES)

NUNIT_TESTS := SimiasTestsCmd.exe

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SimiasTestsCmd.exe: $(SimiasTestsCmd_CSFILES) $(SimiasTestsCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasTestsCmd_FLAGS) $(SimiasTestsCmd_LIBS:%=/r:%) $(SimiasTestsCmd_LIBPATH:%=/lib:%) $(SimiasTestsCmd_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SimiasTestsCmd.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTestsCmd.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SimiasTestsCmd.doc.xml SimiasTestsCmd.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

