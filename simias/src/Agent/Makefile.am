# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Agent_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/Agent.cs $(srcdir)/Library/IAgentService.cs
Agent_CSFILES_CSC := $(subst /,$(SEP),$(Agent_CSFILES))
Agent_INCLUDES = 
Agent_RESOURCES =
Agent_FLAGS = $(CSC_LIBFLAG)
Agent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll CollectionStore.dll
Agent_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../CollectionStore

#NUNIT_TESTS := AgentTests.dll

#AgentTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/AgentTests.cs
#AgentTests_CSFILES_CSC := $(subst /,$(SEP),$(AgentTests_CSFILES))
#AgentTests_INCLUDES = 
#AgentTests_RESOURCES =
#AgentTests_FLAGS = $(CSC_LIBFLAG)
#AgentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Agent.dll
#AgentTests_LIBPATH = $(NUNIT_LIBPATH)

EXTRA_DIST = $(Agent_CSFILES)
#EXTRA_DIST = $(Agent_CSFILES) $(AgentTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Agent.dll

Agent.dll: $(Agent_CSFILES) $(Agent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Agent_FLAGS) $(Agent_LIBS:%=/r:%) $(Agent_LIBPATH:%=/lib:%) $(Agent_CSFILES_CSC) /doc:Agent.doc.xml

#AgentTests.dll: $(AgentTests_CSFILES) $(AgentTests_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(AgentTests_FLAGS) $(AgentTests_LIBS:%=/r:%) $(AgentTests_LIBPATH:%=/lib:%) $(AgentTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Agent.pdb
#DEBUG_CHECK_FILES = AgentTests.pdb
endif
endif

install-exec-local: Agent.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Agent.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)
        
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Agent.dll AgentTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
	
#installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AgentTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Agent.dll $(NUNIT_TESTS) Agent.doc.xml AgentTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

