DIST_SUBDIRS = SimiasClient Simias.dll FlaimProvider mDnsService WebService SyncService POBoxWS SimiasApp SimiasServer mDnsDomain libsimias GaimDomain

if DARWIN
SUBDIRS = SimiasClient Simias.dll FlaimProvider mDnsService WebService SyncService POBoxWS SimiasApp SimiasServer libsimias GaimDomain

endif

if LINUX
SUBDIRS = SimiasClient Simias.dll FlaimProvider mDnsService WebService SyncService POBoxWS SimiasApp SimiasServer libsimias GaimDomain

endif

if WINDOWS
SUBDIRS = SimiasClient Simias.dll FlaimProvider mDnsService WebService SyncService POBoxWS SimiasApp SimiasServer libsimias GaimDomain

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

