# Makefile.in generated automatically by automake 1.4-p6 from Makefile.am

# Copyright (C) 1994, 1995-8, 1999, 2001 Free Software Foundation, Inc.
# This Makefile.in is free software; the Free Software Foundation
# gives unlimited permission to copy and/or distribute it,
# with or without modifications, as long as this notice is preserved.

# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY, to the extent permitted by law; without
# even the implied warranty of MERCHANTABILITY or FITNESS FOR A
# PARTICULAR PURPOSE.


SHELL = /bin/bash

srcdir = .
top_srcdir = ../..

prefix = /cygdrive/c/wa/ifolder/stage
exec_prefix = ${prefix}

bindir = ${exec_prefix}/bin
sbindir = ${exec_prefix}/sbin
libexecdir = ${exec_prefix}/libexec
datadir = ${prefix}/share
sysconfdir = ${prefix}/etc
sharedstatedir = ${prefix}/com
localstatedir = ${prefix}/var
libdir = ${exec_prefix}/lib
infodir = ${prefix}/info
mandir = ${prefix}/man
includedir = ${prefix}/include
oldincludedir = /usr/include

DESTDIR =

pkgdatadir = $(datadir)/simias
pkglibdir = $(libdir)/simias
pkgincludedir = $(includedir)/simias

top_builddir = ../..

ACLOCAL = aclocal-1.4
AUTOCONF = autoconf
AUTOMAKE = automake-1.4
AUTOHEADER = autoheader

INSTALL = /usr/bin/install -c
INSTALL_PROGRAM = ${INSTALL} $(AM_INSTALL_PROGRAM_FLAGS)
INSTALL_DATA = ${INSTALL} -m 644
INSTALL_SCRIPT = ${INSTALL}
transform = s,x,x,

