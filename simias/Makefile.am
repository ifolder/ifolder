SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS) doc package test

EXTRA_DIST = tools external $(srcdir)/simias.pc.in

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig $(DESTDIR)$(datadir)/simias
	$(INSTALL_PROGRAM) $(srcdir)/COPYING $(DESTDIR)$(datadir)/simias
	$(INSTALL_PROGRAM) simias.pc $(DESTDIR)$(libdir)/pkgconfig

installcheck-summary:
	$(MAKE) -C src $@

uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f simias.pc
	cd $(DESTDIR)$(datadir)/simias; rm -f COPYING

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

.PHONY: test
test: install
	$(MAKE) -C test test
		
test-clean:
	$(MAKE) -C test clean
		
.PHONY: doc doc-clean
doc:
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

.PHONY: package package-sdk package-clean

package:
	$(MAKE) -C package all

package-sdk:
	$(MAKE) -C package sdk

package-clean:
	$(MAKE) -C package clean

if WINDOWS
#
# On Windows, overwrite dist targets to alter how gzipped-tarballs are created
#
dist-gzip: distdir
#	$(AMTAR) chof - $(distdir) | GZIP=$(GZIP_ENV) gzip -c >$(distdir).tar.gz
	GZIP=$(GZIP_ENV) $(AMTAR) zchof $(distdir).tar.gz $(distdir)
	$(am__remove_distdir)

dist dist-all: distdir
#	$(AMTAR) chof - $(distdir) | GZIP=$(GZIP_ENV) gzip -c >$(distdir).tar.gz
	GZIP=$(GZIP_ENV) $(AMTAR) zchof $(distdir).tar.gz $(distdir)
	$(am__remove_distdir)

endif # WINDOWS
