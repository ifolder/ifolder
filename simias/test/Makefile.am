SUBDIRS = \
	Common \
	Configuration \
	CollectionStore \
	Smtp
	
DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST =

.PHONY: test
test:
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.dll $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.config $(DESTDIR)$(bindir)
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

