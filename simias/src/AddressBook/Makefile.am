# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

AddressBook_CSFILES = $(srcdir)/Address.cs $(srcdir)/AddressBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/Common.cs $(srcdir)/Contact.cs $(srcdir)/Email.cs $(srcdir)/Group.cs $(srcdir)/InstantMessage.cs $(srcdir)/Manager.cs $(srcdir)/Name.cs $(srcdir)/Phone.cs
AddressBook_CSFILES_CSC := $(subst /,$(SEP),$(AddressBook_CSFILES))
AddressBook_INCLUDES =
AddressBook_RESOURCES =
AddressBook_FLAGS = $(CSC_LIBFLAG)
AddressBook_LIBS = System.dll System.Data.dll $(SYSTEM_XML) SimiasClient.dll SimiasLib.dll
AddressBook_LIBPATH = ../SimiasClient ../SimiasLib.dll

EXTRA_DIST = $(AddressBook_CSFILES)

CUR_DIR := $(shell pwd)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Novell.AddressBook.dll

Novell.AddressBook.dll: $(AddressBook_CSFILES) $(AddressBook_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AddressBook_FLAGS) $(AddressBook_LIBS:%=/r:%) $(AddressBook_LIBPATH:%=/lib:%) $(AddressBook_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.AddressBook.pdb
endif
endif

install-exec-local: Novell.AddressBook.dll
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Novell.AddressBook.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Novell.AddressBook.dll $(DEBUG_FILES)

CLEAN_FILES = Novell.AddressBook.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

