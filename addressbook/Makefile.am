SUBDIRS = src package

DIST_SUBDIRS = $(SUBDIRS) doc

EXTRA_DIST = tools

.PHONY: doc doc-clean
doc: $(DOC_TARGETS)
	$(MAKE) -C doc all

doc-clean:
	$(MAKE) -C doc clean

.PHONY: sub-all sub-clean sub-install sub-uninstall sub-installcheck
.PHONY: sub-distclean sub-maintainer-clean sub-package sub-installcheck-summary

clean: sub-clean
	rm -rf stage *.suo

installcheck-summary: sub-installcheck-summary
	$(MAKE) -C src $@

uninstall-local: sub-uninstall
	rm -rf $(DESTDIR)$(bindir)/.cstore

maintainer-clean-local: sub-maintainer-clean
	rm -f configure Makefile.in addressbook-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: package package-clean package-install package-uninstall

package package-clean: %: sub-%
	$(MAKE) -C package $@

package-install package-uninstall:
	$(MAKE) -C package $@
   
#
# Targets to use if building subprojects
#
if BUILD_SUBPROJECTS

all install installcheck: %: sub-%
	@echo "$@ sub done"

#distclean-local: sub-distclean
distclean: sub-distclean
	@echo "$@ sub done"

# for all, install subprojects
sub-all:
	@echo SUBPROJECT_BUILD_PATHS=$(SUBPROJECT_BUILD_PATHS)
	for subproj in $(SUBPROJECT_BUILD_PATHS); do \
		cd $$subproj; $(MAKE) install; \
	done

# pass these targets on to subprojects (stripping off sub-)
sub-clean sub-install sub-installcheck sub-installcheck-summary sub-uninstall sub-distclean sub-maintainer-clean sub-package-clean:
	@echo SUBPROJECT_BUILD_PATHS=$(SUBPROJECT_BUILD_PATHS)
	for subproj in $(SUBPROJECT_BUILD_PATHS); do \
		cd $$subproj; $(MAKE) $(subst sub-,,$@); \
	done

# for package, install subprojects then build subproject packages
sub-package:
	@echo SUBPROJECT_BUILD_PATHS=$(SUBPROJECT_BUILD_PATHS)
	for subproj in $(SUBPROJECT_BUILD_PATHS); do \
		cd $$subproj; $(MAKE) install package; \
	done

#
# Targets to use if not building subprojects
#
else

sub-clean:
sub-installcheck-summary:
sub-uninstall:
sub-maintainer-clean:
sub-package:
sub-package-clean:

endif

