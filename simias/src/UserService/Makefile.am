# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UserService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/UserService.cs $(srcdir)/SystemManager.cs $(srcdir)/ICollectionWorker.cs
UserService_CSFILES_CSC := $(subst /,$(SEP),$(UserService_CSFILES))
UserService_INCLUDES = 
UserService_RESOURCES =
UserService_FLAGS = $(CSC_LIBFLAG)
UserService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Common.dll CollectionEvents.dll
UserService_LIBPATH = /opt/gnome2/lib /usr/lib ../Configuration ../Common ../CollectionEvents

NUNIT_TESTS := ServiceTests.exe

ServiceTests_CSFILES = $(srcdir)/ServiceTests/AssemblyInfo.cs $(srcdir)/ServiceTests/ServiceTests.cs
ServiceTests_CSFILES_CSC := $(subst /,$(SEP),$(ServiceTests_CSFILES))
ServiceTests_INCLUDES = 
ServiceTests_RESOURCES =
ServiceTests_FLAGS = $(CSC_EXEFLAG)
ServiceTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Configuration.dll Common.dll UserService.dll
ServiceTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin ../Common ../Configuration 



EXTRA_DIST = $(UserService_CSFILES) $(ServiceTests_CSFILES) 

CUR_DIR := $(shell pwd)

all: UserService.dll

UserService.dll: $(UserService_CSFILES) $(UserService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UserService_FLAGS) $(UserService_LIBS:%=/r:%) $(UserService_LIBPATH:%=/lib:%) $(UserService_CSFILES_CSC) /doc:UserService.doc.xml
	
ServiceTests.exe: $(ServiceTests_CSFILES) $(ServiceTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ServiceTests_FLAGS) $(ServiceTests_LIBS:%=/r:%) $(ServiceTests_LIBPATH:%=/lib:%) $(ServiceTests_CSFILES_CSC)
	

if DEBUG
if WINDOWS
DEBUG_FILES = UserService.pdb
endif
endif

install-exec-local: UserService.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) UserService.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f UserService.dll ServiceTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ServiceTest.test.xml $(NUNIT_TESTS)
	
	
CLEAN_FILES = UserService.dll $(NUNIT_TESTS) UserService.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

