#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	iFolder.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# iFolderCmd variables
#=============================================================================
iFolderCmd_SOURCES = iFolderCmd.cs
iFolderCmd_INCLUDES = 
iFolderCmd_FLAGS = /t:exe
iFolderCmd_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll nunit.extensions.dll Agent.dll iFolder.dll CollectionStore.dll Identity.dll iFolder.dll
iFolderCmd_LIBPATH = $(SIMIAS_ROOT)/src/stage
iFolderCmd_LDFLAGS = 

#=============================================================================
# iFolderTests variables
#=============================================================================
iFolderTests_SOURCES = iFolderTests.cs
iFolderTests_INCLUDES = 
iFolderTests_FLAGS = /t:library
iFolderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll nunit.extensions.dll Agent.dll iFolder.dll CollectionStore.dll Identity.dll
iFolderTests_LIBPATH = ../../tools/NUnit/bin ../../tools/NUnit/bin $(SIMIAS_ROOT)/src/stage
iFolderTests_LDFLAGS = 

#=============================================================================
# iFolder variables
#=============================================================================
iFolder_SOURCES = iFolder.cs iFolderFile.cs iFolderManager.cs
iFolder_INCLUDES = 
iFolder_FLAGS = /t:library
iFolder_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionStore.dll Agent.dll Identity.dll
iFolder_LIBPATH = $(SIMIAS_ROOT)/src/stage
iFolder_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = iFolderCmd.exe iFolderTests.dll iFolder.dll 
CLEAN_FILES = Check.xml iFolderCmd.pdb iFolderCmd.Doc.xml Check.xml iFolderTests.pdb iFolderTests.Doc.xml Check.xml iFolder.pdb iFolder.Doc.xml  iFolder.xml iFolder.Doc.xml
NUNIT_TESTS = iFolderTests.dll 
STAGE_FILES = $(TARGETS) iFolderCmd.Doc.xml iFolderTests.Doc.xml iFolder.Doc.xml
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

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:iFolder.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# iFolderCmd targets
#=============================================================================

# fix path seperator
iFolderCmd_SOURCES := $(subst /,$(SEP), $(iFolderCmd_SOURCES))
iFolderCmd_INCLUDES := $(subst /,$(SEP), $(iFolderCmd_INCLUDES))
iFolderCmd_DEF := $(subst /,$(SEP), $(iFolderCmd_DEF))
iFolderCmd_LIBPATH := $(subst /,$(SEP), $(iFolderCmd_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderCmd_LIBPATH) $(iFolderCmd_INCLUDES)

iFolderCmd.exe: $(iFolderCmd_SOURCES) $(iFolderCmd_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:iFolderCmd.Doc.xml $(CSCFLAGS) $(iFolderCmd_FLAGS) $(iFolderCmd_LIBS:%=/r:%) $(iFolderCmd_LIBPATH:%=/lib:%) $(iFolderCmd_SOURCES)

#=============================================================================
# iFolderTests targets
#=============================================================================

# fix path seperator
iFolderTests_SOURCES := $(subst /,$(SEP), $(iFolderTests_SOURCES))
iFolderTests_INCLUDES := $(subst /,$(SEP), $(iFolderTests_INCLUDES))
iFolderTests_DEF := $(subst /,$(SEP), $(iFolderTests_DEF))
iFolderTests_LIBPATH := $(subst /,$(SEP), $(iFolderTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderTests_LIBPATH) $(iFolderTests_INCLUDES)

iFolderTests.dll: $(iFolderTests_SOURCES) $(iFolderTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:iFolderTests.Doc.xml $(CSCFLAGS) $(iFolderTests_FLAGS) $(iFolderTests_LIBS:%=/r:%) $(iFolderTests_LIBPATH:%=/lib:%) $(iFolderTests_SOURCES)

#=============================================================================
# iFolder targets
#=============================================================================

# fix path seperator
iFolder_SOURCES := $(subst /,$(SEP), $(iFolder_SOURCES))
iFolder_INCLUDES := $(subst /,$(SEP), $(iFolder_INCLUDES))
iFolder_DEF := $(subst /,$(SEP), $(iFolder_DEF))
iFolder_LIBPATH := $(subst /,$(SEP), $(iFolder_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolder_LIBPATH) $(iFolder_INCLUDES)

iFolder.dll: $(iFolder_SOURCES) $(iFolder_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:iFolder.Doc.xml $(CSCFLAGS) $(iFolder_FLAGS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_SOURCES)

