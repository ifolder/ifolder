# handle Mono secondary dependencies
GSOAP_DIR=../../tools/gsoap/macosx-2.7

export MONO_PATH := $(MONO_PATH)

EXTRA_DIST = $(srcdir)/English.lproj $(srcdir)/Info.plist $(srcdir)/iFolder.xcode $(srcdir)/iFolder_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m $(srcdir)/version.plist $(srcdir)/LoginWindowController.h $(srcdir)/LoginWindowController.m $(srcdir)/MainWindowController.h $(srcdir)/MainWindowController.m

CUR_DIR := $(shell pwd)

all: $(srcdir)/build/iFolder.app

$(srcdir)/stubs/iFolderClient.h: $(GSOAP_DIR)/wsdl2h $(srcdir)/../WebService/iFolderWebService.wsdl
	$(mkinstalldirs) stubs
	cd stubs; ../$(GSOAP_DIR)/wsdl2h -c -o iFolderClient.h ../../WebService/iFolderWebService.wsdl

$(srcdir)/stubs/iFolderClient.c: $(GSOAP_DIR)/soapcpp2 $(srcdir)/stubs/iFolderClient.h
	cd stubs; ../$(GSOAP_DIR)/soapcpp2 -c -n -piFolderClient iFolderClient.h
	cp $(GSOAP_DIR)/stdsoap2.c stubs
	cp $(GSOAP_DIR)/stdsoap2.h stubs


$(srcdir)/build/iFolder.app: iFolder.xcode $(srcdir)/stubs/iFolderClient.c
	xcodebuild -activebuildstyle

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

