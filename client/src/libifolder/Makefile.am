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
SUBDIRS = libifsoap

DIST_SUBDIRS = $(SUBDIRS)

ifolder_internal_headers =

ifolder_internal_sources =				\
	internal/account-internal.c			\
	internal/change-entry-internal.c	\
	internal/enumeration-internal.c		\
	internal/ifolder-internal.c			\
	internal/user-internal.c

EXTRA_DIST =							\
	$(ifolder_internal_headers)			\
	$(ifolder_internal_sources)

INCLUDES =					\
	-I$(top_srcdir)			\
	-I$(top_builddir)		\
	-I$(srcdir)/internal	\
	-I$(srcdir)/libifsoap

lib_LTLIBRARIES = libifolder.la

ifolder_api_headers =	\
	account.h			\
	change-entry.h		\
	enumeration.h		\
	errors.h			\
	events.h			\
	ifolder-client.h	\
	ifolder.h			\
	prefs.h				\
	user.h

ifolder_api_sources =			\
	account.c			\
	change-entry.c			\
	enumeration.c			\
	events.c			\
	ifolder-client.c		\
	ifolder.c			\
	prefs.c				\
	user.c

ifolder_soap_headers =

ifolder_soap_sources =					\
	libifsoap/envC.c					\
	libifsoap/iFolderClientLib.c		\
	libifsoap/stdsoap2.c

include_HEADERS =						\
	$(srcdir)/ifolder-client-private.h	\
	$(ifolder_api_headers)				\
	$(ifolder_internal_headers)			\
	$(ifolder_soap_headers)

libifolder_la_SOURCES =					\
	$(ifolder_api_sources)				\
	$(ifolder_internal_sources)			\
	$(ifolder_soap_sources)

if DARWIN
libifolder_la_CFLAGS =					\
	-DDARWIN							\
	-DWITH_NONAMESPACES					\
	-DWITH_COOKIES
endif

if !DARWIN
libifolder_la_CFLAGS =					\
	-DWITH_NONAMESPACES					\
	-DWITH_COOKIES
endif

#if !WINDOWS
#install-exec-hook:
#	rm -f $(DESTDIR)$(libdir)/libifolder.a
#	rm -f $(DESTDIR)$(libdir)/libifolder.la
#fi

#sdk:	
#	$(MAKE) -C $(IFOLDER_OS) sdk

maintainer-clean-local:
	rm -f Makefile.in

