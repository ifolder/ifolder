if LINUX
SUBDIRS = Simias.dll FlaimProvider SimiasApp mDnsService AddressBook WebService IProcEvent
DIST_SUBDIRS = $(SUBDIRS)
endif

if DARWIN
SUBDIRS = Simias.dll SimiasApp mDnsService AddressBook WebService IProcEvent
DIST_SUBDIRS = $(SUBDIRS)
endif

if WINDOWS
SUBDIRS = Simias.dll FlaimProvider SimiasApp mDnsService AddressBook WebService IProcEvent
DIST_SUBDIRS = $(SUBDIRS)
endif


EXTRA_DIST = ifdata simias-snakeoil.keys $(srcdir)/install-simias-to-gac.in

install-exec-local: install-simias-to-gac
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifdata $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) install-simias-to-gac $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f install-simias-to-gac
	cd $(DESTDIR)$(webbindir); rm -f ifdata

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

