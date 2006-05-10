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
#  Author: Boyd Timothy <btimothy@novell.com>
#
#######################################################################
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/gaim-ifolder.spec

.PHONY: package-clean

all: gaim-ifolder.spec gaim-ifolder-dist
	rm -rf RPM
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp gaim-ifolder.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba RPM/SPECS/gaim-ifolder.spec
	
gaim-ifolder-dist:
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
