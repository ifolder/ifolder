# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasTests_CSFILES = $(srcdir)/CommonTests.cs $(srcdir)/ConfigurationTests.cs $(srcdir)/AssemblyInfo.cs
SimiasTests_CSFILES_CSC := $(subst /,$(SEP),$(SimiasTests_CSFILES))
SimiasTests_INCLUDES =
SimiasTests_RESOURCES =
SimiasTests_FLAGS = $(CSC_LIBFLAG)
SimiasTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
SimiasTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(SimiasTests_CSFILES)

NUNIT_TESTS = SimiasTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SimiasTests.dll: $(SimiasTests_CSFILES) $(SimiasTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasTests_FLAGS) $(SimiasTests_LIBS:%=/r:%) $(SimiasTests_LIBPATH:%=/lib:%) $(SimiasTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SimiasTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(libdir)

uninstall-local:

installcheck-local: install
if LINUX	
	export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir):$(NUNIT_LIBPATH)
endif        
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SimiasTests.doc.xml SimiasTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

