SUBDIRS = $(SIMIAS_OS)

DIST_SUBDIRS = linux windows

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C $(SIMIAS_OS) $@

maintainer-clean-local:
	rm -f Makefile.in
	

