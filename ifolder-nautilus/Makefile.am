SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = 

installcheck-summary:
	$(MAKE) -C src $@

maintainer-clean-local:
	rm -f configure ifolder-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status config.h.in
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

