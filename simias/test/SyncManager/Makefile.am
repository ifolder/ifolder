# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SyncManagerTests_CSFILES = $(srcdir)/SyncManagerTests.cs $(srcdir)/AssemblyInfo.cs
SyncManagerTests_CSFILES_CSC := $(subst /,$(SEP),$(SyncManagerTests_CSFILES))
SyncManagerTests_INCLUDES =
SyncManagerTests_RESOURCES =
SyncManagerTests_FLAGS = $(CSC_LIBFLAG)
SyncManagerTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
SyncManagerTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(SyncManagerTests_CSFILES)

NUNIT_TESTS = SyncManagerTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SyncManagerTests.dll: $(SyncManagerTests_CSFILES) $(SyncManagerTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncManagerTests_FLAGS) $(SyncManagerTests_LIBS:%=/r:%) $(SyncManagerTests_LIBPATH:%=/lib:%) $(SyncManagerTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SyncManagerTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SyncManagerTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SyncManagerTests.doc.xml SyncManagerTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

