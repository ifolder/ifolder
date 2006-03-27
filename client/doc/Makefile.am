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
#######################################################################
EXTRA_DIST = $(srcdir)/Doxyfile.in

all: Doxyfile clean
	doxygen Doxyfile

Doxyfile: Doxyfile.in
	sed -e "s/@_VERSION_@/$G$(VERSION)/" Doxyfile.in > Doxyfile
	
#install: $(RPM_FILE) 
#	sudo rpm -Uvh $(RPM_FILE) 
	
#uninstall:
#	sudo rpm -e $(PACKAGE) $(PACKAGE)-ui $(PACKAGE)-client

clean-local:
	rm -rf $(srcdir)/doxygen

distclean-local:
	rm -f Doxyfile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

