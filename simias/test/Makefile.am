SUBDIRS = Simias Storage

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST =

.PHONY: test
test:
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_LIBPATH)/*.dll $(NUNIT_LIBPATH)/*.exe $(DESTDIR)$(libdir)
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

