# configure makefile
include ../config.mk

#=============================================================================
# Sync variables
#=============================================================================
Sync_SOURCES := Synker.cs SyncPoint.cs SyncServ.cs SyncPass.cs SyncOps.cs Dredge.cs SyncLog.cs
Sync_INCLUDES := 
Sync_FLAGS := /t:library
Sync_LIBS := Common.dll CollectionEvents.dll CollectionStore.dll Identity.dll Configuration.dll SyncCommon.dll Agent.dll System.dll System.Data.dll System.Runtime.Remoting.dll
Sync_LIBPATH := ../Common ../SyncCommon ../Agent ../CollectionEvents ../CollectionStore ../StoreProvider ../Identity ../FsProvider ../WorkGroupIdentity ../Configuration ../SqliteProvider ../../external/sqlite/linux
Sync_LDFLAGS := 

#=============================================================================
# SyncCmd variables
#=============================================================================
SyncCmd_SOURCES := SyncCmd.cs
SyncCmd_INCLUDES := 
SyncCmd_FLAGS := /t:exe
SyncCmd_LIBS := Sync.dll $(Sync_LIBS)
SyncCmd_LIBPATH := $(Sync_LIBPATH)
SyncCmd_LDFLAGS := 

#=============================================================================
# TestSync variables
#=============================================================================
TestSync_SOURCES := TestSync.cs
TestSync_INCLUDES := 
TestSync_FLAGS := /t:exe
TestSync_LIBS := System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Sync.dll CollectionStore.dll Identity.dll
TestSync_LIBPATH := $(Sync_LIBPATH) ../../tools/NUnit/bin ../../tools/NUnit/bin
TestSync_LDFLAGS := 

#=============================================================================
# SyncTests variables
#=============================================================================
SyncTests_SOURCES := SyncTestStore.cs SyncTestCollection.cs Sync01Tests.cs Sync02Tests.cs
SyncTests_INCLUDES := 
SyncTests_FLAGS := /t:library
SyncTests_LIBS := Sync.dll System.dll System.Data.dll CollectionStore.dll Identity.dll System.Runtime.Remoting.dll nunit.util.dll nunit.framework.dll
SyncTests_LIBPATH := $(Sync_LIBPATH) ../../tools/NUnit/bin
SyncTests_LDFLAGS := 

#=============================================================================
# globals
#=============================================================================

NUNIT_TESTS := TestSync.exe
#NUNIT_TESTS := TestSync.exe SyncTests.dll 
TARGETS := Sync.dll SyncCmd.exe $(NUNIT_TESTS)
CLEAN_FILES = Sync.xml SyncCmd.xml TestSync.xml
STAGE_FILES := $(TARGETS) Sync.pdb TestSync.pdb
STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

export MONO_PATH:=$(subst $(SPACE),:,$(strip $(TestSync_LIBPATH)))

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

all: $(TARGETS) $(STAGE_FILES)

clean:
	-$(RM) $(TARGETS) $(CLEAN_FILES) $(STAGE_FILES)

test:
	-$(RMDIR) tmp*
	$(NUNIT) $(NUNIT_FLAGS) /xml:Sync.TestOutput.xml $(NUNIT_TESTS)

$(STAGE_FILES): $(TARGETS)
	-$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# Sync targets
#=============================================================================

# fix path seperator
Sync_SOURCES := $(subst /,$(SEP), $(Sync_SOURCES))
Sync_INCLUDES := $(subst /,$(SEP), $(Sync_INCLUDES))
Sync_LIBPATH := $(subst /,$(SEP), $(Sync_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Sync_LIBPATH) $(Sync_INCLUDES)

Sync.dll: $(Sync_SOURCES) $(Sync_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:Sync.xml $(CSCFLAGS) $(Sync_FLAGS) $(Sync_LIBS:%=/r:%) $(Sync_LIBPATH:%=/lib:%) $(Sync_SOURCES)

#=============================================================================
# SyncCmd targets
#=============================================================================

# fix path seperator
SyncCmd_SOURCES := $(subst /,$(SEP), $(SyncCmd_SOURCES))
SyncCmd_INCLUDES := $(subst /,$(SEP), $(SyncCmd_INCLUDES))
SyncCmd_LIBPATH := $(subst /,$(SEP), $(SyncCmd_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncCmd_LIBPATH) $(SyncCmd_INCLUDES)

SyncCmd.exe: $(SyncCmd_SOURCES) $(SyncCmd_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:SyncCmd.xml $(CSCFLAGS) $(SyncCmd_FLAGS) $(SyncCmd_LIBS:%=/r:%) $(SyncCmd_LIBPATH:%=/lib:%) $(SyncCmd_SOURCES)

#=============================================================================
# TestSync targets
#=============================================================================

# fix path seperator
TestSync_SOURCES := $(subst /,$(SEP), $(TestSync_SOURCES))
TestSync_INCLUDES := $(subst /,$(SEP), $(TestSync_INCLUDES))
TestSync_LIBPATH := $(subst /,$(SEP), $(TestSync_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(TestSync_LIBPATH) $(TestSync_INCLUDES)

TestSync.exe: $(TestSync_SOURCES) $(TestSync_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:TestSync.xml $(CSCFLAGS) $(TestSync_FLAGS) $(TestSync_LIBS:%=/r:%) $(TestSync_LIBPATH:%=/lib:%) $(TestSync_SOURCES)

#=============================================================================
# SyncTests targets
#=============================================================================

# fix path seperator
SyncTests_SOURCES := $(subst /,$(SEP), $(SyncTests_SOURCES))
SyncTests_INCLUDES := $(subst /,$(SEP), $(SyncTests_INCLUDES))
SyncTests_LIBPATH := $(subst /,$(SEP), $(SyncTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncTests_LIBPATH) $(SyncTests_INCLUDES)

SyncTests.dll: $(SyncTests_SOURCES) $(SyncTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:SyncTests.xml $(CSCFLAGS) $(SyncTests_FLAGS) $(SyncTests_LIBS:%=/r:%) $(SyncTests_LIBPATH:%=/lib:%) $(SyncTests_SOURCES)

