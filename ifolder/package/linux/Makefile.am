EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder.spec.in

RELEASE = 1
RPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).$(target_cpu).rpm
SRPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).src.rpm

.PHONY: package package-clean package-install package-uninstall ifolder

all clean:

package: $(RPM_FILE) $(SRPM_FILE)
 
$(RPM_FILE) $(SRPM_FILE): ifolder.spec $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz
	rm -rf RPM
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp ifolder.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba --target=$(target_triplet) RPM/SPECS/ifolder.spec
	cp RPM/RPMS/*/$(RPM_FILE) .
	cp RPM/SRPMS/$(SRPM_FILE) .

$(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz:
	cd $(top_srcdir); make dist

package-install: $(RPM_FILE)
	su -c "rpm -Uvh $(RPM_FILE)"
	
package-uninstall:
	su -c "rpm -e $(PACKAGE)"

package-clean clean-local:
	rm -rf *.rpm RPM

distclean-local:
	rm -f ifolder.spec

maintainer-clean-local:
	rm -f Makefile.in

