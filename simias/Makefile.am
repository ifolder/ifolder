SUBDIRS = src package

EXTRA_DIST = tools external

.PHONY: doc doc-nodeps doc-clean ndoc ndoc-nodeps

if WINDOWS
if NDOC
NDOC_TARGET := ndoc

ndoc: install ndoc-nodeps

ndoc-nodeps: simias.ndoc
	"$(NDOC)" -project=simias.ndoc -ReferencesPath=`cygpath --windows $(bindir)`

endif
endif

DOC_TARGETS := $(NDOC_TARGET)

doc: $(DOC_TARGETS)

doc-clean:
	rm -rf $(DOCDIR)

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C package $@

installcheck-summary:
	$(MAKE) -C src $@
	
uninstall-local:
	rm -rf $(DESTDIR)$(bindir)/.cstore

clean-local:
	rm -rf stage *.suo

dist-clean-local: doc-clean

maintainer-clean-local:
	rm -f configure Makefile.in simias-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

