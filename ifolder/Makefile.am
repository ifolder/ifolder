SUBDIRS = src

EXTRA_DIST = tools

DISTCHECK_CONFIGURE_FLAGS = @DISTCHECK_CONFIGURE_FLAGS@

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C src $@

maintainer-clean-local:
	rm -f configure Makefile.in ifolder-*.tar.gz
	rm -f aclocal.m4
	rm -f config.guess config.sub
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache
	for subdir in $(NON_PLATFORM_SUBDIRS); do rm -rf "src/$$subdir/Makefile" "src/$$subdir/Makefile.in"; done
	
uninstall-local:
	rm -rf $(DESTDIR)$(bindir)/.cstore
