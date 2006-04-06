SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS) doc package test

# remove all of the CVS directories
dist-hook:
	rm -rf `find $(distdir) -name CVS`
	rm -rf `find $(distdir) -name ".svn"`

EXTRA_DIST =

DISTCLEANFILES = intl-extract intltool-update intltool-merge

installcheck-summary:
	$(MAKE) -C src $@

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure nautilus-ifolder-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status config.h.in
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: doc doc-clean package package-clean test test-clean

doc:
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

package:
	$(MAKE) -C package all

package-clean:
	$(MAKE) -C package clean

test:
	$(MAKE) -C test all

test-clean:
	$(MAKE) -C test clean
