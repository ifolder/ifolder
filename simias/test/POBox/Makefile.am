# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

POBoxTests_CSFILES = $(srcdir)/POBoxTests.cs $(srcdir)/AssemblyInfo.cs
POBoxTests_CSFILES_CSC := $(subst /,$(SEP),$(POBoxTests_CSFILES))
POBoxTests_INCLUDES =
POBoxTests_RESOURCES =
POBoxTests_FLAGS = $(CSC_LIBFLAG)
POBoxTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
POBoxTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(POBoxTests_CSFILES)

NUNIT_TESTS := POBoxTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

POBoxTests.dll: $(POBoxTests_CSFILES) $(POBoxTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(POBoxTests_FLAGS) $(POBoxTests_LIBS:%=/r:%) $(POBoxTests_LIBPATH:%=/lib:%) $(POBoxTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = POBoxTests.pdb
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
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:POBoxTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) POBoxTests.doc.xml POBoxTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

