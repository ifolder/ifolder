SUBDIRS = src help po

DIST_SUBDIRS = $(SUBDIRS) doc package test

if WINDOWS
if DEBUG	
ADDITIONAL_LIB_FILES = msvcr71d.dll
MSVCR_DEBUG_DLL_PATH = $(shell cygpath --unix "$(FRAMEWORKDIR)/v1.1*")
MSVCR_DEBUG_DLL = $(shell find $(MSVCR_DEBUG_DLL_PATH) -name msvcr71d.dll)
endif
endif

EXTRA_DIST = tools $(srcdir)/$(PACKAGE)*.pc.in $(ADDITIONAL_LIB_FILES)

ifolder3.pc: ifolder3.pc.in
	sed -e 's,@_VERSION_@,@VERSION@,g' -e 's,@_PKG_CONFIG_REQUIRES_@,@PKG_CONFIG_REQUIRES@,g' -e 's,@_PKG_CONFIG_CONFLICTS_@,@PKG_CONFIG_CONFLICTS@,g' -e 's,@_prefix_@,@prefix@,g' -e 's,@_exec_prefix_@,@exec_prefix@,g' -e 's,@_libdir_@,@libdir@,g' -e 's,@_PKG_CONFIG_LIBS_@,@PKG_CONFIG_LIBS@,g' $< > $@
	chmod +x $@
	
ifolder3-c.pc: ifolder3-c.pc.in
	sed -e 's,@_VERSION_@,@VERSION@,g' -e 's,@_PKG_CONFIG_CONFLICTS_@,@PKG_CONFIG_CONFLICTS@,g' -e 's,@_prefix_@,@prefix@,g' -e 's,@_exec_prefix_@,@exec_prefix@,g' -e 's,@_libdir_@,@libdir@,g' $< > $@
	chmod +x $@

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

if WINDOWS
if DEBUG	
msvcr71d.dll:	$(MSVCR_DEBUG_DLL)
	cp -f $< $@
endif
endif

install-exec-local: $(ADDITIONAL_LIB_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig $(DESTDIR)$(datadir)/$(PACKAGE)
	$(INSTALL_PROGRAM) $(srcdir)/COPYING $(DESTDIR)$(datadir)/$(PACKAGE)
	$(INSTALL_PROGRAM) $(PACKAGE)*.pc $(DESTDIR)$(libdir)/pkgconfig
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(ADDITIONAL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(ADDITIONAL_LIB_FILES) $(DESTDIR)$(webbindir)
endif
endif

register unregister:
if WINDOWS
	$(MAKE) -C src $@
endif

uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f $(PACKAGE)*.pc
	cd $(DESTDIR)$(datadir)/$(PACKAGE); rm -f COPYING
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(libdir); rm -f $(ADDITIONAL_LIB_FILES)
	cd $(DESTDIR)$(webbindir); rm -f $(ADDITIONAL_LIB_FILES)
endif
endif

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure $(PACKAGE)-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status config.h.in
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

doc-nodeps:
	$(MAKE) -C doc all-nodeps

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
