SUBDIR = $(SIMIAS_OS)

DIST_SUBDIR = linux windows

all:
	$(MAKE) -C $(SIMIAS_OS) $@

clean-local:
	$(MAKE) -C $(SIMIAS_OS) $@

maintainer-clean-local:
	rm -f Makefile.in
	$(MAKE) -C $(SIMIAS_OS) $@
	

