#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	ABGui.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# abgui variables
#=============================================================================
abgui_SOURCES = ABGui.cs AssemblyInfo.cs
abgui_INCLUDES = 
abgui_FLAGS = /t:winexe $(ICON_FLAG)App$(ICON_EXT)
abgui_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) WorkGroupIdentity.dll CollectionStore.dll Identity.dll StoreProvider.dll AddressBook.dll
abgui_LIBPATH = $(SIMIAS_ROOT)/src/stage ../stage
abgui_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = abgui.exe 
CLEAN_FILES = Check.xml abgui.pdb abgui.Doc.xml  ABGui.xml ABGui.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) abgui.Doc.xml abgui.pdb 
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
# abgui targets
#=============================================================================

# fix path seperator
abgui_SOURCES := $(subst /,$(SEP), $(abgui_SOURCES))
abgui_INCLUDES := $(subst /,$(SEP), $(abgui_INCLUDES))
abgui_DEF := $(subst /,$(SEP), $(abgui_DEF))
abgui_LIBPATH := $(subst /,$(SEP), $(abgui_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(abgui_LIBPATH) $(abgui_INCLUDES)

abgui.exe: $(abgui_SOURCES) $(abgui_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:abgui.Doc.xml $(CSCFLAGS) $(abgui_FLAGS) $(abgui_LIBS:%=/r:%) $(abgui_LIBPATH:%=/lib:%) $(abgui_SOURCES)

