#***********************************************************************
#*  $RCSfile$
#* 
#*  Copyright (C) 2004 Novell, Inc.
#*
#*  This library is free software; you can redistribute it and/or
#*  modify it under the terms of the GNU General Public
#*  License as published by the Free Software Foundation; either
#*  version 2 of the License, or (at your option) any later version.
#*
#*  This library is distributed in the hope that it will be useful,
#*  but WITHOUT ANY WARRANTY; without even the implied warranty of
#*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#*  Library General Public License for more details.
#*
#*  You should have received a copy of the GNU General Public
#*  License along with this library; if not, write to the Free
#*  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#*
#***********************************************************************/

include config.mk

#=============================================================================
# All of the targets get passed down to src
#=============================================================================
all:
	@$(MAKE) -C src $@

stage:
	@$(MAKE) -C src $@

stage-clean:
	@$(MAKE) -C src $@

clean:
	@$(MAKE) -C src $@

distclean:
	@$(MAKE) -C src $@
	$(call RM_IF_EXISTS,config.mk)

test:
	@$(MAKE) -C src $@
	
check:
	@$(MAKE) -C src $@

dist:
	@$(MAKE) -C src $@

package:
	@$(MAKE) -C src $@

package-nodeps:
	@$(MAKE) -C src $@

package-clean:
	@$(MAKE) -C src $@

package-test:
	@$(MAKE) -C src $@

install:
	@$(MAKE) -C src $@

uninstall:
	@$(MAKE) -C src $@

doc:
	@$(MAKE) -C src $@

doc-nodeps:
	@$(MAKE) -C src $@

doc-clean:
	@$(MAKE) -C src $@

api-doc:
	@$(MAKE) -C src $@

api-doc-nodeps:
	@$(MAKE) -C src $@

api-doc-clean:
	@$(MAKE) -C src $@

report:
	@$(MAKE) -C src $@


#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.1  2004/02/21 23:59:58  cgaisford
# Modified the build process and structure.  Now configure can be run at the root level and then make can be run.  Cleaned up configure stuff that used to be down in src
#
#
#
#=============================================================================
