SUBDIRS = $(COMMON_SUBDIRS) $(PLATFORM_SUBDIRS)

DIST_SUBDIRS = $(COMMON_SUBDIRS) $(LINUX_SUBDIRS) $(WINDOWS_SUBDIRS) $(NOBUILD_SUBDIRS)

EXTRA_DIST = $(srcdir)/ifolder-snakeoil.keys $(srcdir)/install-ifolder-to-gac.in

install-exec-local: install-ifolder-to-gac
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) install-ifolder-to-gac $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f install-ifolder-to-gac

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

