# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SyncEndToEndTests_CSFILES = $(srcdir)/SyncEndToEndTests.cs $(srcdir)/AssemblyInfo.cs
SyncEndToEndTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncEndToEndTests_CSFILES))
SyncEndToEndTests_INCLUDES =
SyncEndToEndTests_RESOURCES =
SyncEndToEndTests_FLAGS = $(CSC_LIBFLAG)
SyncEndToEndTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
SyncEndToEndTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(SyncEndToEndTests_CSFILES)

NUNIT_TESTS = SyncEndToEndTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SyncEndToEndTests.dll: $(SyncEndToEndTests_CSFILES) $(SyncEndToEndTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncEndToEndTests_FLAGS) $(SyncEndToEndTests_LIBS:%=/r:%) $(SyncEndToEndTests_LIBPATH:%=/lib:%) $(SyncEndToEndTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SyncEndToEndTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SyncEndToEndTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SyncEndToEndTests.doc.xml SyncEndToEndTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

