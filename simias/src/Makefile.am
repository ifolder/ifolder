SUBDIRS = SimiasClient Simias.dll FlaimProvider mDnsService mDnsDomain AddressBook WebService SyncService POBoxWS SimiasApp SimiasServer

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = ifdata simias-snakeoil.keys SqliteProvider

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) ifdata $(DESTDIR)$(webbindir)

uninstall-local:
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

