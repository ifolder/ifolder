#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	JpegMeta.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# JpegMeta variables
#=============================================================================
JpegMeta_SOURCES = AssemblyInfo.cs JpegMeta.cs
JpegMeta_INCLUDES = 
JpegMeta_FLAGS = /t:library
JpegMeta_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll
JpegMeta_LIBPATH = 
JpegMeta_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = JpegMeta.dll 
CLEAN_FILES = Check.xml JpegMeta.pdb JpegMeta.Doc.xml  JpegMeta.xml JpegMeta.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) JpegMeta.Doc.xml JpegMeta.pdb 
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
# JpegMeta targets
#=============================================================================

# fix path seperator
JpegMeta_SOURCES := $(subst /,$(SEP), $(JpegMeta_SOURCES))
JpegMeta_INCLUDES := $(subst /,$(SEP), $(JpegMeta_INCLUDES))
JpegMeta_DEF := $(subst /,$(SEP), $(JpegMeta_DEF))
JpegMeta_LIBPATH := $(subst /,$(SEP), $(JpegMeta_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(JpegMeta_LIBPATH) $(JpegMeta_INCLUDES)

JpegMeta.dll: $(JpegMeta_SOURCES) $(JpegMeta_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:JpegMeta.Doc.xml $(CSCFLAGS) $(JpegMeta_FLAGS) $(JpegMeta_LIBS:%=/r:%) $(JpegMeta_LIBPATH:%=/lib:%) $(JpegMeta_SOURCES)

