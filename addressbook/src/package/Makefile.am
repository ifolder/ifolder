SUBDIRS = $(ADDRESSBOOK_OS)

DIST_SUBDIRS = linux windows

EXTRA_DIST = $(srcdir)/Makefile.am

all:
	$(MAKE) -C $(ADDRESSBOOK_OS) $@

clean-local:
	$(MAKE) -C $(ADDRESSBOOK_OS) $@

maintainer-clean-local:
	rm -f Makefile.in Makefile
	$(MAKE) -C $(ADDRESSBOOK_OS) $@
	

