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
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/gnome-vfs-ifolder.spec.in

RELEASE = 1
RPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).$(target_cpu).rpm
SRPM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).src.rpm

.PHONY: package package-clean package-install package-uninstall

all: gnome-vfs-ifolder.spec gnome-vfs-ifolder-dist clean
	mkdir RPM
	echo %_topdir `pwd`/RPM > $(HOME)/.rpmmacros
	mkdir -p RPM/BUILD
	mkdir -p RPM/RPMS
	mkdir -p RPM/SOURCES
	mkdir -p RPM/SPECS
	mkdir -p RPM/SRPMS
	cp gnome-vfs-ifolder.spec RPM/SPECS
	cp $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz RPM/SOURCES
	rpmbuild -ba --target=$(target_triplet) RPM/SPECS/gnome-vfs-ifolder.spec
	cp RPM/RPMS/*/$(RPM_FILE) .
	cp RPM/SRPMS/$(SRPM_FILE) .
	@test "x" = "x$(INSTALL_PKGS)" || \
		CMD='cp -f $(RPM_FILE) $(SRPM_FILE) "$(INSTALL_PKGS)"'; \
		echo $$CMD; \
		eval $$CMD

gnome-vfs-ifolder-dist:
	cd $(top_srcdir); make dist
	@test "x" = "x$(SOURCE_TARBALLS)" || \
		CMD='cp -f $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz "$(SOURCE_TARBALLS)"'; \
		echo $$CMD; \
		eval $$CMD

install: $(RPM_FILE) $(UI_RPM_FILE)
	sudo rpm -Uvh $(RPM_FILE)
	
uninstall:
	sudo rpm -e $(PACKAGE)

clean-local:
	rm -rf *.rpm RPM

distclean-local:
	rm -f gnome-vfs-ifolder.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

