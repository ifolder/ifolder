EXTRA_DIST = en

if WINDOWS

install-exec-local: 
	$(mkinstalldirs) $(DESTDIR)$(prefix)/help
	cp -r en $(DESTDIR)$(prefix)/help
	find $(DESTDIR)$(prefix)/help -name "CVS" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(prefix)/help; rm -rf en

else

install-exec-local: 
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder/help
	cp -r en $(DESTDIR)$(datadir)/ifolder/help
	find $(DESTDIR)$(datadir)/ifolder/help -name "CVS" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder/help; rm -rf en

endif

installcheck-summary:

clean-local:
    
distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

