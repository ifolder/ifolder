CollectionEvents_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/EventBroker.cs $(srcdir)/EventPublisher.cs $(srcdir)/EventSubscriber.cs $(srcdir)/CollectionEventArgs.cs $(srcdir)/FileEventArgs.cs $(srcdir)/ServiceEventArgs.cs $(srcdir)/ServiceEventSubscriber.cs $(srcdir)/CollectionRootChangedEventArgs.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/FileRenameEventArgs.cs
CollectionEvents_CSFILES_CSC := $(subst /,$(SEP),$(CollectionEvents_CSFILES))
CollectionEvents_INCLUDES = 
CollectionEvents_RESOURCES =
CollectionEvents_FLAGS = $(CSC_LIBFLAG)
CollectionEvents_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll Common.dll
CollectionEvents_LIBPATH = /opt/gnome2/lib /usr/lib ../Configuration ../Common

CsEventBroker_CSFILES = $(srcdir)/CsEventBroker/AssemblyInfo.cs $(srcdir)/CsEventBroker/CsEventBroker.cs
CsEventBroker_CSFILES_CSC := $(subst /,$(SEP),$(CsEventBroker_CSFILES))
CsEventBroker_INCLUDES = 
CsEventBroker_RESOURCES =
CsEventBroker_FLAGS = $(CSC_EXEFLAG)
CsEventBroker_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll Common.dll CollectionEvents.dll
CsEventBroker_LIBPATH = /opt/gnome2/lib /usr/lib ../Configuration ../Common

NUNIT_TESTS := CollectionEventTests.exe

CollectionEventTests_CSFILES = $(srcdir)/CollectionEventsTests/AssemblyInfo.cs $(srcdir)/CollectionEventsTests/CollectionEventsTests.cs
CollectionEventTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionEventTests_CSFILES))
CollectionEventTests_INCLUDES = 
CollectionEventTests_RESOURCES =
CollectionEventTests_FLAGS = $(CSC_EXEFLAG)
CollectionEventTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Configuration.dll Common.dll CollectionEvents.dll
CollectionEventTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin ../Common ../Configuration 

EXTRA_DIST = $(CollectionEvents_CSFILES) $(CsEventBroker_CSFILES) $(CollectionEventTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CollectionEvents.dll CsEventBroker.exe

CollectionEvents.dll: $(CollectionEvents_CSFILES) $(CollectionEvents_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionEvents_FLAGS) $(CollectionEvents_LIBS:%=/r:%) $(CollectionEvents_LIBPATH:%=/lib:%) $(CollectionEvents_CSFILES_CSC) /doc:CollectionEvents.doc.xml

CsEventBroker.exe: $(CsEventBroker_CSFILES) $(CsEventBroker_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CsEventBroker_FLAGS) $(CsEventBroker_LIBS:%=/r:%) $(CsEventBroker_LIBPATH:%=/lib:%) $(CsEventBroker_CSFILES_CSC) /doc:CsEventBroker.doc.xml

CollectionEventTests.exe: $(CollectionEventTests_CSFILES) $(CollectionEventTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionEventTests_FLAGS) $(CollectionEventTests_LIBS:%=/r:%) $(CollectionEventTests_LIBPATH:%=/lib:%) $(CollectionEventTests_CSFILES_CSC)
	
install-exec-local: CollectionEvents.dll CsEventBroker.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CollectionEvents.dll CsEventBroker.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CollectionEvents.dll CsEventBroker.exe CollectionEventTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionEventTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CollectionEvents.dll CsEventBroker.exe $(NUNIT_TESTS) CollectionEvents.doc.xml CsEventBroker.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

