DIST_SUBDIRS = SimiasClient SimiasLib.dll FlaimProvider WebService SyncService POBoxWS SimiasApp libsimias

if DARWIN
SUBDIRS = SimiasClient SimiasLib.dll FlaimProvider WebService SyncService POBoxWS SimiasApp libsimias

endif

if LINUX
SUBDIRS = SimiasClient SimiasLib.dll FlaimProvider WebService SyncService POBoxWS SimiasApp libsimias

endif

if WINDOWS
SUBDIRS = SimiasClient SimiasLib.dll FlaimProvider WebService SyncService POBoxWS SimiasApp libsimias

endif

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

