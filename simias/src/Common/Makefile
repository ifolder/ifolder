#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	Common.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# Common variables
#=============================================================================
Common_SOURCES = Library/AssemblyInfo.cs Library/MyDns.cs Library/MyEnvironment.cs Library/MyPath.cs Library/MyPlatformID.cs Library/MyRuntimeID.cs Library/MyTrace.cs
Common_INCLUDES = 
Common_FLAGS = /t:library
Common_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Common_LIBPATH = Library
Common_LDFLAGS = 

#=============================================================================
# CommonTests variables
#=============================================================================
CommonTests_SOURCES = Tests/AssemblyInfo.cs Tests/CommonTests.cs
CommonTests_INCLUDES = 
CommonTests_FLAGS = /t:library
CommonTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll nunit.framework.dll
CommonTests_LIBPATH = Tests Tests/../Library Tests/../../../tools/NUnit/bin
CommonTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = 
LIB_FILES = Common.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = CommonTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = Common.dll CommonTests.dll 
CLEAN_FILES = Check.xml Common.pdb Common.Doc.xml Check.xml CommonTests.pdb CommonTests.Doc.xml  Common.xml Common.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = CommonTests.dll 
STAGE_FILES = $(TARGETS) Common.Doc.xml CommonTests.Doc.xml Common.pdb CommonTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:Common.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# Common targets
#=============================================================================

# fix path seperator
Common_SOURCES := $(subst /,$(SEP), $(Common_SOURCES))
Common_INCLUDES := $(subst /,$(SEP), $(Common_INCLUDES))
Common_DEF := $(subst /,$(SEP), $(Common_DEF))
Common_LIBPATH := $(subst /,$(SEP), $(Common_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Common_LIBPATH) $(Common_INCLUDES)

Common.dll: $(Common_SOURCES) $(Common_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Common.Doc.xml $(CSCFLAGS) $(Common_FLAGS) $(Common_LIBS:%=/r:%) $(Common_LIBPATH:%=/lib:%) $(Common_SOURCES)

#=============================================================================
# CommonTests targets
#=============================================================================

# fix path seperator
CommonTests_SOURCES := $(subst /,$(SEP), $(CommonTests_SOURCES))
CommonTests_INCLUDES := $(subst /,$(SEP), $(CommonTests_INCLUDES))
CommonTests_DEF := $(subst /,$(SEP), $(CommonTests_DEF))
CommonTests_LIBPATH := $(subst /,$(SEP), $(CommonTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CommonTests_LIBPATH) $(CommonTests_INCLUDES)

CommonTests.dll: $(CommonTests_SOURCES) $(CommonTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CommonTests.Doc.xml $(CSCFLAGS) $(CommonTests_FLAGS) $(CommonTests_LIBS:%=/r:%) $(CommonTests_LIBPATH:%=/lib:%) $(CommonTests_SOURCES)

include ../../common.mk
