SUBDIRS = $(IFOLDER_OS)

DIST_SUBDIRS = linux windows

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C $(IFOLDER_OS) $@

# On Linux, distclean and maintainer-clean use DIST_SUBDIRS.
# On Windows, they use SUBDIRS.  These overloaded targets
# pass the target on to any directories in DIST_SUBDIRS that
# are not in SUBDIRS.
distclean maintainer-clean: package-clean
	for subdir in $(DIST_SUBDIRS); do \
	  test ! -f "$$subdir/Makefile" || $(MAKE) -C "$$subdir" $@; \
	done
	if test "$@" = maintainer-clean; then \
		rm -f Makefile.in; \
	fi

maintainer-clean:


