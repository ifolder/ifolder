# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

AddressBook_CSFILES = $(srcdir)/Address.cs $(srcdir)/AddressBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/Common.cs $(srcdir)/Contact.cs $(srcdir)/Email.cs $(srcdir)/Group.cs $(srcdir)/InstantMessage.cs $(srcdir)/Manager.cs $(srcdir)/Name.cs $(srcdir)/Phone.cs
AddressBook_CSFILES_CSC := $(subst /,$(SEP),$(AddressBook_CSFILES))
AddressBook_INCLUDES =
AddressBook_RESOURCES =
AddressBook_FLAGS = $(CSC_LIBFLAG)
AddressBook_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll StoreProvider.dll Storage.dll
AddressBook_LIBPATH = ../Common ../CollectionEvents ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../StoreProvider ../CollectionWatcher ../StoreWatcher ../Storage

NUNIT_TESTS := AddressBookTests.exe

AddressBookTests_CSFILES = $(srcdir)/AddressBookTests/AddressBookTests.cs $(srcdir)/AddressBookTests/AssemblyInfo.cs
AddressBookTests_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookTests_CSFILES))
AddressBookTests_INCLUDES =
AddressBookTests_RESOURCES =
AddressBookTests_FLAGS = $(CSC_LIBFLAG)
AddressBookTests_LIBS = $(AddressBook_LIBS) $(SYSTEM_XML) nunit.util.dll nunit.framework.dll AddressBook.dll
AddressBookTests_LIBPATH = $(AddressBook_LIBPATH) AddressBookTests $(NUNIT_LIBPATH)

EXTRA_DIST = $(AddressBook_CSFILES) $(AddressBookTests_CSFILES)

CUR_DIR := $(shell pwd)

all: AddressBook.dll

AddressBook.dll: $(AddressBook_CSFILES) $(AddressBook_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBook_FLAGS) $(AddressBook_LIBS:%=/r:%) $(AddressBook_LIBPATH:%=/lib:%) $(AddressBook_CSFILES_CSC) /doc:AddressBook.doc.xml

AddressBookTests.exe: $(AddressBookTests_CSFILES) $(AddressBookTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBookTests_FLAGS) $(AddressBookTests_LIBS:%=/r:%) $(AddressBookTests_LIBPATH:%=/lib:%) $(AddressBookTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = AddressBook.pdb
DEBUG_CHECK_FILES = AddressBookTests.pdb
endif
endif

install-exec-local: AddressBook.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) AddressBook.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f AddressBook.dll AddressBookTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AddressBookTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = AddressBook.dll $(NUNIT_TESTS) AddressBook.doc.xml AddressBookTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

