# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

StorageTests_CSFILES = $(srcdir)/CollectionStoreTests.cs $(srcdir)/AssemblyInfo.cs
StorageTests_CSFILES_CSC := $(subst /,$(SEP),$(StorageTests_CSFILES))
StorageTests_INCLUDES =
StorageTests_RESOURCES =
StorageTests_FLAGS = $(CSC_LIBFLAG)
StorageTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
StorageTests_LIBPATH = ../../src/Simias.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(StorageTests_CSFILES)

NUNIT_TESTS := StorageTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

StorageTests.dll: $(StorageTests_CSFILES) $(StorageTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(StorageTests_FLAGS) $(StorageTests_LIBS:%=/r:%) $(StorageTests_LIBPATH:%=/lib:%) $(StorageTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = StorageTests.pdb
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
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:StorageTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) StorageTests.doc.xml StorageTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

