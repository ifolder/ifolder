
install-exec-local: 
	$(mkinstalldirs) $(DESTDIR)$(helpdir)
	cp -r en $(DESTDIR)$(helpdir)
	find $(DESTDIR)$(helpdir) -name "CVS" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(helpdir); rm -rf en

installcheck-summary:

clean-local:
    
distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

