SUBDIRS = $(COMMON_SUBDIRS) $(PLATFORM_SUBDIRS)

DIST_SUBDIRS = $(COMMON_SUBDIRS) $(LINUX_SUBDIRS) $(WINDOWS_SUBDIRS) package

.PHONY: package package-clean
package:
	$(MAKE) -C package all

package-clean package-install package-uninstall:
	$(MAKE) -C package $(subst package-,,$@)

clean-local:
	$(MAKE) -C package $@

maintainer-clean-local:
	rm -f Makefile.in
	$(MAKE) -C package $@

