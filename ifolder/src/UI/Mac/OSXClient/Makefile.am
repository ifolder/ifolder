# handle Mono secondary dependencies
#GSOAP_DIR=../../tools/gsoap/macosx-2.7
SUBDIRS=MacKeyRecovery UninstalliFolder
DIST_SUBDIRS=$(SUBDIRS)

GSOAP_DIR=$(GSOAP_PATH)

export MONO_PATH := $(MONO_PATH)

dist-hook:
	rm -rf `find $(distdir) -name CVS`

LPROJDIRS = es.lproj

EXTRA_DIST = $(LPROJDIRS) $(srcdir)/English.lproj $(srcdir)/Info.plist.in $(srcdir)/iFolder.xcodeproj $(srcdir)/iFolder_Prefix.pch $(srcdir)/ifolder.icns $(srcdir)/main.m $(srcdir)/version.plist $(srcdir)/src $(srcdir)/webservice $(srcdir)/notify


if DEBUG
BUILDCONF=debug
else
BUILDCONF=release
endif


CUR_DIR := $(shell pwd)

all: "$(srcdir)/build/$(BUILDCONF)/iFolder 3.app"


"$(srcdir)/build/$(BUILDCONF)/iFolder 3.app": iFolder.xcodeproj
	xcodebuild -configuration $(BUILDCONF) HEADER_SEARCH_PATHS="../../../common/libifolder $(SIMIAS_CLIENT_C_INCLUDES)" LIBRARY_SEARCH_PATHS="../../../common/libifolder/.libs $(SIMIAS_CLIENT_C_LIBS)" OTHER_LDFLAGS="-bind_at_load"

install-exec-local: "$(srcdir)/build/$(BUILDCONF)/iFolder 3.app"
	$(mkinstalldirs) $(DESTDIR)$(prefix)/Applications
	rm -rf "$(DESTDIR)$(prefix)/Applications/iFolder 3.app"
	cp -rf "$(srcdir)/build/$(BUILDCONF)/iFolder 3.app" $(DESTDIR)$(prefix)/Applications

uninstall-local:
	cd $(DESTDIR)$(prefix)/Applications; rm -rf "iFolder 3.app"

CLEAN_FILES =

clean-local:
	rm -rf stubs build $(COMMON_CLEAN_FILES)
    
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

