# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

AddressBookCmd_CSFILES = $(srcdir)/AddressBookCmd.cs
AddressBookCmd_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookCmd_CSFILES))
AddressBookCmd_INCLUDES =
AddressBookCmd_RESOURCES =
AddressBookCmd_FLAGS = $(CSC_EXEFLAG)
AddressBookCmd_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Novell.AddressBook.dll
AddressBookCmd_LIBPATH = ../AddressBook

NUNIT_TESTS :=

EXTRA_DIST = $(srcdir)/addressbookcmd.in $(AddressBookCmd_CSFILES)

CUR_DIR := $(shell pwd)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: AddressBookCmd.exe

AddressBookCmd.exe: $(AddressBookCmd_CSFILES) $(AddressBookCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(AddressBookCmd_FLAGS) $(AddressBookCmd_LIBS:%=/r:%) $(AddressBookCmd_LIBPATH:%=/lib:%) $(AddressBookCmd_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = AddressBookCmd.pdb
endif
endif

install-exec-local: AddressBookCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) AddressBookCmd.exe $(DESTDIR)$(bindir)
if MONO
	$(INSTALL_PROGRAM) addressbookcmd $(DESTDIR)$(bindir)
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f addressbookcmd AddressBookCmd.exe $(DEBUG_FILES)

installcheck-local: install
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AddressBookCmdTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = AddressBookCmd.exe $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

