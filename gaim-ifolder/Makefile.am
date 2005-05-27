SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS) package

if WINDOWS
if DEBUG	
ADDITIONAL_LIB_FILES = msvcr71d.dll
MSVCR_DEBUG_DLL_PATH = $(shell cygpath --unix "$(FRAMEWORKDIR)/v1.1*")
MSVCR_DEBUG_DLL = $(shell find $(MSVCR_DEBUG_DLL_PATH) -name msvcr71d.dll)
endif
endif

EXTRA_DIST = 

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

if WINDOWS
if DEBUG	
msvcr71d.dll:	$(MSVCR_DEBUG_DLL)
	cp -f $< $@
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

.PHONY: package package-clean

package:
	$(MAKE) -C package all

package-clean:
	$(MAKE) -C package clean