#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FsWatcher.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# FsWatcher variables
#=============================================================================
FsWatcher_SOURCES = AssemblyInfo.cs FsWatcher.cs
FsWatcher_INCLUDES = 
FsWatcher_FLAGS = /t:exe $(ICON_FLAG)App$(ICON_EXT)
FsWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionStore.dll CollectionEvents.dll UserService.dll
FsWatcher_LIBPATH = ../CollectionStore ../CollectionEvents ../UserService
FsWatcher_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = FsWatcher.exe 
CLEAN_FILES = Check.xml FsWatcher.pdb FsWatcher.Doc.xml  FsWatcher.xml FsWatcher.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) FsWatcher.Doc.xml FsWatcher.pdb 
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
# FsWatcher targets
#=============================================================================

# fix path seperator
FsWatcher_SOURCES := $(subst /,$(SEP), $(FsWatcher_SOURCES))
FsWatcher_INCLUDES := $(subst /,$(SEP), $(FsWatcher_INCLUDES))
FsWatcher_DEF := $(subst /,$(SEP), $(FsWatcher_DEF))
FsWatcher_LIBPATH := $(subst /,$(SEP), $(FsWatcher_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(FsWatcher_LIBPATH) $(FsWatcher_INCLUDES)

FsWatcher.exe: $(FsWatcher_SOURCES) $(FsWatcher_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:FsWatcher.Doc.xml $(CSCFLAGS) $(FsWatcher_FLAGS) $(FsWatcher_LIBS:%=/r:%) $(FsWatcher_LIBPATH:%=/lib:%) $(FsWatcher_SOURCES)

