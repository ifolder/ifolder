#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	CollectionEvents.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# CollectionEventsTests variables
#=============================================================================
CollectionEventsTests_SOURCES = CollectionEventsTests/AssemblyInfo.cs CollectionEventsTests/CollectionEventsTests.cs
CollectionEventsTests_INCLUDES = 
CollectionEventsTests_FLAGS = /t:exe
CollectionEventsTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Configuration.dll CollectionEvents.dll Common.dll
CollectionEventsTests_LIBPATH = CollectionEventsTests ../../tools/NUnit/bin ../Common ../Configuration
CollectionEventsTests_LDFLAGS = 

#=============================================================================
# CollectionEvents variables
#=============================================================================
CollectionEvents_SOURCES = AssemblyInfo.cs EventBroker.cs EventPublisher.cs EventSubscriber.cs
CollectionEvents_INCLUDES = 
CollectionEvents_FLAGS = /t:library
CollectionEvents_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll
CollectionEvents_LIBPATH = ../Configuration
CollectionEvents_LDFLAGS = 

#=============================================================================
# CsEventBroker variables
#=============================================================================
CsEventBroker_SOURCES = CsEventBroker/AssemblyInfo.cs CsEventBroker/CsEventBroker.cs
CsEventBroker_INCLUDES = 
CsEventBroker_FLAGS = /t:exe $(ICON_FLAG)CsEventBroker/App$(ICON_EXT)
CsEventBroker_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll CollectionEvents.dll 
CsEventBroker_LIBPATH = ../Common ../Configuration CsEventBroker
CsEventBroker_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = CsEventBroker.exe
LIB_FILES = CollectionEvents.dll
DATA_FILES = 
TEST_BIN_FILES = CollectionEventsTests.exe
TEST_LIB_FILES = 
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = CollectionEventsTests.exe CollectionEvents.dll CsEventBroker.exe 
CLEAN_FILES = Check.xml CollectionEventsTests.pdb CollectionEventsTests.Doc.xml Check.xml CollectionEvents.pdb CollectionEvents.Doc.xml Check.xml CsEventBroker.pdb CsEventBroker.Doc.xml  CollectionEvents.xml CollectionEvents.Doc.xml
CLEAN_DIRS = CollectionEventsTests/bin CollectionEventsTests/obj bin obj CsEventBroker/bin CsEventBroker/obj 
NUNIT_TESTS = CollectionEventsTests.exe 
STAGE_FILES = $(TARGETS) CollectionEventsTests.Doc.xml CollectionEvents.Doc.xml CsEventBroker.Doc.xml CollectionEventsTests.pdb CollectionEvents.pdb CsEventBroker.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionEvents.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# CollectionEventsTests targets
#=============================================================================

# fix path seperator
CollectionEventsTests_SOURCES := $(subst /,$(SEP), $(CollectionEventsTests_SOURCES))
CollectionEventsTests_INCLUDES := $(subst /,$(SEP), $(CollectionEventsTests_INCLUDES))
CollectionEventsTests_DEF := $(subst /,$(SEP), $(CollectionEventsTests_DEF))
CollectionEventsTests_LIBPATH := $(subst /,$(SEP), $(CollectionEventsTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionEventsTests_LIBPATH) $(CollectionEventsTests_INCLUDES)

CollectionEventsTests.exe: $(CollectionEventsTests_SOURCES) $(CollectionEventsTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionEventsTests.Doc.xml $(CSCFLAGS) $(CollectionEventsTests_FLAGS) $(CollectionEventsTests_LIBS:%=/r:%) $(CollectionEventsTests_LIBPATH:%=/lib:%) $(CollectionEventsTests_SOURCES)

#=============================================================================
# CollectionEvents targets
#=============================================================================

# fix path seperator
CollectionEvents_SOURCES := $(subst /,$(SEP), $(CollectionEvents_SOURCES))
CollectionEvents_INCLUDES := $(subst /,$(SEP), $(CollectionEvents_INCLUDES))
CollectionEvents_DEF := $(subst /,$(SEP), $(CollectionEvents_DEF))
CollectionEvents_LIBPATH := $(subst /,$(SEP), $(CollectionEvents_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionEvents_LIBPATH) $(CollectionEvents_INCLUDES)

CollectionEvents.dll: $(CollectionEvents_SOURCES) $(CollectionEvents_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionEvents.Doc.xml $(CSCFLAGS) $(CollectionEvents_FLAGS) $(CollectionEvents_LIBS:%=/r:%) $(CollectionEvents_LIBPATH:%=/lib:%) $(CollectionEvents_SOURCES)

#=============================================================================
# CsEventBroker targets
#=============================================================================

# fix path seperator
CsEventBroker_SOURCES := $(subst /,$(SEP), $(CsEventBroker_SOURCES))
CsEventBroker_INCLUDES := $(subst /,$(SEP), $(CsEventBroker_INCLUDES))
CsEventBroker_DEF := $(subst /,$(SEP), $(CsEventBroker_DEF))
CsEventBroker_LIBPATH := $(subst /,$(SEP), $(CsEventBroker_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CsEventBroker_LIBPATH) $(CsEventBroker_INCLUDES)

CsEventBroker.exe: $(CsEventBroker_SOURCES) $(CsEventBroker_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CsEventBroker.Doc.xml $(CSCFLAGS) $(CsEventBroker_FLAGS) $(CsEventBroker_LIBS:%=/r:%) $(CsEventBroker_LIBPATH:%=/lib:%) $(CsEventBroker_SOURCES)

include ../../common.mk

