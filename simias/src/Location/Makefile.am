# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Location_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/LocationService.cs $(srcdir)/Library/ILocationProvider.cs $(srcdir)/Library/DefaultLocationProvider.cs
Location_CSFILES_CSC := $(subst /,$(SEP),$(Location_CSFILES))
Location_INCLUDES = 
Location_RESOURCES =
Location_FLAGS = $(CSC_LIBFLAG)
Location_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll
Location_LIBPATH = /opt/gnome2/lib /usr/lib ../Common

NUNIT_TESTS := LocationTests.dll

LocationTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/LocationTests.cs
LocationTests_CSFILES_CSC := $(subst /,$(SEP),$(LocationTests_CSFILES))
LocationTests_INCLUDES = 
LocationTests_RESOURCES =
LocationTests_FLAGS = $(CSC_LIBFLAG)
LocationTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Location.dll
LocationTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Location_CSFILES) $(LocationTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Location.dll

Location.dll: $(Location_CSFILES) $(Location_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Location_FLAGS) $(Location_LIBS:%=/r:%) $(Location_LIBPATH:%=/lib:%) $(Location_CSFILES_CSC) /doc:Location.doc.xml

LocationTests.dll: $(LocationTests_CSFILES) $(LocationTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(LocationTests_FLAGS) $(LocationTests_LIBS:%=/r:%) $(LocationTests_LIBPATH:%=/lib:%) $(LocationTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Location.pdb
DEBUG_CHECK_FILES = LocationTests.pdb
endif
endif

install-exec-local: Location.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Location.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Location.dll LocationTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:LocationTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Location.dll $(NUNIT_TESTS) Location.doc.xml LocationTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

