SUBDIRS = \
	Common \
	ChangeLog \
	Configuration \
	CollectionStore \
	Smtp \
	SimiasTestsWS \
	SimiasTestsCmd
	
DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST =

.PHONY: test
test:
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.exe $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.config $(DESTDIR)$(webbindir)
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

