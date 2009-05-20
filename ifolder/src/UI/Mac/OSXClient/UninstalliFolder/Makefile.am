#Make file for Un-installing iFolder

dist-hook:
	rm -rf `find $(distdir) -name CVS`

EXTRA_DIST = uninstall.icns 

if DEBUG
BUILDCONF=debug
else
BUILDCONF=release
endif

all: "$(srcdir)/build/$(BUILDCONF)/UninstalliFolder.app"


"$(srcdir)/build/$(BUILDCONF)/UninstalliFolder.app": 
	xcodebuild 

install-exec-local: "$(srcdir)/build/$(BUILDCONF)/UninstalliFolder.app"
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf "$(DESTDIR)$(prefix)/Applications/UninstalliFolder.app"
	cp -rf "$(srcdir)/build/$(BUILDCONF)/UninstalliFolder.app" $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf "UninstalliFolder.app"


clean-local:
	rm -rf build 
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

