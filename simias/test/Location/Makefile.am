# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

LocationTests_CSFILES = $(srcdir)/LocationTests.cs $(srcdir)/AssemblyInfo.cs
LocationTests_CSFILES_CSC := $(subst /,$(SEP),$(LocationTests_CSFILES))
LocationTests_INCLUDES =
LocationTests_RESOURCES =
LocationTests_FLAGS = $(CSC_LIBFLAG)
LocationTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
LocationTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(LocationTests_CSFILES)

NUNIT_TESTS = LocationTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

LocationTests.dll: $(LocationTests_CSFILES) $(LocationTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(LocationTests_FLAGS) $(LocationTests_LIBS:%=/r:%) $(LocationTests_LIBPATH:%=/lib:%) $(LocationTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = LocationTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:LocationTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) LocationTests.doc.xml LocationTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

