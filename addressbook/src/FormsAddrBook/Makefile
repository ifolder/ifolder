#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FormsAddrBook.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# Book variables
#=============================================================================
Book_SOURCES = AssemblyInfo.cs FormsAddrBook.cs
Book_INCLUDES = 
Book_FLAGS = /t:winexe $(ICON_FLAG)App$(ICON_EXT)
Book_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) FormsBookLib.dll AddressBook.dll
Book_LIBPATH = ../FormsBookLib $(SIMIAS_ROOT)/src/stage
Book_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = Book.exe 
CLEAN_FILES = Check.xml Book.pdb Book.Doc.xml  FormsAddrBook.xml FormsAddrBook.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) Book.Doc.xml Book.pdb 
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
# Book targets
#=============================================================================

# fix path seperator
Book_SOURCES := $(subst /,$(SEP), $(Book_SOURCES))
Book_INCLUDES := $(subst /,$(SEP), $(Book_INCLUDES))
Book_DEF := $(subst /,$(SEP), $(Book_DEF))
Book_LIBPATH := $(subst /,$(SEP), $(Book_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Book_LIBPATH) $(Book_INCLUDES)

Book.exe: $(Book_SOURCES) $(Book_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:Book.Doc.xml $(CSCFLAGS) $(Book_FLAGS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_SOURCES)

