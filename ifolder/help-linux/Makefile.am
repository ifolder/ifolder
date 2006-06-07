EXTRA_DIST = en cs de es fr hu it ja pl pt ru sk zh-CN zh-TW

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder3/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(datadir)/ifolder3/help
	find $(DESTDIR)$(datadir)/ifolder3/help -name ".svn" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder3/help; rm -rf $(EXTRA_DIST)

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

