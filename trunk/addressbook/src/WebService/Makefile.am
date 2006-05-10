# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

CABWebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/AddressBookService.cs $(srcdir)/AddressBook.cs $(srcdir)/Contact.cs
CABWebService_CSFILES_CSC := $(subst /,$(SEP),$(CABWebService_CSFILES))
CABWervice_INCLUDES =
CABWebService_RESOURCES =
CABWebService_FLAGS = $(CSC_LIBFLAG)
CABWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.Services.dll
CABWebService_LIBPATH = $(LOG4NET_DIR)

EXTRA_DIST = AddressBook.asmx $(CABWebService_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Novell.AddressBook.Web.dll

Novell.AddressBook.Web.dll: $(CABWebService_CSFILES) $(CABWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CABWebService_FLAGS) $(SIMIAS_LIBS) $(IFOLDER_LIBS) $(CABWebService_LIBS:%=/r:%) $(CABWebService_LIBPATH:%=/lib:%) $(CABWebService_RESOURCES:%=$(ICON_FLAG)%) $(CABWebService_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.AddressBook.Web.pdb
endif
endif

install-exec-local: Novell.AddressBook.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(mkinstalldirs) $(DESTDIR)$(bindir)/bin
	$(INSTALL_PROGRAM) AddressBook.asmx $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.AddressBook.Web.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)/bin
	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f AddressBook.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir)/bin; rm -f Novell.AddressBook.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Novell.AddressBook.Web.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

