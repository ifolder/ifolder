# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

CollectionStore_CSFILES = $(srcdir)/AccessControl.cs $(srcdir)/Alias.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BaseContact.cs $(srcdir)/BaseFileNode.cs $(srcdir)/Collection.cs $(srcdir)/Collision.cs $(srcdir)/DirNode.cs $(srcdir)/Exception.cs $(srcdir)/FileNode.cs $(srcdir)/ICSEnumerator.cs $(srcdir)/IdentityManager.cs $(srcdir)/LinkNode.cs $(srcdir)/LocalAddressBook.cs $(srcdir)/MultiValuedList.cs $(srcdir)/Node.cs $(srcdir)/NodeTypes.cs $(srcdir)/Property.cs $(srcdir)/PropertyList.cs $(srcdir)/ShallowNode.cs $(srcdir)/Store.cs $(srcdir)/StoreFileNode.cs
CollectionStore_CSFILES_CSC := $(subst /,$(SEP),$(CollectionStore_CSFILES))
CollectionStore_INCLUDES =
CollectionStore_RESOURCES =
CollectionStore_FLAGS = $(CSC_LIBFLAG)
CollectionStore_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll Storage.dll StoreProvider.dll Common.dll CollectionEvents.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll
CollectionStore_LIBPATH = ../Common ../Storage ../StoreProvider ../CollectionEvents $(srcdir)/../../external/SecureChannelSinks

NUNIT_TESTS := CollectionStoreTests.dll

CollectionStoreTests_CSFILES = $(srcdir)/CollectionStoreTests/AssemblyInfo.cs $(srcdir)/CollectionStoreTests/CollectionStoreTests.cs
CollectionStoreTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionStoreTests_CSFILES))
CollectionStoreTests_INCLUDES =
CollectionStoreTests_RESOURCES =
CollectionStoreTests_FLAGS = $(CSC_LIBFLAG)
CollectionStoreTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll nunit.util.dll nunit.framework.dll Common.dll CollectionStore.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Storage.dll
CollectionStoreTests_LIBPATH = $(NUNIT_LIBPATH) ../Common $(srcdir)/../../external/SecureChannelSinks ../Storage

EXTRA_DIST = $(CollectionStore_CSFILES) $(CollectionStoreTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CollectionStore.dll

CollectionStore.dll: $(CollectionStore_CSFILES) $(CollectionStore_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionStore_FLAGS) $(CollectionStore_LIBS:%=/r:%) $(CollectionStore_LIBPATH:%=/lib:%) $(CollectionStore_CSFILES_CSC) /doc:CollectionStore.doc.xml

CollectionStoreTests.dll: $(CollectionStoreTests_CSFILES) $(CollectionStoreTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionStoreTests_FLAGS) $(CollectionStoreTests_LIBS:%=/r:%) $(CollectionStoreTests_LIBPATH:%=/lib:%) $(CollectionStoreTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = CollectionStore.pdb
DEBUG_CHECK_FILES = CollectionStoreTests.pdb
endif
endif

install-exec-local: CollectionStore.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CollectionStore.dll ../../external/SecureChannelSinks/*.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CollectionStore.dll CollectionStoreTests.test.xml Novell.Security.SecureSink*.dll $(NUNIT_TESTS); rm -rf CollectionStoreTestDir .cstore $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionStoreTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CollectionStore.dll $(NUNIT_TESTS) CollectionStore.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

