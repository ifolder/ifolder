if LINUX
SUBDIRS = Simias.dll SimiasApp mDnsService AddressBook SimiasAspService WebService
DIST_SUBDIRS = $(SUBDIRS)
endif

if DARWIN
SUBDIRS = Simias.dll SimiasApp mDnsService AddressBook SimiasAspService WebService
DIST_SUBDIRS = $(SUBDIRS)
endif

if WINDOWS
SUBDIRS = Simias.dll SimiasApp mDnsService AddressBook SimiasAspService WebService
DIST_SUBDIRS = $(SUBDIRS)
endif


EXTRA_DIST = ifdata simias-snakeoil.keys $(srcdir)/install-simias-to-gac.in

install-exec-local: install-simias-to-gac
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifdata install-simias-to-gac $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ifdata install-simias-to-gac

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

