# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SyncCommonTests_CSFILES = $(srcdir)/SyncCommonTests.cs $(srcdir)/AssemblyInfo.cs
SyncCommonTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncCommonTests_CSFILES))
SyncCommonTests_INCLUDES =
SyncCommonTests_RESOURCES =
SyncCommonTests_FLAGS = $(CSC_LIBFLAG)
SyncCommonTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
SyncCommonTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(SyncCommonTests_CSFILES)

NUNIT_TESTS = SyncCommonTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SyncCommonTests.dll: $(SyncCommonTests_CSFILES) $(SyncCommonTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncCommonTests_FLAGS) $(SyncCommonTests_LIBS:%=/r:%) $(SyncCommonTests_LIBPATH:%=/lib:%) $(SyncCommonTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SyncCommonTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SyncCommonTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SyncCommonTests.doc.xml SyncCommonTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

