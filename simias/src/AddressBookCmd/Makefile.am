# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

AddressBookCmd_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/AddressBookCmd.cs
AddressBookCmd_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookCmd_CSFILES))
AddressBookCmd_INCLUDES =
AddressBookCmd_RESOURCES =
AddressBookCmd_FLAGS = $(CSC_EXEFLAG) $(ICON_FLAG)$(srcdir)/App$(ICON_EXT)
AddressBookCmd_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll
AddressBookCmd_LIBPATH = ../Simias.dll

NUNIT_TESTS :=

AddressBookCmdTests_CSFILES =
AddressBookCmdTests_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookCmdTests_CSFILES))
AddressBookCmdTests_INCLUDES =
AddressBookCmdTests_RESOURCES =
AddressBookCmdTests_FLAGS =
AddressBookCmdTests_LIBS =
AddressBookCmdTests_LIBPATH = $(NUNIT_LIBPATH)

EXTRA_DIST = $(AddressBookCmd_CSFILES) $(AddressBookCmdTests_CSFILES) App$(ICON_EXT)

CUR_DIR := $(shell pwd)

all: AddressBookCmd.exe

AddressBookCmd.exe: $(AddressBookCmd_CSFILES) $(AddressBookCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBookCmd_FLAGS) $(AddressBookCmd_LIBS:%=/r:%) $(AddressBookCmd_LIBPATH:%=/lib:%) $(AddressBookCmd_CSFILES_CSC) /doc:AddressBookCmd.doc.xml

#AddressBookCmdTests.dll: $(AddressBookCmdTests_CSFILES) $(AddressBookCmdTests_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBookCmdTests_FLAGS) $(AddressBookCmdTests_LIBS:%=/r:%) $(AddressBookCmdTests_LIBPATH:%=/lib:%) $(AddressBookCmdTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = AddressBookCmd.pdb
endif
endif

install-exec-local: AddressBookCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) AddressBookCmd.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f AddressBookCmd.exe AddressBookCmdTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES)

installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AddressBookCmdTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = AddressBookCmd.exe $(NUNIT_TESTS) AddressBookCmd.doc.xml AddressBookCmdTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

