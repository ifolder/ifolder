#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	WorkGroupIdentity.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# WorkGroupIdentityTests variables
#=============================================================================
WorkGroupIdentityTests_SOURCES = WorkGroupIdentityTests/AssemblyInfo.cs WorkGroupIdentityTests/WorkGroupIdentityTests.cs
WorkGroupIdentityTests_INCLUDES = 
WorkGroupIdentityTests_FLAGS = /t:library
WorkGroupIdentityTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.tests.dll nunit.extensions.dll nunit.framework.dll Identity.dll WorkGroupIdentity.dll
WorkGroupIdentityTests_LIBPATH = WorkGroupIdentityTests WorkGroupIdentityTests/../../../tools/NUnit/bin WorkGroupIdentityTests/../../../tools/NUnit/bin WorkGroupIdentityTests/../../../tools/NUnit/bin WorkGroupIdentityTests/../../stage WorkGroupIdentityTests/../../stage
WorkGroupIdentityTests_LDFLAGS = 

#=============================================================================
# WorkGroupIdentity variables
#=============================================================================
WorkGroupIdentity_SOURCES = AssemblyInfo.cs WorkGroupIdentity.cs
WorkGroupIdentity_INCLUDES = 
WorkGroupIdentity_FLAGS = /t:library
WorkGroupIdentity_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Identity.dll
WorkGroupIdentity_LIBPATH = ../stage
WorkGroupIdentity_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = WorkGroupIdentityTests.dll WorkGroupIdentity.dll 
CLEAN_FILES = Check.xml WorkGroupIdentityTests.pdb WorkGroupIdentityTests.Doc.xml Check.xml WorkGroupIdentity.pdb WorkGroupIdentity.Doc.xml  WorkGroupIdentity.xml WorkGroupIdentity.Doc.xml
CLEAN_DIRS = WorkGroupIdentityTests/bin WorkGroupIdentityTests/obj bin obj 
NUNIT_TESTS = WorkGroupIdentityTests.dll 
STAGE_FILES = $(TARGETS) WorkGroupIdentityTests.Doc.xml WorkGroupIdentity.Doc.xml WorkGroupIdentityTests.pdb WorkGroupIdentity.pdb 
STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

#=============================================================================
# custom makefile
#=============================================================================
ifneq "$(wildcard custom.mk)" ""
	include custom.mk
endif

#=============================================================================
# global targets
#=============================================================================

# fix path seperator
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))
EXTRA_STAGE_FILES := $(subst /,$(SEP),$(EXTRA_STAGE_FILES))
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
CLEAN_DIRS := $(subst /,$(SEP),$(CLEAN_DIRS))

# build clean file list up
CLEAN_FILES := $(sort $(CLEAN_FILES) $(TARGETS))
CLEAN_TARGETS := $(addprefix clean., $(CLEAN_FILES))

.PHONY: $(CLEAN_TARGETS) $(EXTRA_STAGE_FILES)

# all
all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:WorkGroupIdentity.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# WorkGroupIdentityTests targets
#=============================================================================

# fix path seperator
WorkGroupIdentityTests_SOURCES := $(subst /,$(SEP), $(WorkGroupIdentityTests_SOURCES))
WorkGroupIdentityTests_INCLUDES := $(subst /,$(SEP), $(WorkGroupIdentityTests_INCLUDES))
WorkGroupIdentityTests_DEF := $(subst /,$(SEP), $(WorkGroupIdentityTests_DEF))
WorkGroupIdentityTests_LIBPATH := $(subst /,$(SEP), $(WorkGroupIdentityTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(WorkGroupIdentityTests_LIBPATH) $(WorkGroupIdentityTests_INCLUDES)

WorkGroupIdentityTests.dll: $(WorkGroupIdentityTests_SOURCES) $(WorkGroupIdentityTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:WorkGroupIdentityTests.Doc.xml $(CSCFLAGS) $(WorkGroupIdentityTests_FLAGS) $(WorkGroupIdentityTests_LIBS:%=/r:%) $(WorkGroupIdentityTests_LIBPATH:%=/lib:%) $(WorkGroupIdentityTests_SOURCES)

#=============================================================================
# WorkGroupIdentity targets
#=============================================================================

# fix path seperator
WorkGroupIdentity_SOURCES := $(subst /,$(SEP), $(WorkGroupIdentity_SOURCES))
WorkGroupIdentity_INCLUDES := $(subst /,$(SEP), $(WorkGroupIdentity_INCLUDES))
WorkGroupIdentity_DEF := $(subst /,$(SEP), $(WorkGroupIdentity_DEF))
WorkGroupIdentity_LIBPATH := $(subst /,$(SEP), $(WorkGroupIdentity_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(WorkGroupIdentity_LIBPATH) $(WorkGroupIdentity_INCLUDES)

WorkGroupIdentity.dll: $(WorkGroupIdentity_SOURCES) $(WorkGroupIdentity_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:WorkGroupIdentity.Doc.xml $(CSCFLAGS) $(WorkGroupIdentity_FLAGS) $(WorkGroupIdentity_LIBS:%=/r:%) $(WorkGroupIdentity_LIBPATH:%=/lib:%) $(WorkGroupIdentity_SOURCES)

