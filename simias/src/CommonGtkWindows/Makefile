#=============================================================================
# Module:	GtkTraceWindow Makefile
#
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# CommonGtkWin variables
#=============================================================================
CommonGtkWin_SOURCES = GtkTraceWindow.cs
CommonGtkWin_INCLUDES = 
CommonGtkWin_RESOURCES =
CommonGtkWin_FLAGS = /t:library
CommonGtkWin_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll pango-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll Common.dll $(SYSTEM_XML)
CommonGtkWin_LIBPATH = ../Common /opt/gnome2/lib /usr/lib
CommonGtkWin_LDFLAGS = 

#=============================================================================
# InviteApp variables
#=============================================================================
ComGtkWinTests_SOURCES = GtkTraceWindowTests.cs
ComGtkWinTests_INCLUDES = 
ComGtkWinTests_RESOURCES = 
ComGtkWinTests_FLAGS = /t:exe
ComGtkWinTests_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) CommonGtkWindows.dll nunit.util.dll nunit.framework.dll Configuration.dll Common.dll
ComGtkWinTests_LIBPATH = /opt/gnome2/lib /usr/lib ../../tools/NUnit/bin ../Configuration ../Common
ComGtkWinTests_LDFLAGS = 
ComGtkWinTests_STAGE_FILES =

#=============================================================================
# globals
#=============================================================================

TARGETS = CommonGtkWindows.dll CommonGtkWindowsTests.exe
CLEAN_FILES = GtkTraceWindow.xml
NUNIT_TESTS = CommonGtkWindowsTests.exe
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:CommonGtkWindows.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES):	$(TARGETS)
	$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# Book targets
#=============================================================================

# fix path seperator
#CommonGtkWin_SOURCES := $(subst /,$(SEP), $(CommonGtkWin_SOURCES))
#CommonGtkWin_INCLUDES := $(subst /,$(SEP), $(CommonGtkWin_INCLUDES))
#CommonGtkWin_LIBPATH := $(subst /,$(SEP), $(CommonGtkWin_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CommonGtkWin_LIBPATH) $(CommonGtkWin_INCLUDES) $(ComGtkWinTests_LIBPATH) $(ComGtkWinTests_INCLUDES)

CommonGtkWindows.dll: $(CommonGtkWin_SOURCES) $(CommonGtkWin_RESOURCE) $(CommongtkWin_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:CommonGtkWin.xml $(CSCFLAGS) $(CommonGtkWin_FLAGS) $(CommonGtkWin_LIBS:%=/r:%) $(CommonGtkWin_LIBPATH:%=/lib:%) $(CommonGtkWin_SOURCES)

CommonGtkWindowsTests.exe: $(ComGtkWinTests_SOURCES) $(ComGtkWinTests_RESOURCE) $(ComGtkWinTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:ComGtkWinTests.xml $(CSCFLAGS) $(ComGtkWinTests_FLAGS) $(ComGtkWinTests_LIBS:%=/r:%) $(ComGtkWinTests_LIBPATH:%=/lib:%) $(ComGtkWinTests_SOURCES)
