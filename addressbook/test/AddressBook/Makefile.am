# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

AddressBookTests_CSFILES = $(srcdir)/AddressBookTests.cs
AddressBookTests_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookTests_CSFILES))
AddressBookTests_INCLUDES =
AddressBookTests_RESOURCES =
AddressBookTests_FLAGS = $(CSC_EXEFLAG)
AddressBookTests_LIBS = $(SYSTEM_XML) Novell.AddressBook.dll
AddressBookTests_LIBPATH = ../../src/AddressBook

EXTRA_DIST = $(AddressBookTests_CSFILES) $(AddressBookTests_RESOURCES)

NUNIT_TESTS = ABTests.exe

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

ABTests.exe: $(AddressBookTests_CSFILES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(AddressBookTests_FLAGS) $(AddressBookTests_LIBS:%=/r:%) $(AddressBookTests_LIBPATH:%=/lib:%) $(AddressBookTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = ABTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ABTests.exe ABTests.test.xml $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:ABTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) ABTests.doc.xml ABTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

