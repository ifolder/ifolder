#######################################################################
#  file: ifolder-nautilus.spec
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
#  Author: Calvin Gaisford <cgaisford@novell.com>
#		   Boyd Timothy <btimothy@novell.com>
#
#######################################################################

%define prefix /opt/gnome

Summary      : nautilus-ifolder
Name         : nautilus-ifolder
Version      : 0.1.0
Release      : 1
Copyright    : GPL
Group        : Applications/Productivity
Source       : %{name}-%{version}.tar.gz
Vendor       : Novell, Inc.
Packager     : Boyd Timothy <btimothy@novell.com>
BuildRoot    : %{_tmppath}/%{name}-%{version}

Requires     : nautilus >= 2.5
Requires     : simias >= 1.0
Requires     : ifolder3 >= 3.0

#=============================================================================
%Description
nautilus-ifolder Provides the iFolder plugin interface to the Nautilus File 
Manager

#=============================================================================
%ChangeLog


#=============================================================================
%Prep
%setup -n %{name}-%{version}

#=============================================================================
%Build
./configure --prefix=%{prefix}
make

#=============================================================================
%Install
%{__rm} -rf $RPM_BUILD_ROOT
make DESTDIR=$RPM_BUILD_ROOT install

#=============================================================================
%Clean
%{__rm} -rf $RPM_BUILD_ROOT

#=============================================================================
%Post

#=============================================================================
%Preun

#=============================================================================
%Postun

#=============================================================================
%Files
%defattr(-,root,root)
%{prefix}/*

