SUBDIRS = $(COMMON_SUBDIRS) $(PLATFORM_SUBDIRS)

DIST_SUBDIRS = $(COMMON_SUBDIRS) $(LINUX_SUBDIRS) $(WINDOWS_SUBDIRS) $(NOBUILD_SUBDIRS)

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf stage *.suo

maintainer-clean-local:
	rm -f Makefile.in

