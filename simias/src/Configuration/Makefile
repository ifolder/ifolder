#=============================================================================
# Module:	Configuration Makefile
#
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# Configuration variables
#=============================================================================
Configuration_SOURCES = Configuration.cs
Configuration_INCLUDES = 
Configuration_RESOURCES =
Configuration_FLAGS = /t:library
Configuration_LIBS = System.dll $(SYSTEM_XML)
Configuration_LIBPATH = /opt/gnome2/lib /usr/lib
Configuration_LDFLAGS = 

#=============================================================================
# ConfigurationTests variables
#=============================================================================
ConfTests_SOURCES = ConfTests/ConfTests.cs
ConfTests_INCLUDES = 
ConfTests_FLAGS = /t:exe
ConfTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Configuration.dll
ConfTests_LIBPATH = ConfTests ../../tools/NUnit/bin
ConfTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = Configuration.dll ConfTests.exe
CLEAN_FILES = default.conf Configuration.xml ConfTests.xml
NUNIT_TESTS = ConfTests.exe
# for now we are including libegg.so and egg-sharp.dll until this gets
# distributed with gtk-sharp
STAGE_FILES = $(TARGETS)
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
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))

# all
all: $(TARGETS) $(STAGE_FILES)

# clean
clean:
	$(RM) $(TARGETS) $(CLEAN_FILES) $(STAGE_FILES)

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:ConfTests.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# TrayAppGtk targets
#=============================================================================

# fix path seperator
Configuration_SOURCES := $(subst /,$(SEP), $(Configuration_SOURCES))
Configuration_INCLUDES := $(subst /,$(SEP), $(Configuration_INCLUDES))
Configuration_LIBPATH := $(subst /,$(SEP), $(Configuration_LIBPATH))

ConfTests_SOURCES := $(subst /,$(SEP), $(ConfTests_SOURCES))
ConfTests_INCLUDES := $(subst /,$(SEP), $(ConfTests_INCLUDES))
ConfTests_LIBPATH := $(subst /,$(SEP), $(ConfTests_LIBPATH))
# update VPATH

VPATH := $(VPATH) $(Configuration_LIBPATH) $(Configuration_INCLUDES) $(ConfTests_LIBPATH) $(ConfTests_INCLUDES)

Configuration.dll: $(Configuration_SOURCES) $(Configuration_RESOURCE) $(Configuration_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Configuration.xml $(CSCFLAGS) $(Configuration_FLAGS) $(Configuration_LIBS:%=/r:%) $(Configuration_LIBPATH:%=/lib:%) $(Configuration_SOURCES)

ConfTests.exe: $(ConfTests_SOURCES) $(ConfTests_RESOURCE) $(ConfTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:ConfTests.xml $(CSCFLAGS) $(ConfTests_FLAGS) $(ConfTests_LIBS:%=/r:%) $(ConfTests_LIBPATH:%=/lib:%) $(ConfTests_SOURCES)
