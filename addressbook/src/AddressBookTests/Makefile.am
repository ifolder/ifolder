# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

AddressBookTests_CSFILES = $(srcdir)/AddressBookTests.cs
AddressBookTests_CSFILES_CSC := $(subst /,$(SEP),$(AddressBookTests_CSFILES))
AddressBookTests_INCLUDES =
AddressBookTests_RESOURCES =
AddressBookTests_FLAGS = $(CSC_EXEFLAG)
AddressBookTests_LIBS = $(SYSTEM_XML) Novell.AddressBook.dll
AddressBookTests_LIBPATH = ../AddressBook

EXTRA_DIST = $(AddressBookTests_CSFILES)

CUR_DIR := $(shell pwd)

all: ABTests.exe

ABTests.exe: $(AddressBookTests_CSFILES) $(AddressBookTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(AddressBookTests_FLAGS) $(AddressBookTests_LIBS:%=/r:%) $(AddressBookTests_LIBPATH:%=/lib:%) $(AddressBookTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = ABTests.pdb
DEBUG_CHECK_FILES = ABTests.pdb
endif
endif

install-exec-local: ABTests.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ABTests.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ABTests.exe ABTests.exe.xml $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

CLEAN_FILES = ABTests.exe ABTests.exe.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

