if LINUX
SUBDIRS = WebService iFolder TrayIcon GtkTrayApp WebService
endif

if DARWIN
SUBDIRS = WebService iFolder
endif

if WINDOWS
SUBDIRS = WebService iFolder iFolderShell FormsTrayApp FormsInvitationWizard
endif

DIST_SUBDIRS = iFolder TrayIcon GtkTrayApp iFolderShell FormsTrayApp FormsInvitationWizard iFolderGtkUI iFolderGtkApp WebService

EXTRA_DIST = $(srcdir)/ifolder-snakeoil.keys $(srcdir)/install-ifolder-to-gac.in

install-exec-local: install-ifolder-to-gac
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) install-ifolder-to-gac $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f install-ifolder-to-gac

register unregister:
if WINDOWS
	$(MAKE) -C iFolderShell $@
endif

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

