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

# linux only
ifeq "linux" "$(TARGET_OS)"
export EXTRA_STAGE_FILES := ../../external/sqlite/linux/libsqlite.so 
else
export EXTRA_STAGE_FILES := ../../external/sqlite/w32/sqlite.dll 
endif

