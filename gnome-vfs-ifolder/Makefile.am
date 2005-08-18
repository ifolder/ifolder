SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS) package

EXTRA_DIST = $(srcdir)/$(PACKAGE)*.pc.in

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) $(srcdir)/$(PACKAGE)*.pc $(DESTDIR)$(libdir)/pkgconfig
	
installcheck-summary:
	$(MAKE) -C src $@
	
uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f $(PACKAGE)*.pc

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure $(PACKAGE)-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: package package-sdk package-clean

package:
	$(MAKE) -C package all

package-sdk:
	$(MAKE) -C package sdk

package-clean:
	$(MAKE) -C package clean

doc:


