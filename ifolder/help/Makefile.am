EXTRA_DIST = en de es fr hu it ja pl pt ru sk zh-CN zh-TW

if WINDOWS

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(prefix)/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(prefix)/help
	find $(DESTDIR)$(prefix)/help -name "CVS" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(prefix)/help; rm -rf $(EXTRA_DIST)

else

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder3/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(datadir)/ifolder3/help
	find $(DESTDIR)$(datadir)/ifolder3/help -name "CVS" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder3/help; rm -rf $(EXTRA_DIST)

endif

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

