#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	Agent.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# Agent variables
#=============================================================================
Agent_SOURCES = Library/AgentFactory.cs Library/AssemblyInfo.cs Library/IInviteAgent.cs Library/Invitation.cs
Agent_INCLUDES = 
Agent_FLAGS = /t:library
Agent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Identity.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll
Agent_LIBPATH = ../Configuration ../Identity ../StoreProvider ../CollectionEvents ../CollectionStore Library 
Agent_LDFLAGS = 

#=============================================================================
# AgentTests variables
#=============================================================================
AgentTests_SOURCES = Tests/AgentTests.cs Tests/AssemblyInfo.cs
AgentTests_INCLUDES = 
AgentTests_FLAGS = /t:library
AgentTests_LIBS = $(Agent_LIBS) $(SYSTEM_XML) Common.dll Agent.dll nunit.framework.dll 
AgentTests_LIBPATH = $(Agent_LIBPATH) ../Common Tests ../../tools/NUnit/bin
AgentTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = Agent.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = AgentTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = Agent.dll AgentTests.dll 
CLEAN_FILES = Check.xml Agent.pdb Agent.Doc.xml Check.xml AgentTests.pdb AgentTests.Doc.xml  Agent.xml Agent.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = AgentTests.dll 
STAGE_FILES = $(TARGETS) Agent.Doc.xml AgentTests.Doc.xml Agent.pdb AgentTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:Agent.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# Agent targets
#=============================================================================

# fix path seperator
Agent_SOURCES := $(subst /,$(SEP), $(Agent_SOURCES))
Agent_INCLUDES := $(subst /,$(SEP), $(Agent_INCLUDES))
Agent_DEF := $(subst /,$(SEP), $(Agent_DEF))
Agent_LIBPATH := $(subst /,$(SEP), $(Agent_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Agent_LIBPATH) $(Agent_INCLUDES)

Agent.dll: $(Agent_SOURCES) $(Agent_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Agent.Doc.xml $(CSCFLAGS) $(Agent_FLAGS) $(Agent_LIBS:%=/r:%) $(Agent_LIBPATH:%=/lib:%) $(Agent_SOURCES)

#=============================================================================
# AgentTests targets
#=============================================================================

# fix path seperator
AgentTests_SOURCES := $(subst /,$(SEP), $(AgentTests_SOURCES))
AgentTests_INCLUDES := $(subst /,$(SEP), $(AgentTests_INCLUDES))
AgentTests_DEF := $(subst /,$(SEP), $(AgentTests_DEF))
AgentTests_LIBPATH := $(subst /,$(SEP), $(AgentTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(AgentTests_LIBPATH) $(AgentTests_INCLUDES)

AgentTests.dll: $(AgentTests_SOURCES) $(AgentTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:AgentTests.Doc.xml $(CSCFLAGS) $(AgentTests_FLAGS) $(AgentTests_LIBS:%=/r:%) $(AgentTests_LIBPATH:%=/lib:%) $(AgentTests_SOURCES)

include ../../common.mk
