#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FormsTrayApp.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# iFolderApp variables
#=============================================================================
iFolderApp_SOURCES = AssemblyInfo.cs FormsTrayApp.cs
iFolderApp_INCLUDES = 
iFolderApp_FLAGS = /t:winexe $(ICON_FLAG)App$(ICON_EXT)
iFolderApp_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Sync.dll SyncManager.dll SyncCommon.dll CommonFormWindows.dll Common.dll SyncLogicLite.dll Configuration.dll CollectionEvents.dll
iFolderApp_LIBPATH = $(SIMIAS_ROOT)/src/stage
iFolderApp_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = iFolderApp.exe 
CLEAN_FILES = Check.xml iFolderApp.pdb iFolderApp.Doc.xml  FormsTrayApp.xml FormsTrayApp.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) iFolderApp.Doc.xml iFolderApp.pdb 
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
# iFolderApp targets
#=============================================================================

# fix path seperator
iFolderApp_SOURCES := $(subst /,$(SEP), $(iFolderApp_SOURCES))
iFolderApp_INCLUDES := $(subst /,$(SEP), $(iFolderApp_INCLUDES))
iFolderApp_DEF := $(subst /,$(SEP), $(iFolderApp_DEF))
iFolderApp_LIBPATH := $(subst /,$(SEP), $(iFolderApp_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderApp_LIBPATH) $(iFolderApp_INCLUDES)

iFolderApp.exe: $(iFolderApp_SOURCES) $(iFolderApp_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:iFolderApp.Doc.xml $(CSCFLAGS) $(iFolderApp_FLAGS) $(iFolderApp_LIBS:%=/r:%) $(iFolderApp_LIBPATH:%=/lib:%) $(iFolderApp_SOURCES)

