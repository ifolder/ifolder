CollectionStore_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/AccessControl.cs $(srcdir)/Alias.cs $(srcdir)/Identity.cs $(srcdir)/IdentityManager.cs $(srcdir)/LocalAddressBook.cs $(srcdir)/CacheNode.cs $(srcdir)/Collection.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/FileEntry.cs $(srcdir)/FileSystemEntry.cs $(srcdir)/ICSEnumerator.cs $(srcdir)/MultiValuedList.cs $(srcdir)/Node.cs $(srcdir)/NodeStream.cs $(srcdir)/Property.cs $(srcdir)/PropertyList.cs $(srcdir)/Store.cs
CollectionStore_CSFILES_CSC := $(subst /,$(SEP),$(CollectionStore_CSFILES))
CollectionStore_INCLUDES =
CollectionStore_RESOURCES =
CollectionStore_FLAGS = $(CSC_LIBFLAG)
CollectionStore_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll System.Security.dll CollectionEvents.dll
CollectionStore_LIBPATH = ../Configuration ../StoreProvider ../Identity ../CollectionEvents

NUNIT_TESTS := CollectionStoreTests.dll

CollectionStoreTests_CSFILES = $(srcdir)/CollectionStoreTests/AssemblyInfo.cs $(srcdir)/CollectionStoreTests/CollectionStoreTests.cs
CollectionStoreTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionStoreTests_CSFILES))
CollectionStoreTests_INCLUDES =
CollectionStoreTests_RESOURCES =
CollectionStoreTests_FLAGS = $(CSC_LIBFLAG)
CollectionStoreTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Configuration.dll StoreProvider.dll System.Security.dll CollectionEvents.dll CollectionStore.dll
CollectionStoreTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin ../Configuration ../StoreProvider ../Identity ../CollectionEvents

EXTRA_DIST = $(CollectionStore_CSFILES) $(CollectionStoreTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CollectionStore.dll

CollectionStore.dll: $(CollectionStore_CSFILES) $(CollectionStore_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionStore_FLAGS) $(CollectionStore_LIBS:%=/r:%) $(CollectionStore_LIBPATH:%=/lib:%) $(CollectionStore_CSFILES_CSC) /doc:CollectionStore.xml

CollectionStoreTests.dll: $(CollectionStoreTests_CSFILES) $(CollectionStoreTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionStoreTests_FLAGS) $(CollectionStoreTests_LIBS:%=/r:%) $(CollectionStoreTests_LIBPATH:%=/lib:%) $(CollectionStoreTests_CSFILES_CSC)
	
install-exec-local: CollectionStore.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CollectionStore.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CollectionStore.dll CollectionStoreTests.xml $(NUNIT_TESTS); rm -rf CollectionStoreTestDir .cstore

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionStoreTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CollectionStore.dll $(NUNIT_TESTS) CollectionStore.xml
CLEAN_DIRS = CollectionStoreTestDir .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

