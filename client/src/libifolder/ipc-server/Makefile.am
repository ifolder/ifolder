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
SUBDIRS = libsync libsoap libipcserver

DIST_SUBDIRS = $(SUBDIRS)

INCLUDES =			\
	-I$(top_srcdir)		\
	-I$(top_builddir)

lib_LTLIBRARIES = libifolder-trayapp.la

libifolder_trayapp_LDFLAGS =	\
	$(srcdir)/libsync/.libs/libsync.a		\
	$(srcdir)/libsoap/.libs/libsoap.a		\
	$(srcdir)/libipcserver/.libs/libipcserver.a

#libifolder_trayapp_SOURCES =

#libifolder_trayapp_HEADERS = \

#if !WINDOWS
#install-exec-hook:
#	rm -f $(DESTDIR)$(libdir)/libifolder.a
#	rm -f $(DESTDIR)$(libdir)/libifolder.la
#fi

#sdk:	
#	$(MAKE) -C $(IFOLDER_OS) sdk

maintainer-clean-local:
	rm -f Makefile.in

