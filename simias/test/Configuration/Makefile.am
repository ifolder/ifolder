# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

ConfigurationTests_CSFILES = $(srcdir)/ConfigurationTests.cs $(srcdir)/AssemblyInfo.cs
ConfigurationTests_CSFILES_CSC := $(subst /,$(SEP),$(ConfigurationTests_CSFILES))
ConfigurationTests_INCLUDES =
ConfigurationTests_RESOURCES =
ConfigurationTests_FLAGS = $(CSC_LIBFLAG)
ConfigurationTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
ConfigurationTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(ConfigurationTests_CSFILES)

NUNIT_TESTS = ConfigurationTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

ConfigurationTests.dll: $(ConfigurationTests_CSFILES) $(ConfigurationTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ConfigurationTests_FLAGS) $(ConfigurationTests_LIBS:%=/r:%) $(ConfigurationTests_LIBPATH:%=/lib:%) $(ConfigurationTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = ConfigurationTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:ConfigurationTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) ConfigurationTests.doc.xml ConfigurationTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

