SUBDIRS = src package

EXTRA_DIST = tools external

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C package $@

clean-local:
	rm -rf stage *.suo

maintainer-clean-local:
	rm -f configure Makefile.in simias-*.tar.gz
	rm -f aclocal.m4
	rm -f config.guess config.sub
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache
	
uninstall-local:
	rm -rf $(DESTDIR)$(bindir)/.cstore
