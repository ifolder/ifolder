SUBDIRS = $(SIMIAS_OS)

DIST_SUBDIRS = linux windows

EXTRA_DIST = $(srcdir)/Makefile.am

all:
	$(MAKE) -C $(SIMIAS_OS) $@

clean-local:
	$(MAKE) -C $(SIMIAS_OS) $@

maintainer-clean-local:
	rm -f Makefile.in Makefile
	$(MAKE) -C $(SIMIAS_OS) $@
	

