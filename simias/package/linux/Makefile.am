#######################################################################
#  $RCSfile$
#
#  Copyright (C) 2004 Novell, Inc.
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public
#  License as published by the Free Software Foundation; either
#  version 2 of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#  General Public License for more details.
#
#  You should have received a copy of the GNU General Public
#  License along with this program; if not, write to the Free
#  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#
#  Author: Paul Thomas <pthomas@novell.com>
#
#######################################################################
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias.spec.in

RELEASE = 1
RPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).$(target_cpu).rpm
SRPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).src.rpm

.PHONY: package package-clean package-install package-uninstall ifolder simias-dist

all clean:

package: simias.spec simias-dist
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

simias-dist:
	cd $(top_srcdir); make dist

package-install: package
	su -c "rpm -Uvh $(RPM_FILE)"
	
package-uninstall:
	su -c "rpm -e $(PACKAGE)"

package-clean clean-local:
	rm -rf *.rpm RPM

distclean-local: package-clean
	rm -f simias.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

