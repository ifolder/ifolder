# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

FormsBookLib_CSFILES = $(srcdir)/AddBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BooksContacts.cs $(srcdir)/ContactEditor.cs $(srcdir)/ContactPicker.cs $(srcdir)/FullName.cs $(srcdir)/FullAddress.cs $(srcdir)/GenericEntry.cs $(srcdir)/TelephoneEntry.cs $(srcdir)/EmailEntry.cs $(srcdir)/AddressEntry.cs $(srcdir)/FullNameParser.cs
FormsBookLib_CSFILES_CSC := $(subst /,$(SEP),$(FormsBookLib_CSFILES))
FormsBookLib_INCLUDES =
FormsBookLib_RESOURCES =
FormsBookLib_FLAGS = $(CSC_LIBFLAG)
FormsBookLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Simias.dll Novell.AddressBook.dll
FormsBookLib_LIBPATH =

DATA_FILES = $(srcdir)/ifolder_add_bk.ico $(srcdir)/ifolder_contact_card.ico $(srcdir)/ifolder_contact_full.ico $(srcdir)/ifolder_contact_read.ico $(srcdir)/ifolder_contact_read_write.ico $(srcdir)/ifolder_me_card.ico $(srcdir)/contactimport_16.ico $(srcdir)/contactexport_16.ico

EXTRA_DIST = $(FormsBookLib_CSFILES) $(DATA_FILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: FormsBookLib.dll

FormsBookLib.dll: $(FormsBookLib_CSFILES) $(FormsBookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FormsBookLib_FLAGS) $(SIMIAS_LIBS) $(FormsBookLib_LIBS:%=/r:%) $(FormsBookLib_LIBPATH:%=/lib:%) $(FormsBookLib_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = FormsBookLib.pdb
endif
endif

install-exec-local: FormsBookLib.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/res
	$(INSTALL_PROGRAM) FormsBookLib.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(libdir)/res

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f FormsBookLib.dll $(DATA_FILES) $(DEBUG_FILES)
	cd $(DESTDIR)$(libdir)/res; rm -f $(DATA_FILES:$(srcdir)/%=%)

installcheck-local: install
	
CLEAN_FILES = FormsBookLib.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

