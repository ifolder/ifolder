SUBDIRS = $(IFOLDER_OS)

DIST_SUBDIRS = linux windows

EXTRA_DIST = $(srcdir)/Makefile.am

all:
	$(MAKE) -C $(IFOLDER_OS) $@

clean-local:
	$(MAKE) -C $(IFOLDER_OS) $@

maintainer-clean-local:
	rm -f Makefile.in Makefile
	$(MAKE) -C $(IFOLDER_OS) $@
	

