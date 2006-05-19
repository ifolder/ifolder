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

EXTRA_DIST = Simias.wsdl DomainService.wsdl

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep simiasClient.cpp`
	rm -rf `find $(distdir) |grep envC.cpp`
	rm -rf `find $(distdir) |grep stdsoap2.cpp`


INCLUDES = \
	-I$(top_srcdir)		\
	-I$(srcdir)/..		\
	-I$(top_builddir)

noinst_LIBRARIES = libifolder-core.a

DEFINES = 

libifolder_core_a_CXXFLAGS = \
	$(DEFINES)					\
	$(GLIB_CFLAGS)				\
	-DWITH_NONAMESPACES			\
	-DWITH_COOKIES

libifolder_core_a_LDFLAGS = \
	$(GLIB_LIBS)

#libifolder_core_a_LIBFLAGS = 

libifolder_core_a_SOURCES = \
	stdsoap2.cpp				\
	simiasC.cpp					\
	simiasClient.cpp			\
	glibclient.cpp				\
	IFApplication.cpp			\
	IFChanges.cpp				\
	IFConflict.cpp				\
	IFDirectory.cpp				\
	IFDomain.cpp				\
	IFFileInfo.cpp				\
	IFiFolder.cpp				\
	IFOptions.cpp				
	
#
# SOAP Generation
#
simiasC.cpp: simias.h
	$(GSOAP_PATH)/soapcpp2 -LCxn -psimias $(ProjectDir)simias.h

simias.h: stdsoap2.h stdsoap2.cpp
	$(GSOAP_PATH)/wsdl2h -s -o simias.h Simias.wsdl DomainService.wsdl

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
