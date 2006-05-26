#######################################################################
#  $RCSfile$
#
#  Copyright (C) 2006 Novell, Inc.
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

#LPROJDIRS = es.lproj
LPROJDIRS =

EXTRA_DIST = $(LPROJDIRS) $(srcdir)/English.lproj $(srcdir)/Info.plist.in $(srcdir)/iFolder3.xcodeproj $(srcdir)/iFolder3_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m

CUR_DIR := $(shell pwd)

all: "$(srcdir)/build/$(DEVENV_CONFIGURATION)/iFolder3.app"

"$(srcdir)/build/$(DEVENV_CONFIGURATION)/iFolder3.app": iFolder3.xcodeproj
	xcodebuild -configuration $(DEVENV_CONFIGURATION)

install-exec-local: "$(srcdir)/build/$(DEVENV_CONFIGURATION)/iFolder3.app"
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf "$(DESTDIR)$(prefix)/Applications/iFolder3.app"
	cp -rf "$(srcdir)/build/$(DEVENV_CONFIGURATION)/iFolder3.app" $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf "iFolder3.app"

CLEAN_FILES =

clean-local:
	rm -rf build $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

generate-strings:
	genstrings -q -o $(srcdir)/English.lproj `cat intlsrcfiles`
	nibfiles=`cat intlnibfiles`; \
	for nibfile in $$nibfiles; do \
		nibtool -L $$nibfile > $$nibfile.strings; \
	done

clean-strings:
	nibfiles=`cat intlnibfiles`; \
	for nibfile in $$nibfiles; do \
		rm -f $$nibfile.strings; \
	done
	lprojdirs='$(LPROJDIRS)'; \
	for lprojdir in $$lprojdirs; do \
		nibfiles=`cat intlnibfiles`; \
		for nibfile in $$nibfiles; do \
			nibfile=$$lprojdir/`basename $$nibfile`; \
			rm -f $$nibfile.strings; \
		done; \
	done

update-strings:
	lprojdirs='$(LPROJDIRS)'; \
	for lprojdir in $$lprojdirs; do \
		nibfiles=`cat intlnibfiles`; \
		for nibfile in $$nibfiles; do \
			nibfile=$$lprojdir/`basename $$nibfile`; \
			mv $$nibfile/CVS $$nibfile.CVS; \
			nibtool -r -v -d $$nibfile.strings -I $$nibfile $$nibfile; \
			mv $$nibfile.CVS $$nibfile/CVS; \
		done; \
	done

