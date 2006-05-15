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
SUBDIRS = 
DIST_SUBDIRS = $(SUBDIRS)

INCLUDES = \
	-I$(top_srcdir)		\
	-I$(srcdir)/..		\
	-I$(top_builddir)

noinst_LIBRARIES = libifolder-trayapp.a

#libifolder_trayapp_a_LIBADD =	\
#	libsync/libsync.a		\
#	libsoap/libsoap.a		\
#	libipcserver/libipcserver.a	

DEFINES = 

libifolder_trayapp_a_CFLAGS = \
	$(DEFINES)					\
	$(GLIB_CFLAGS)

libifolder_trayapp_a_LDFLAGS = \
	$(GLIB_LIBS)

libifolder_trayapp_a_LIBFLAGS = 

libifolder_trayapp_a_SOURCES = \
	ifolder-client.c			\
	ifolder-domain.c

maintainer-clean-local:
	rm -f Makefile.in
