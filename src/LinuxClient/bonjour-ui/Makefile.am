# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

DOMAINUILIB_CSFILES = $(srcdir)/BonjourDomainProviderUI.cs $(srcdir)/BonjourAccountDialog.cs


DOMAINUILIB_CSFILES_CSC := $(subst /,$(SEP),$(DOMAINUILIB_CSFILES))
DOMAINUILIB_INCLUDES =
DOMAINUILIB_RESOURCES =
DOMAINUILIB_FLAGS = $(CSC_LIBFLAG)
DOMAINUILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll Novell.iFolder.UI.gtk.dll Mono.Posix.dll
DOMAINUILIB_LIBPATH = $(srcdir)/../library

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif


all: Novell.iFolder.BonjourDomainProviderUI.gtk.dll

Novell.iFolder.BonjourDomainProviderUI.gtk.dll: $(DOMAINUILIB_CSFILES) $(DOMAINUILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(DOMAINUILIB_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(GCONF_SHARP_LIBS) $(DOMAINUILIB_LIBS:%=/r:%) $(DOMAINUILIB_LIBPATH:%=/lib:%) $(DOMAINUILIB_RESOURCES:%=$(ICON_FLAG)%) $(DOMAINUILIB_CSFILES_CSC) $(DOC_FILE_CMD)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Novell.iFolder.BonjourDomainProviderUI.gtk.dll $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.BonjourDomainProviderUI.gtk.dll

CLEAN_FILES = Novell.iFolder.BonjourDomainProviderUI.gtk.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf *.dbg $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
