#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	CommonFormWindows.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# CommonFormWindows variables
#=============================================================================
CommonFormWindows_SOURCES = Library/AssemblyInfo.cs Library/MyTraceForm.cs Library/MyTraceListener.cs
CommonFormWindows_INCLUDES = 
CommonFormWindows_FLAGS = /t:library
CommonFormWindows_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Common.dll
CommonFormWindows_LIBPATH = Library Library/../../Common
CommonFormWindows_LDFLAGS = 

#=============================================================================
# CommonFormWindowsTests variables
#=============================================================================
CommonFormWindowsTests_SOURCES = Tests/AssemblyInfo.cs Tests/CommonFormWindowsTests.cs
CommonFormWindowsTests_INCLUDES = 
CommonFormWindowsTests_FLAGS = /t:library
CommonFormWindowsTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Common.dll CommonFormWindows.dll System.Windows.Forms.dll
CommonFormWindowsTests_LIBPATH = Tests Tests/../../../tools/NUnit/bin Tests/../../Common Tests/../Library
CommonFormWindowsTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = CommonFormWindows.dll CommonFormWindowsTests.dll 
CLEAN_FILES = Check.xml CommonFormWindows.pdb CommonFormWindows.Doc.xml Check.xml CommonFormWindowsTests.pdb CommonFormWindowsTests.Doc.xml  CommonFormWindows.xml CommonFormWindows.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = CommonFormWindowsTests.dll 
STAGE_FILES = $(TARGETS) CommonFormWindows.Doc.xml CommonFormWindowsTests.Doc.xml CommonFormWindows.pdb CommonFormWindowsTests.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:CommonFormWindows.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# CommonFormWindows targets
#=============================================================================

# fix path seperator
CommonFormWindows_SOURCES := $(subst /,$(SEP), $(CommonFormWindows_SOURCES))
CommonFormWindows_INCLUDES := $(subst /,$(SEP), $(CommonFormWindows_INCLUDES))
CommonFormWindows_DEF := $(subst /,$(SEP), $(CommonFormWindows_DEF))
CommonFormWindows_LIBPATH := $(subst /,$(SEP), $(CommonFormWindows_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CommonFormWindows_LIBPATH) $(CommonFormWindows_INCLUDES)

CommonFormWindows.dll: $(CommonFormWindows_SOURCES) $(CommonFormWindows_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:CommonFormWindows.Doc.xml $(CSCFLAGS) $(CommonFormWindows_FLAGS) $(CommonFormWindows_LIBS:%=/r:%) $(CommonFormWindows_LIBPATH:%=/lib:%) $(CommonFormWindows_SOURCES)

#=============================================================================
# CommonFormWindowsTests targets
#=============================================================================

# fix path seperator
CommonFormWindowsTests_SOURCES := $(subst /,$(SEP), $(CommonFormWindowsTests_SOURCES))
CommonFormWindowsTests_INCLUDES := $(subst /,$(SEP), $(CommonFormWindowsTests_INCLUDES))
CommonFormWindowsTests_DEF := $(subst /,$(SEP), $(CommonFormWindowsTests_DEF))
CommonFormWindowsTests_LIBPATH := $(subst /,$(SEP), $(CommonFormWindowsTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CommonFormWindowsTests_LIBPATH) $(CommonFormWindowsTests_INCLUDES)

CommonFormWindowsTests.dll: $(CommonFormWindowsTests_SOURCES) $(CommonFormWindowsTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:CommonFormWindowsTests.Doc.xml $(CSCFLAGS) $(CommonFormWindowsTests_FLAGS) $(CommonFormWindowsTests_LIBS:%=/r:%) $(CommonFormWindowsTests_LIBPATH:%=/lib:%) $(CommonFormWindowsTests_SOURCES)

