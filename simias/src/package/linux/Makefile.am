EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias.spec.in

RELEASE = 1
RPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).$(target_cpu).rpm
SRPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).src.rpm

all: $(RPM_FILE) $(SRPM_FILE)
 
$(RPM_FILE) $(SRPM_FILE): simias.spec $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz
	rm -rf RPM
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp simias.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba --target=$(target_triplet) RPM/SPECS/simias.spec
	cp RPM/RPMS/*/$(RPM_FILE) .
	cp RPM/SRPMS/$(SRPM_FILE) .

$(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz:
	cd $(top_srcdir); make dist

install: $(RPM_FILE)
	su -c "rpm -Uvh $(RPM_FILE)"
	
uninstall:
	su -c "rpm -e $(PACKAGE)"

clean-local:
	rm -rf *.rpm RPM

maintainer-clean-local:
	rm -f Makefile.in

