SUBDIRS = Simias.dll mDnsService SimiasTests.dll

DIST_SUBDIRS = Simias.dll mDnsService SimiasTests.dll

EXTRA_DIST = simias-snakeoil.keys

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

