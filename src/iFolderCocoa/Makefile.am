# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

EXTRA_DIST = $(srcdir)/English.lproj $(srcdir)/Info.plist $(srcdir)/iFolder.xcode $(srcdir)/iFolder_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m $(srcdir)/version.plist $(srcdir)/LoginWindowController.h $(srcdir)/LoginWindowController.m $(srcdir)/MainWindowController.h $(srcdir)/MainWindowController.m

CUR_DIR := $(shell pwd)

all: $(srcdir)/build/iFolder.app

$(srcdir)/stubs/iFolder.m: $(srcdir)/../WebService/iFolderWebService.wsdl
	mkdir stubs
	/Developer/Tools/WSMakeStubs -x ObjC -dir ./stubs -name iFolder -file $(srcdir)/../WebService/iFolderWebService.wsdl

$(srcdir)/build/iFolder.app: iFolder.xcode $(srcdir)/stubs/iFolder.m
	xcodebuild

install-exec-local: $(srcdir)/build/iFolder.app
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf $(DESTDIR)$(prefix)/Applications/iFolder.app
	cp -rf $(srcdir)/build/iFolder.app $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf iFolder.app

CLEAN_FILES =

clean-local:
	rm -rf build stubs $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

