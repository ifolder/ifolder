# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Book_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsAddrBook.cs
Book_CSFILES_CSC = $(subst /,$(SEP),$(Book_CSFILES))
Book_INCLUDES = 
Book_RESOURCES = $(srcdir)/App$(ICON_EXT)
Book_FLAGS = $(CSC_WINEXEFLAG)
Book_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) FormsBookLib.dll  Simias.dll Novell.AddressBook.dll
Book_LIBPATH = ../FormsBookLib

DATA_FILES = $(srcdir)/address_app.ico $(srcdir)/App.ico
PNG_FILES = $(srcdir)/../GtkAddrBook/blankhead.png $(srcdir)/../GtkAddrBook/book.png $(srcdir)/../GtkAddrBook/contact.png $(srcdir)/../GtkAddrBook/contact_me.png $(srcdir)/../GtkAddrBook/vcardexport.png $(srcdir)/../GtkAddrBook/vcardimport.png

EXTRA_DIST = $(Book_CSFILES) $(srcdir)/ContactBrowser.exe.manifest $(DATA_FILES) $(PNG_FILES)

all: ContactBrowser.exe

ContactBrowser.exe: $(Book_CSFILES) $(Book_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Book_FLAGS) $(SIMIAS_LIBS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_RESOURCES:%=$(ICON_FLAG)%) $(Book_CSFILES_CSC) /doc:ContactBrowser.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = ContactBrowser.pdb
endif
endif

install-exec-local: ContactBrowser.exe $(DATA_FILES) $(PNG_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ContactBrowser.exe ContactBrowser.exe.manifest $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(PNG_FILES) $(DESTDIR)$(bindir)/res
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ContactBrowser.exe ContactBrowser.exe.manifest $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir)/res; rm -f $(DATA_FILES:$(srcdir)/%=%) $(PNG_FILES:$(srcdir)/../GtkAddrBook/%=%)
    
installcheck-local: install
	
CLEAN_FILES = ContactBrowser.exe ContactBrowser.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

