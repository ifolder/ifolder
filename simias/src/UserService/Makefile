#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	UserService.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# UserService variables
#=============================================================================
UserService_SOURCES = AssemblyInfo.cs UserService.cs
UserService_INCLUDES = 
UserService_FLAGS = /t:library
UserService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionEvents.dll
UserService_LIBPATH = ../stage
UserService_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = UserService.dll 
CLEAN_FILES = Check.xml UserService.pdb UserService.Doc.xml  UserService.xml UserService.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) UserService.Doc.xml UserService.pdb 
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
	@echo No NUnit test assemblies found.

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# UserService targets
#=============================================================================

# fix path seperator
UserService_SOURCES := $(subst /,$(SEP), $(UserService_SOURCES))
UserService_INCLUDES := $(subst /,$(SEP), $(UserService_INCLUDES))
UserService_DEF := $(subst /,$(SEP), $(UserService_DEF))
UserService_LIBPATH := $(subst /,$(SEP), $(UserService_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(UserService_LIBPATH) $(UserService_INCLUDES)

UserService.dll: $(UserService_SOURCES) $(UserService_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:UserService.Doc.xml $(CSCFLAGS) $(UserService_FLAGS) $(UserService_LIBS:%=/r:%) $(UserService_LIBPATH:%=/lib:%) $(UserService_SOURCES)

