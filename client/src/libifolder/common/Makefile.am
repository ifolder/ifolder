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

lib_LIBRARIES = libifolder-common.a

#libifolder_trayapp_a_LIBADD =	\
#	libsync/libsync.a		\
#	libsoap/libsoap.a		\
#	libipcserver/libipcserver.a	

DEFINES = \
	$(QT4_DEFINES)				\
	-DQT_NETWORK					\
	-DQT_CORE_LIB				\
	-DQT_SHARED

libifolder_common_a_CXXFLAGS = \
	$(DEFINES)					\
	$(QT4_CFLAGS)				\
	$(QT4_CORE_CFLAGS)			\
	$(QT4_NETWORK_CFLAGS)

libifolder_common_a_LIBADD = \
	$(QT4_LIBS)				\
	$(QT4_CORE_LIBS)			\
	$(QT4_NETWORK_LIBS)
	
libifolder_common_a_SOURCES = 			\
	ifolder-client.cpp					\
	IFNamedPipe.cpp						\
	IFNamedPipe.h						\
	IFMessages.h

#if !WINDOWS
#install-exec-hook:
#	rm -f $(DESTDIR)$(libdir)/libifolder.a
#	rm -f $(DESTDIR)$(libdir)/libifolder.la
#fi

#sdk:	
#	$(MAKE) -C $(IFOLDER_OS) sdk

maintainer-clean-local:
	rm -f Makefile.in

