SUBDIRS = src package

DIST_SUBDIRS = $(SUBDIRS) doc

EXTRA_DIST = tools external $(srcdir)/simias.pc.in

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) $(srcdir)/simias.pc $(DESTDIR)$(libdir)/pkgconfig

installcheck-summary:
	$(MAKE) -C src $@
	
uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f simias.pc

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure simias-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: doc doc-clean
doc:
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

.PHONY: package package-sdk package-clean package-install package-uninstall

package package-sdk package-clean package-install package-uninstall:
	$(MAKE) -C package $@

