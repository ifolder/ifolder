AddressBook_CSFILES = $(srcdir)/Address.cs $(srcdir)/AddressBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/Common.cs $(srcdir)/Contact.cs $(srcdir)/ContactList.cs $(srcdir)/Email.cs $(srcdir)/Manager.cs $(srcdir)/Name.cs $(srcdir)/Phone.cs
AddressBook_CSFILES_CSC := $(subst /,$(SEP),$(AddressBook_CSFILES))
AddressBook_INCLUDES = 
AddressBook_RESOURCES =
AddressBook_FLAGS = $(CSC_LIBFLAG)
AddressBook_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Configuration.dll StoreProvider.dll CollectionEvents.dll Identity.dll CollectionStore.dll Identity.dll WorkGroupIdentity.dll StoreProvider.dll
AddressBook_LIBPATH = ../Common ../CollectionEvents ../CollectionStore ../Configuration ../StoreProvider ../Identity ../CollectionWatcher ../StoreWatcher ../WorkGroupIdentity

NUNIT_TESTS := AddressBookTests.exe

AddressBookTests_CSFILES = $(srcdir)/AddressBookTests/AddressBookTests.cs $(srcdir)/AddressBookTests/AssemblyInfo.cs
AddressBookTests_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookTests_CSFILES))
AddressBookTests_INCLUDES = 
AddressBookTests_RESOURCES =
AddressBookTests_FLAGS = $(CSC_LIBFLAG)
AddressBookTests_LIBS = $(AddressBook_LIBS) $(SYSTEM_XML) nunit.util.dll nunit.framework.dll AddressBook.dll
AddressBookTests_LIBPATH = $(AddressBook_LIBPATH) AddressBookTests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(AddressBook_CSFILES) $(AddressBookTests_CSFILES)

CUR_DIR := $(shell pwd)

all: AddressBook.dll

AddressBook.dll: $(AddressBook_CSFILES) $(AddressBook_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBook_FLAGS) $(AddressBook_LIBS:%=/r:%) $(AddressBook_LIBPATH:%=/lib:%) $(AddressBook_CSFILES_CSC) /doc:AddressBook.xml

AddressBookTests.exe: $(AddressBookTests_CSFILES) $(AddressBookTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBookTests_FLAGS) $(AddressBookTests_LIBS:%=/r:%) $(AddressBookTests_LIBPATH:%=/lib:%) $(AddressBookTests_CSFILES_CSC)
	
install-exec-local: AddressBook.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) AddressBook.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f AddressBook.dll AddressBookTests.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AddressBookTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = AddressBook.dll $(NUNIT_TESTS) AddressBook.xml AddressBookTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

