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
include sources.mak

EXTRA_DIST = DomainService.wsdl iFolderWeb.wsdl

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep simiasClient.cpp`
	rm -rf `find $(distdir) |grep envC.cpp`
	rm -rf `find $(distdir) |grep stdsoap2.cpp`


INCLUDES = \
	-I$(top_srcdir)		\
	-I$(srcdir)/..		\
	-I$(top_builddir)

lib_LTLIBRARIES = libifolder3-core.la

DEFINES = 

libifolder3_core_la_CXXFLAGS =	\
	$(DEFINES)					\
	$(UNIV_CFLAGS)				\
	$(GLIB_CFLAGS)				\
	-DWITH_NONAMESPACES			\
	-DWITH_COOKIES

libifolder3_core_la_LDFLAGS =	\
	$(UNIV_LDFLAGS)				\
	$(GLIB_LIBS)

#libifolder_core_a_LIBFLAGS = 

	
#
# SOAP Generation
#
simiasC.cpp: simias.h
	$(GSOAP_PATH)/soapcpp2 -LCxn -psimias $(ProjectDir)simias.h

simias.h: stdsoap2.h stdsoap2.cpp
	$(GSOAP_PATH)/wsdl2h -s -o simias.h DomainService.wsdl iFolderWeb.wsdl

simiasClient.cpp:
	$(GSOAP_PATH)/soapcpp2 -LCxn -psimias $(ProjectDir)simias.h

stdsoap2.cpp:
	cp $(GSOAP_PATH)/stdsoap2.cpp .

stdsoap2.h:
	cp $(GSOAP_PATH)/stdsoap2.h .

clean-local:
	rm -rf *.xml
	rm -rf simias*
	rm -rf stdsoap2.*

maintainer-clean-local:
	rm -f Makefile.in
