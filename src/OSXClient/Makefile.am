# handle Mono secondary dependencies
GSOAP_DIR=../../tools/gsoap/macosx-2.7

export MONO_PATH := $(MONO_PATH)

dist-hook:
	rm -rf `find $(distdir) -name CVS`

EXTRA_DIST = $(srcdir)/English.lproj $(srcdir)/Info.plist $(srcdir)/iFolder.xcode $(srcdir)/iFolder_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m $(srcdir)/version.plist $(srcdir)/src $(srcdir)/webservice $(srcdir)/notify


if DEBUG
BUILDSTYLE=Development
else
BUILDSTYLE=Deployment
endif


CUR_DIR := $(shell pwd)

all: $(srcdir)/build/iFolder.app


$(srcdir)/build/iFolder.app: iFolder.xcode
	xcodebuild -buildstyle $(BUILDSTYLE) HEADER_SEARCH_PATHS="../libifolder $(SIMIAS_CLIENT_C_INCLUDES)" LIBRARY_SEARCH_PATHS="../libifolder/.libs $(SIMIAS_CLIENT_C_LIBS)"

install-exec-local: $(srcdir)/build/iFolder.app
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf $(DESTDIR)$(prefix)/Applications/iFolder.app
	cp -rf $(srcdir)/build/iFolder.app $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf iFolder.app

CLEAN_FILES =

clean-local:
	rm -rf stubs build $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

