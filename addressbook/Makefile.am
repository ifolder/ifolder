SUBDIRS = src package

DIST_SUBDIRS = $(SUBDIRS) doc

EXTRA_DIST = tools $(srcdir)/addressbook.pc.in

.PHONY: doc doc-clean
doc:
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) $(srcdir)/addressbook.pc $(DESTDIR)$(libdir)/pkgconfig

installcheck-summary:
	$(MAKE) -C src $@

uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f addressbook.pc

clean-local:
	rm -rf stage *.suo

dist-clean-local: doc-clean

maintainer-clean-local:
	rm -f configure Makefile.in addressbook-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: package package-clean package-install package-uninstall

package package-clean package-install package-uninstall:
	$(MAKE) -C package $@
