WorkGroupIdentity_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/WorkGroupIdentity.cs
WorkGroupIdentity_CSFILES_CSC := $(subst /,$(SEP),$(WorkGroupIdentity_CSFILES))
WorkGroupIdentity_INCLUDES = 
WorkGroupIdentity_RESOURCES =
WorkGroupIdentity_FLAGS = $(CSC_LIBFLAG)
WorkGroupIdentity_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Identity.dll
WorkGroupIdentity_LIBPATH = ../Identity

NUNIT_TESTS := WorkGroupIdentityTests.dll

WorkGroupIdentityTests_CSFILES = $(srcdir)/WorkGroupIdentityTests/AssemblyInfo.cs $(srcdir)/WorkGroupIdentityTests/WorkGroupIdentityTests.cs
WorkGroupIdentityTests_CSFILES_CSC := $(subst /,$(SEP),$(WorkGroupIdentityTests_CSFILES))
WorkGroupIdentityTests_INCLUDES = 
WorkGroupIdentityTests_RESOURCES =
WorkGroupIdentityTests_FLAGS = $(CSC_LIBFLAG)
WorkGroupIdentityTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.tests.dll nunit.extensions.dll nunit.framework.dll Identity.dll WorkGroupIdentity.dll
WorkGroupIdentityTests_LIBPATH = WorkGroupIdentityTests ../Identity $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(WorkGroupIdentity_CSFILES) $(WorkGroupIdentityTests_CSFILES)

CUR_DIR := $(shell pwd)

all: WorkGroupIdentity.dll

WorkGroupIdentity.dll: $(WorkGroupIdentity_CSFILES) $(WorkGroupIdentity_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(WorkGroupIdentity_FLAGS) $(WorkGroupIdentity_LIBS:%=/r:%) $(WorkGroupIdentity_LIBPATH:%=/lib:%) $(WorkGroupIdentity_CSFILES_CSC) /doc:WorkGroupIdentity.doc.xml

WorkGroupIdentityTests.dll: $(WorkGroupIdentityTests_CSFILES) $(WorkGroupIdentityTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(WorkGroupIdentityTests_FLAGS) $(WorkGroupIdentityTests_LIBS:%=/r:%) $(WorkGroupIdentityTests_LIBPATH:%=/lib:%) $(WorkGroupIdentityTests_CSFILES_CSC)
	
install-exec-local: WorkGroupIdentity.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) WorkGroupIdentity.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f WorkGroupIdentity.dll WorkGroupIdentityTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:WorkGroupIdentityTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = WorkGroupIdentity.dll $(NUNIT_TESTS) WorkGroupIdentity.doc.xml WorkGroupIdentityTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

