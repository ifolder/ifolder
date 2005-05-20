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
#  Author: Calvin Gaisford <cgaisford@novell.com>
#
#######################################################################
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/Description.plist $(srcdir)/Info.plist $(srcdir)/Install_Resources

PACKAGE_DIR = `pwd`
TMPDIR = $(PACKAGE_DIR)/temp

.PHONY: simias-dist simias-pkg

all: simias-dist simias-pkg

simias-dist:
	cd $(top_srcdir); make dist
	$(mkinstalldirs) $(TMPDIR)
	cd $(TMPDIR); rm -rf $(PACKAGE)-$(VERSION); 
	tar -C $(TMPDIR) -zxvf $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz
	cd $(TMPDIR)/$(PACKAGE)-$(VERSION);./configure --prefix=/opt/novell/ifolder-se
	cd $(TMPDIR)/$(PACKAGE)-$(VERSION);make;rm -rf $(TMPDIR)/Simias.dst;
	cd $(TMPDIR)/$(PACKAGE)-$(VERSION);make DESTDIR=$(TMPDIR)/Simias.dst install

simias-pkg:
	/Developer/Applications/Utilities/PackageMaker.app/Contents/MacOS/PackageMaker -build -p $(PACKAGE_DIR)/Simias.pkg -f $(TMPDIR)/$(PACKAGE)-$(VERSION)/temp/Simias.dst/opt/novell/ifolder-se/ -r $(PACKAGE_DIR)/Install_Resources -i $(PACKAGE_DIR)/Info.plist -d $(PACKAGE_DIR)/Description.plist

clean-local:
	rm -rf *.pkg
	rm -rf $(TMPDIR)

distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

