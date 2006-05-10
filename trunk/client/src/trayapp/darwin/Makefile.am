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

EXTRA_DIST = $(LPROJDIRS) $(srcdir)/English.lproj $(srcdir)/Info.plist.in $(srcdir)/iFolderStatusIcon.xcodeproj $(srcdir)/iFolderStatusIcon_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m

if DEBUG
BUILDCONF=debug
else
BUILDCONF=release
endif

CUR_DIR := $(shell pwd)

all: "$(srcdir)/build/$(BUILDCONF)/iFolderStatusIcon.app"

"$(srcdir)/build/$(BUILDCONF)/iFolderStatusIcon.app": iFolderStatusIcon.xcodeproj
	xcodebuild -configuration $(BUILDCONF)

install-exec-local: "$(srcdir)/build/$(BUILDCONF)/iFolderStatusIcon.app"
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf "$(DESTDIR)$(prefix)/Applications/iFolderStatusIcon.app"
	cp -rf "$(srcdir)/build/$(BUILDCONF)/iFolderStatusIcon.app" $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf "iFolderStatusIcon.app"

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

