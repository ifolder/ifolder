#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	Configuration.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# ConfTests variables
#=============================================================================
ConfTests_SOURCES = ConfTests/AssemblyInfo.cs ConfTests/ConfTests.cs
ConfTests_INCLUDES = 
ConfTests_FLAGS = /t:library
ConfTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll nunit.extensions.dll Configuration.dll
ConfTests_LIBPATH = ConfTests ConfTests/../../../tools/NUnit/bin ConfTests/../../../tools/NUnit/bin ConfTests/..
ConfTests_LDFLAGS = 

#=============================================================================
# Configuration variables
#=============================================================================
Configuration_SOURCES = AssemblyInfo.cs Configuration.cs
Configuration_INCLUDES = 
Configuration_FLAGS = /t:library
Configuration_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Configuration_LIBPATH = 
Configuration_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = Configuration.dll
LIB_FILES = 
DATA_FILES = 
TEST_BIN_FILES = ConfTests.dll
TEST_LIB_FILES = 
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
CLEAN_FILES = Check.xml ConfTests.pdb ConfTests.xml Check.xml Configuration.pdb Configuration.xml  Configuration.xml Configuration.Doc.xml
NUNIT_TESTS = ConfTests.dll 
STAGE_FILES = $(TARGETS) ConfTests.Doc.xml Configuration.Doc.xml ConfTests.pdb Configuration.pdb 
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

# build clean file list up
CLEAN_FILES := $(sort $(CLEAN_FILES) $(TARGETS))
CLEAN_TARGETS := $(addprefix clean., $(CLEAN_FILES))

.PHONY: $(CLEAN_TARGETS) $(EXTRA_STAGE_FILES)

# all
all: $(TARGETS)
#all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:Configuration.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# ConfTests targets
#=============================================================================

# fix path seperator
ConfTests_SOURCES := $(subst /,$(SEP), $(ConfTests_SOURCES))
ConfTests_INCLUDES := $(subst /,$(SEP), $(ConfTests_INCLUDES))
ConfTests_DEF := $(subst /,$(SEP), $(ConfTests_DEF))
ConfTests_LIBPATH := $(subst /,$(SEP), $(ConfTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(ConfTests_LIBPATH) $(ConfTests_INCLUDES)

ConfTests.dll: $(ConfTests_SOURCES) $(ConfTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:ConfTests.Doc.xml $(CSCFLAGS) $(ConfTests_FLAGS) $(ConfTests_LIBS:%=/r:%) $(ConfTests_LIBPATH:%=/lib:%) $(ConfTests_SOURCES)

#=============================================================================
# Configuration targets
#=============================================================================

# fix path seperator
Configuration_SOURCES := $(subst /,$(SEP), $(Configuration_SOURCES))
Configuration_INCLUDES := $(subst /,$(SEP), $(Configuration_INCLUDES))
Configuration_DEF := $(subst /,$(SEP), $(Configuration_DEF))
Configuration_LIBPATH := $(subst /,$(SEP), $(Configuration_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Configuration_LIBPATH) $(Configuration_INCLUDES)

Configuration.dll: $(Configuration_SOURCES) $(Configuration_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Configuration.Doc.xml $(CSCFLAGS) $(Configuration_FLAGS) $(Configuration_LIBS:%=/r:%) $(Configuration_LIBPATH:%=/lib:%) $(Configuration_SOURCES)


include ../../common.mk
