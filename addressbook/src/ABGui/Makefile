#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	ABGui.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# abgui variables
#=============================================================================
abgui_SOURCES = ABGui.cs AssemblyInfo.cs
abgui_INCLUDES = 
abgui_FLAGS = /t:winexe $(ICON_FLAG)App$(ICON_EXT)
abgui_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Identity.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
abgui_LIBPATH = Identity Configuration StoreProvider CollectionEvents CollectionStore AddressBook
abgui_LDFLAGS = 
abgui_LIBPATH := $(addprefix $(SIMIAS_ROOT)/src/, $(abgui_LIBPATH))

#=============================================================================
# globals
#=============================================================================
BIN_FILES = abgui.exe
LIB_FILES = 
DATA_FILES =
TEST_BIN_FILES =
TEST_LIB_FILES =
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES)
#TARGETS = abgui.exe 
CLEAN_FILES = Check.xml abgui.pdb abgui.Doc.xml  ABGui.xml ABGui.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
#STAGE_FILES = $(TARGETS) abgui.Doc.xml abgui.pdb 
#STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

#=============================================================================
# custom makefile
#=============================================================================
ifneq "$(wildcard custom.mk)" ""
	include custom.mk
endif

#=============================================================================
# global targets
#=============================================================================
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))

# build clean file list up
CLEAN_FILES := $(sort $(CLEAN_FILES) $(TARGETS))
CLEAN_TARGETS := $(addprefix clean., $(CLEAN_FILES))

.PHONY: $(CLEAN_TARGETS)

# all
all: $(TARGETS)
# all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

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

abgui.exe: $(abgui_SOURCES) $(abgui_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:abgui.Doc.xml $(CSCFLAGS) $(abgui_FLAGS) $(abgui_LIBS:%=/r:%) $(abgui_LIBPATH:%=/lib:%) $(abgui_SOURCES)

include ../../common.mk

