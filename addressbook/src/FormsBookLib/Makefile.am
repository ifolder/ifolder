# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

FormsBookLib_CSFILES = $(srcdir)/AddBook.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BooksContacts.cs $(srcdir)/ContactEditor.cs $(srcdir)/ContactPicker.cs $(srcdir)/FullName.cs $(srcdir)/FullAddress.cs $(srcdir)/GenericEntry.cs $(srcdir)/TelephoneEntry.cs $(srcdir)/EmailEntry.cs $(srcdir)/AddressEntry.cs $(srcdir)/FullNameParser.cs
FormsBookLib_CSFILES_CSC := $(subst /,$(SEP),$(FormsBookLib_CSFILES))
FormsBookLib_INCLUDES = 
FormsBookLib_RESOURCES =
FormsBookLib_FLAGS = $(CSC_LIBFLAG)
FormsBookLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Simias.dll
FormsBookLib_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib

DATA_FILES = ifolder_add_bk.ico ifolder_contact_card.ico ifolder_me_card.ico

EXTRA_DIST = $(FormsBookLib_CSFILES) $(DATA_FILES)

all: FormsBookLib.dll

FormsBookLib.dll: $(FormsBookLib_CSFILES) $(FormsBookLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(FormsBookLib_FLAGS) $(SIMIAS_LIBS) $(FormsBookLib_LIBS:%=/r:%) $(FormsBookLib_LIBPATH:%=/lib:%) $(FormsBookLib_CSFILES_CSC) /doc:FormsBookLib.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = FormsBookLib.pdb
endif
endif

install-exec-local: FormsBookLib.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) FormsBookLib.dll $(DATA_FILES) $(DEBUG_FILES) $(DESTDIR)$(libdir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f FormsBookLib.dll $(DATA_FILES) $(DEBUG_FILES)
    
installcheck-local: install
	
CLEAN_FILES = FormsBookLib.dll FormsBookLib.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

