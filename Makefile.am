SUBDIRS = src package

EXTRA_DIST = tools

INSTALL_FILES = $(srcdir)/tools/ifolder $(srcdir)/tools/setup-autostart

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C package $@

install-exec-local: 
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f $(notdir $(INSTALL_FILES))
	rm -rf $(DESTDIR)$(bindir)/.cstore

clean-local:
	rm -rf stage *.suo

maintainer-clean-local:
	rm -f configure Makefile.in ifolder-*.tar.gz
	rm -f aclocal.m4
	rm -f config.guess config.sub
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

