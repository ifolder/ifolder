EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/nautilus-ifolder3.spec

.PHONY: package-clean

all: nautilus-ifolder3.spec nautilus-ifolder-dist
	rm -rf RPM
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp nautilus-ifolder3.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba RPM/SPECS/nautilus-ifolder3.spec
	
nautilus-ifolder-dist:
	cd $(top_srcdir); make dist

install: package
	sudo rpm -Uvh $(RPM_FILE)
	
uninstall:
	sudo rpm -e $(PACKAGE)

clean-local:
	rm -rf *.rpm RPM

distclean-local: package-clean

maintainer-clean-local:
	rm -rf Makefile Makefile.in RPM
