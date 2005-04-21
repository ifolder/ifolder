# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

CollectionStoreTests_CSFILES = $(srcdir)/CollectionStoreTests.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/CollectionOwnerTests.cs $(srcdir)/FindMemberTests.cs
CollectionStoreTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionStoreTests_CSFILES))
CollectionStoreTests_INCLUDES =
CollectionStoreTests_RESOURCES =
CollectionStoreTests_FLAGS = $(CSC_LIBFLAG)
CollectionStoreTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
CollectionStoreTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(CollectionStoreTests_CSFILES)

NUNIT_TESTS := CollectionStoreTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

CollectionStoreTests.dll: $(CollectionStoreTests_CSFILES) $(CollectionStoreTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionStoreTests_FLAGS) $(CollectionStoreTests_LIBS:%=/r:%) $(CollectionStoreTests_LIBPATH:%=/lib:%) $(CollectionStoreTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = CollectionStoreTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:CollectionStoreTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) CollectionStoreTests.doc.xml CollectionStoreTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

