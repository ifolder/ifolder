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
	-I$(top_srcdir)/src/core \
	-I$(top_builddir)

lib_LTLIBRARIES = libifolder3.la

#libifolder_trayapp_a_LIBADD =	\
#	libsync/libsync.a		\
#	libsoap/libsoap.a		\
#	libipcserver/libipcserver.a	

DEFINES = 

libifolder3_la_CXXFLAGS = \
	$(DEFINES)					\
	$(GLIB_CFLAGS)

libifolder3_la_LDFLAGS = \
	$(GLIB_LIBS)

#libifolder_trayapp_a_LIBDADD = \
#	$(top_srcdir)/src/core/libifolder-core.a

libifolder3_la_LIBFLAGS = 

libifolder3_la_SOURCES = \
	ifolder-client.cpp			\
	ifolder-domain.cpp			\
	ifolder.cpp					\
	ifolder-user-prefs.cpp

maintainer-clean-local:
	rm -f Makefile.in
