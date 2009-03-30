EXTRA_DIST = en cs de es fr hu it ja pl pt-BR ru sk zh-CN zh-TW

if WINDOWS

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(prefix)/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(prefix)/help
	find $(DESTDIR)$(prefix)/help -name ".svn" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(prefix)/help; rm -rf $(EXTRA_DIST)

endif

if DARWIN 

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder3/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(datadir)/ifolder3/help
	find $(DESTDIR)$(datadir)/ifolder3/help -name ".svn" | xargs rm -rf
	ln -fs $(DESTDIR)$(datadir)/ifolder3/help/zh-CN $(DESTDIR)$(datadir)/ifolder3/help/zh-Hans
	ln -fs  $(DESTDIR)$(datadir)/ifolder3/help/zh-TW $(DESTDIR)$(datadir)/ifolder3/help/zh-Hant
	ln -fs  $(DESTDIR)$(datadir)/ifolder3/help/pt-BR $(DESTDIR)$(datadir)/ifolder3/help/pt_BR

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder3/help; rm -rf $(EXTRA_DIST)

endif

if LINUX

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder3/help
	cp -r $(EXTRA_DIST) $(DESTDIR)$(datadir)/ifolder3/help
	find $(DESTDIR)$(datadir)/ifolder3/help -name ".svn" | xargs rm -rf

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder3/help; rm -rf $(EXTRA_DIST)

endif

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

