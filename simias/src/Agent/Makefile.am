Agent_CSFILES = $(srcdir)/Library/AgentFactory.cs $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/IInviteAgent.cs $(srcdir)/Library/Invitation.cs
Agent_CSFILES_CSC := $(subst /,$(SEP),$(Agent_CSFILES))
Agent_INCLUDES = 
Agent_RESOURCES =
Agent_FLAGS = $(CSC_LIBFLAG)
Agent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Identity.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll
Agent_LIBPATH = ../Configuration ../Identity ../StoreProvider ../CollectionEvents ../CollectionStore Library

NUNIT_TESTS := AgentTests.dll

AgentTests_CSFILES = $(srcdir)/Tests/AgentTests.cs $(srcdir)/Tests/AssemblyInfo.cs
AgentTests_CSFILES_CSC := $(subst /,$(SEP),$(AgentTests_CSFILES))
AgentTests_INCLUDES = 
AgentTests_RESOURCES =
AgentTests_FLAGS = $(CSC_LIBFLAG)
AgentTests_LIBS = $(Agent_LIBS) $(SYSTEM_XML) Common.dll Agent.dll nunit.framework.dll
AgentTests_LIBPATH = $(Agent_LIBPATH) ../Common Tests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Agent_CSFILES) $(AgentTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Agent.dll

Agent.dll: $(Agent_CSFILES) $(Agent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Agent_FLAGS) $(Agent_LIBS:%=/r:%) $(Agent_LIBPATH:%=/lib:%) $(Agent_CSFILES_CSC) /doc:Agent.xml

AgentTests.dll: $(AgentTests_CSFILES) $(AgentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(AgentTests_FLAGS) $(AgentTests_LIBS:%=/r:%) $(AgentTests_LIBPATH:%=/lib:%) $(AgentTests_CSFILES_CSC)
	
install-exec-local: Agent.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Agent.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Agent.dll AgentTests.xml invitation.ifi $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:AgentTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Agent.dll $(NUNIT_TESTS) Agent.xml AgentTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