NORMAL_INSTALL = :
PRE_INSTALL = :
POST_INSTALL = :
NORMAL_UNINSTALL = :
PRE_UNINSTALL = :
POST_UNINSTALL = :
build_alias = 
build_triplet = i686-pc-cygwin
host_alias = 
host_triplet = i686-pc-cygwin
target_alias = 
target_triplet = i686-pc-cygwin
APIDOCDIR = $(top_srcdir)/src/api-doc
CC = cl -c
CDEF = -D
CINC = -I
COMMON_CLEAN_FILES = *.suo */*.suo *.csproj.user */*.csproj.user bin obj */bin */obj *.xml */*.xml .cstore
COMMON_SUBDIRS = Configuration Common CollectionEvents UserService StoreProvider FsProvider SqliteProvider CollectionStore FsWatcher Agent SyncCommon StoreWatcher CollectionWatcher SyncManager Sync SyncLogicLite SyncTools AddressBook Smtp InviteAgent MiniFolder
COUT = -Fo
CSC = csc
CSCFLAGS = /debug+ /d:DEBUG /d:DOTNET /warn:4 /d:TRACE /nologo
CSC_EXEFLAG = /target:exe
CSC_LIBFLAG = /target:library
CSC_WINEXEFLAG = /target:winexe
CXX = cl -c
CXXFLAGS = -MDd -Od -Zi -RTC1 -DDEBUG -D_DEBUG -nologo -GX -W3 -DWIN32 -D_WINDOWS -DWINDOWS -DPACKAGE=$(PACKAGE) -DVERSION=$(VERSION)
CYGWIN_PATH = c:/cygwin
DEF = -def:
DEVENV_CONFIGURATION = Debug
DOCDIR = $(top_srcdir)/src/doc
EMPTY = 
ICON_EXT = .ico
ICON_FLAG = /win32icon:
LD = link
LDFLAGS = -nologo -machine:X86
LDINC = -libpath:
LDOUT = -out:
LIBS = kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib rpcrt4.lib ws2_32.lib imagehlp.lib
LINUX_SUBDIRS = CommonGtkWindows
MAINT = 
MAKEINFO = makeinfo
MONO = 
NON_PLATFORM_SUBDIRS = CommonGtkWindows
NUNIT = $(top_srcdir)/tools/NUnit/bin/nunit-console.exe
NUNIT_FLAGS =  /nologo
NUNIT_LIBPATH = $(top_srcdir)/tools/NUnit/bin
OBJ_EXT = .obj
PACKAGE = simias
PKG_CONFIG = /usr/bin/pkg-config
PLATFORM_SUBDIRS = CommonFormWindows
RC = rc
RCFLAGS = 
RES_EXT = .res
ROOTDIR = 
SEP = $(EMPTY)\\$(EMPTY)
SHARED_LIB_FLAG = -dll
SIMIAS_OS = windows
SQLITELIB = sqlite.dll
SQLITELIB_PATH = ../../external/sqlite/w32
SRCDIR = $(top_srcdir)/src
SYSTEM_XML = System.XML.dll
TLBX = tlbexp
TOOLDIR = $(top_srcdir)/tools
VERSION = 0.5
WINDOWS_SUBDIRS = CommonFormWindows

MiniFolder_CSFILES = $(srcdir)/Application/AssemblyInfo.cs $(srcdir)/Application/MiniFolder.cs
MiniFolder_CSFILES_CSC := $(subst /,$(SEP),$(MiniFolder_CSFILES))
MiniFolder_INCLUDES = 
MiniFolder_RESOURCES = 
MiniFolder_FLAGS = $(CSC_EXEFLAG)
MiniFolder_LIBS = System.dll $(SYSTEM_XML) Common.dll CollectionStore.dll StoreProvider.dll SyncManager.dll CollectionWatcher.dll SyncCommon.dll SyncLogicLite.dll Sync.dll
MiniFolder_LIBPATH = ../CollectionStore  ../../external/SecureChannelSinks ../StoreProvider ../StoreWatcher ../CollectionWatcher ../Agent ../AddressBook ../Common ../SyncCommon ../SyncManager ../SyncLogicLite ../Sync ../Configuration ../Common ../CollectionEvents
NUNIT_TESTS := 
EXTRA_DIST = $(MiniFolder_CSFILES)
CUR_DIR := $(shell pwd)
CLEAN_FILES = MiniFolder.exe $(NUNIT_TESTS) MiniFolder.xml
mkinstalldirs = $(SHELL) $(top_srcdir)/mkinstalldirs
CONFIG_CLEAN_FILES = 
DIST_COMMON =  Makefile.am Makefile.in


DISTFILES = $(DIST_COMMON) $(SOURCES) $(HEADERS) $(TEXINFOS) $(EXTRA_DIST)

TAR = tar
GZIP_ENV = --best
all: all-redirect
.SUFFIXES:
$(srcdir)/Makefile.in:  Makefile.am $(top_srcdir)/configure.in $(ACLOCAL_M4) 
	cd $(top_srcdir) && $(AUTOMAKE) --gnu src/MiniFolder/Makefile

Makefile: $(srcdir)/Makefile.in  $(top_builddir)/config.status $(BUILT_SOURCES)
	cd $(top_builddir) \
	  && CONFIG_FILES=$(subdir)/$@ CONFIG_HEADERS= $(SHELL) ./config.status

tags: TAGS
TAGS:


distdir = $(top_builddir)/$(PACKAGE)-$(VERSION)/$(subdir)

subdir = src/MiniFolder

distdir: $(DISTFILES)
	here=`cd $(top_builddir) && pwd`; \
	top_distdir=`cd $(top_distdir) && pwd`; \
	distdir=`cd $(distdir) && pwd`; \
	cd $(top_srcdir) \
	  && $(AUTOMAKE) --include-deps --build-dir=$$here --srcdir-name=$(top_srcdir) --output-dir=$$top_distdir --gnu src/MiniFolder/Makefile
	$(mkinstalldirs) $(distdir)/$(srcdir)/Application
	@for file in $(DISTFILES); do \
	  d=$(srcdir); \
	  if test -d $$d/$$file; then \
	    cp -pr $$d/$$file $(distdir)/$$file; \
	  else \
	    test -f $(distdir)/$$file \
	    || ln $$d/$$file $(distdir)/$$file 2> /dev/null \
	    || cp -p $$d/$$file $(distdir)/$$file || :; \
	  fi; \
	done
info-am:
info: info-am
dvi-am:
dvi: dvi-am
check-am: all-am
check: check-am
installcheck-am: installcheck-local
installcheck: installcheck-am
install-exec-am: install-exec-local
install-exec: install-exec-am

install-data-am:
install-data: install-data-am

install-am: all-am
	@$(MAKE) $(AM_MAKEFLAGS) install-exec-am install-data-am
install: install-am
uninstall-am: uninstall-local
uninstall: uninstall-am
all-am: Makefile
all-redirect: all-am
install-strip:
	$(MAKE) $(AM_MAKEFLAGS) AM_INSTALL_PROGRAM_FLAGS=-s install
installdirs:


mostlyclean-generic:

clean-generic:

distclean-generic:
	-rm -f Makefile $(CONFIG_CLEAN_FILES)
	-rm -f config.cache config.log stamp-h stamp-h[0-9]*

maintainer-clean-generic:
mostlyclean-am:  mostlyclean-generic

mostlyclean: mostlyclean-am

clean-am:  clean-generic mostlyclean-am clean-local

clean: clean-am

distclean-am:  distclean-generic clean-am distclean-local

distclean: distclean-am

maintainer-clean-am:  maintainer-clean-generic distclean-am \
		maintainer-clean-local
	@echo "This command is intended for maintainers to use;"
	@echo "it deletes files that may require special tools to rebuild."

maintainer-clean: maintainer-clean-am

.PHONY: tags distdir info-am info dvi-am dvi check check-am \
installcheck-local installcheck-am installcheck install-exec-local \
install-exec-am install-exec install-data-am install-data install-am \
install uninstall-local uninstall-am uninstall all-redirect all-am all \
installdirs mostlyclean-generic distclean-generic clean-generic \
maintainer-clean-generic clean mostlyclean distclean maintainer-clean





all: MiniFolder.exe

MiniFolder.exe: $(MiniFolder_CSFILES) $(MiniFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(MiniFolder_FLAGS) $(MiniFolder_LIBS:%=/r:%) $(MiniFolder_LIBPATH:%=/lib:%) $(MiniFolder_CSFILES_CSC) /doc:MiniFolder.xml

install-exec-local: MiniFolder.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) MiniFolder.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f MiniFolder.exe $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ConfTests.xml $(NUNIT_TESTS)
	

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in


# Tell versions [3.59,3.63) of GNU make to not export all variables.
# Otherwise a system limit (for SysV at least) may be exceeded.
.NOEXPORT:
