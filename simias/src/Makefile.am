SUBDIRS = $(COMMON_SUBDIRS) $(PLATFORM_SUBDIRS)

DIST_SUBDIRS = $(COMMON_SUBDIRS) $(LINUX_SUBDIRS) $(WINDOWS_SUBDIRS)

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf stage *.suo

distclean-local:
	for subdir in $(NON_PLATFORM_SUBDIRS); do rm -rf "$$subdir/Makefile"; done

maintainer-clean-local:
	for subdir in $(NON_PLATFORM_SUBDIRS); do rm -rf "$$subdir/Makefile.in"; done
	rm -f Makefile.in

