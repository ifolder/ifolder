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
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder-nautilus.spec

.PHONY: package package-clean package-install package-uninstall ifolder-dist

all clean:

package: ifolder-nautilus.spec ifolder-nautilus-dist
	rm -rf RPM
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp ifolder-nautilus.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba RPM/SPECS/ifolder-nautilus.spec
	
ifolder-nautilus-dist:
	cd $(top_srcdir); make dist

package-install: package
	su -c "rpm -Uvh $(RPM_FILE)"
	
package-uninstall:
	su -c "rpm -e $(PACKAGE)"

clean-local:
	rm -rf *.rpm RPM

distclean-local: package-clean

maintainer-clean-local:
	rm -rf Makefile Makefile.in RPM

