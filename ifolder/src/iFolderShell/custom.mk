#=============================================================================
# Module:   Custom Makefile
#
# Abstract: Custom makefile directives for the FlaimProvider directory.
#
# Notes:		    
#
# CVS Info:
#  $Source$
#  $Revision$
#  $Author$
#  $Date$
#
# Unpublished Copyright of Novell, Inc. All Rights Reserved.
#
# No part of this file may be duplicated, revised, translated, localized,
# or modified in any manner or compiled, linked or uploaded or downloaded
# to or from any computer system without the prior written consent of
# Novell, Inc.
#=============================================================================

# windows only
ifeq "windows" "$(TARGET_OS)"

CHARFLAGS = -DUNICODE -D_UNICODE	
CXXFLAGS := $(CXXFLAGS) $(CHARFLAGS)
CLEAN_FILES := $(CLEAN_FILES) iFolderShell\ifoldercomponent.tlh iFolderShell\ifoldercomponent.tli
                                          
endif
                                                                       
