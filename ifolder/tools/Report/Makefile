#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	Report.sln
#=============================================================================

# configure makefile
include config.mk

#=============================================================================
# Report variables
#=============================================================================
Report_SOURCES = AssemblyInfo.cs Report.cs
Report_INCLUDES = 
Report_FLAGS = /t:exe
Report_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Report_LIBPATH = 
Report_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = Report.exe 
CLEAN_FILES = Check.xml Report.pdb Report.xml  Report.xml
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS)

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
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))

# all
all: $(TARGETS)

# clean
clean:
	$(RM) $(TARGETS) $(CLEAN_FILES)

# test
test:
	@echo No NUnit test assemblies found.

# stage
STAGE_TARGETS = $(addprefix stage., $(STAGE_FILES))

.PHONY: stage $(STAGE_TARGETS)

stage: all $(STAGE_TARGETS)

$(STAGE_TARGETS):
	$(CP) $(subst stage.,,$@) $(STAGE_DIR)

#=============================================================================
# Report targets
#=============================================================================

# fix path seperator
Report_SOURCES := $(subst /,$(SEP), $(Report_SOURCES))
Report_INCLUDES := $(subst /,$(SEP), $(Report_INCLUDES))
Report_LIBPATH := $(subst /,$(SEP), $(Report_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Report_LIBPATH) $(Report_INCLUDES)

Report.exe: $(Report_SOURCES) $(Report_LIBS) config.mk
	$(CSC) /out:$@ /doc:Report.xml $(CSCFLAGS) $(Report_FLAGS) $(Report_LIBS:%=/r:%) $(Report_LIBPATH:%=/lib:%) $(Report_SOURCES)

