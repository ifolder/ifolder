# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

CollectionEvents_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/EventBroker.cs $(srcdir)/EventPublisher.cs $(srcdir)/EventSubscriber.cs $(srcdir)/CollectionEventArgs.cs $(srcdir)/FileEventArgs.cs $(srcdir)/CollectionRootChangedEventArgs.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/FileRenameEventArgs.cs
CollectionEvents_CSFILES_CSC := $(subst /,$(SEP),$(CollectionEvents_CSFILES))
CollectionEvents_INCLUDES = 
CollectionEvents_RESOURCES =
CollectionEvents_FLAGS = $(CSC_LIBFLAG)
CollectionEvents_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Common.dll
CollectionEvents_LIBPATH = /opt/gnome2/lib /usr/lib ../Common

EventService_CSFILES = $(srcdir)/EventService/AssemblyInfo.cs $(srcdir)/EventService/EventService.cs
EventService_CSFILES_CSC := $(subst /,$(SEP),$(EventService_CSFILES))
EventService_INCLUDES = 
EventService_RESOURCES =
EventService_FLAGS = $(CSC_LIBFLAG)
EventService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Common.dll CollectionEvents.dll Simias.Service.dll
EventService_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../Service

NUNIT_TESTS := CollectionEventTests.exe

CollectionEventTests_CSFILES = $(srcdir)/CollectionEventsTests/AssemblyInfo.cs $(srcdir)/CollectionEventsTests/CollectionEventsTests.cs
CollectionEventTests_CSFILES_CSC := $(subst /,$(SEP),$(CollectionEventTests_CSFILES))
CollectionEventTests_INCLUDES = 
CollectionEventTests_RESOURCES =
CollectionEventTests_FLAGS = $(CSC_EXEFLAG)
CollectionEventTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Common.dll CollectionEvents.dll Simias.Service.dll
CollectionEventTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin ../Common ../Service 

EXTRA_DIST = $(CollectionEvents_CSFILES) $(EventService_CSFILES) $(CollectionEventTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CollectionEvents.dll EventService.exe

CollectionEvents.dll: $(CollectionEvents_CSFILES) $(CollectionEvents_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionEvents_FLAGS) $(CollectionEvents_LIBS:%=/r:%) $(CollectionEvents_LIBPATH:%=/lib:%) $(CollectionEvents_CSFILES_CSC) /doc:CollectionEvents.doc.xml

EventService.exe: $(EventService_CSFILES) $(EventService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(EventService_FLAGS) $(EventService_LIBS:%=/r:%) $(EventService_LIBPATH:%=/lib:%) $(EventService_CSFILES_CSC) /doc:EventService.doc.xml

CollectionEventTests.exe: $(CollectionEventTests_CSFILES) $(CollectionEventTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionEventTests_FLAGS) $(CollectionEventTests_LIBS:%=/r:%) $(CollectionEventTests_LIBPATH:%=/lib:%) $(CollectionEventTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = CollectionEvents.pdb EventService.pdb
DEBUG_CHECK_FILES = CollectionEventTests.pdb
endif
endif

install-exec-local: CollectionEvents.dll EventService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CollectionEvents.dll EventService.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CollectionEvents.dll EventService.exe CollectionEventTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionEventTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CollectionEvents.dll EventService.exe $(NUNIT_TESTS) CollectionEvents.doc.xml EventService.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

