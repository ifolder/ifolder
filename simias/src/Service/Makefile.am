# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Service_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/BaseProcessService.cs $(srcdir)/IThreadService.cs $(srcdir)/Manager.cs $(srcdir)/Message.cs $(srcdir)/ProcessServiceCtl.cs $(srcdir)/ServiceCtl.cs $(srcdir)/ServiceType.cs $(srcdir)/State.cs $(srcdir)/ThreadServiceCtl.cs
Service_CSFILES_CSC := $(subst /,$(SEP),$(Service_CSFILES))
Service_INCLUDES =
Service_RESOURCES =
Service_FLAGS = $(CSC_LIBFLAG)
Service_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll
Service_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../CollectionEvents

NUNIT_TESTS := ServiceTests.exe ThreadServiceTest.dll ProcessServiceTest.exe

ServiceTests_CSFILES = $(srcdir)/ServiceTests/AssemblyInfo.cs $(srcdir)/ServiceTests/ServiceTests.cs
ServiceTests_CSFILES_CSC := $(subst /,$(SEP),$(ServiceTests_CSFILES))
ServiceTests_INCLUDES =
ServiceTests_RESOURCES =
ServiceTests_FLAGS = $(CSC_EXEFLAG)
ServiceTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Common.dll Simias.Service.dll
ServiceTests_LIBPATH = $(NUNIT_LIBPATH) ../Common

ThreadServiceTest_CSFILES = $(srcdir)/ThreadServiceTest/AssemblyInfo.cs $(srcdir)/ThreadServiceTest/ThreadServiceTest.cs
ThreadServiceTest_CSFILES_CSC := $(subst /,$(SEP),$(ThreadServiceTest_CSFILES))
ThreadServiceTest_INCLUDES =
ThreadServiceTest_RESOURCES =
ThreadServiceTest_FLAGS = $(CSC_LIBFLAG)
ThreadServiceTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Common.dll Simias.Service.dll
ThreadServiceTest_LIBPATH = $(NUNIT_LIBPATH) ../Common

ProcessServiceTest_CSFILES = $(srcdir)/ProcessServiceTest/AssemblyInfo.cs $(srcdir)/ProcessServiceTest/ProcessServiceTest.cs
ProcessServiceTest_CSFILES_CSC := $(subst /,$(SEP),$(ProcessServiceTest_CSFILES))
ProcessServiceTest_INCLUDES =
ProcessServiceTest_RESOURCES =
ProcessServiceTest_FLAGS = $(CSC_EXEFLAG)
ProcessServiceTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Common.dll Simias.Service.dll
ProcessServiceTest_LIBPATH = $(NUNIT_LIBPATH) ../Common

EXTRA_DIST = $(Service_CSFILES) $(ServiceTests_CSFILES) $(ThreadServiceTest_CSFILES) $(ProcessServiceTest_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: Simias.Service.dll

Simias.Service.dll: $(Service_CSFILES) $(Service_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Service_FLAGS) $(Service_LIBS:%=/r:%) $(Service_LIBPATH:%=/lib:%) $(Service_CSFILES_CSC) $(DOC_FILE_CMD)
	
ServiceTests.exe: $(ServiceTests_CSFILES) $(ServiceTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ServiceTests_FLAGS) $(ServiceTests_LIBS:%=/r:%) $(ServiceTests_LIBPATH:%=/lib:%) $(ServiceTests_CSFILES_CSC)
	
ProcessServiceTest.exe: $(ProcessServiceTest_CSFILES) $(ProcessServiceTest_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ProcessServiceTest_FLAGS) $(ProcessServiceTest_LIBS:%=/r:%) $(ProcessServiceTest_LIBPATH:%=/lib:%) $(ProcessServiceTest_CSFILES_CSC)
	
ThreadServiceTest.dll: $(ThreadServiceTest_CSFILES) $(ThreadServiceTest_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ThreadServiceTest_FLAGS) $(ThreadServiceTest_LIBS:%=/r:%) $(ThreadServiceTest_LIBPATH:%=/lib:%) $(ThreadServiceTest_CSFILES_CSC)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Service.pdb
endif
endif

install-exec-local: Simias.Service.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.Service.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Simias.Service.dll ServiceTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ServiceTest.test.xml $(NUNIT_TESTS)
	
	
CLEAN_FILES = Simias.Service.dll $(NUNIT_TESTS) $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

