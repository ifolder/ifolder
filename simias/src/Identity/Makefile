#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	Identity.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# Identity variables
#=============================================================================
Identity_SOURCES = AssemblyInfo.cs IIdentity.cs
Identity_INCLUDES = 
Identity_FLAGS = /t:library
Identity_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Identity_LIBPATH = 
Identity_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = Identity.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES =
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = Identity.dll 
CLEAN_FILES = Check.xml Identity.pdb Identity.Doc.xml  Identity.xml Identity.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) Identity.Doc.xml Identity.pdb 
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
all: $(TARGETS)
#all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
#test:
#	@echo No NUnit test assemblies found.

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# Identity targets
#=============================================================================

# fix path seperator
Identity_SOURCES := $(subst /,$(SEP), $(Identity_SOURCES))
Identity_INCLUDES := $(subst /,$(SEP), $(Identity_INCLUDES))
Identity_DEF := $(subst /,$(SEP), $(Identity_DEF))
Identity_LIBPATH := $(subst /,$(SEP), $(Identity_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Identity_LIBPATH) $(Identity_INCLUDES)

Identity.dll: $(Identity_SOURCES) $(Identity_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Identity.Doc.xml $(CSCFLAGS) $(Identity_FLAGS) $(Identity_LIBS:%=/r:%) $(Identity_LIBPATH:%=/lib:%) $(Identity_SOURCES)

include ../../common.mk
