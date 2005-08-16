SUBDIRS = src

DIST_SUBDIRS = package $(SUBDIRS)

EXTRA_DIST = tools

.PHONY: doc doc-clean
doc:
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

.PHONY: package package-clean package-install package-uninstall
package:
	$(MAKE) -C package all

package-clean package-install package-uninstall:
	$(MAKE) -C package $@

installcheck-summary:
	$(MAKE) -C src $@
	
uninstall-local:

clean-local:
	rm -rf stage *.suo

dist-clean-local: doc-clean

maintainer-clean-local:
	rm -f configure Makefile.in config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

