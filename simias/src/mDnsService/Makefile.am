# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

mDnsService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/mDnsService.cs 
Service_CSFILES_CSC := $(subst /,$(SEP),$(mDnsService_CSFILES))
Service_INCLUDES = 
Service_RESOURCES =
Service_FLAGS = $(CSC_EXEFLAG)
Service_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll mDnsResponder.dll
Service_LIBPATH = ../Simias.dll $(MDNSRESPONDER_DIR)

EXTRA_DIST = $(mDnsService_CSFILES)

CUR_DIR := $(shell pwd)

all: Simias.Service.mDnsService.exe

Simias.Service.mDnsService.exe: $(Service_CSFILES) $(Service_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Service_FLAGS) $(Service_LIBS:%=/r:%) $(Service_LIBPATH:%=/lib:%) $(Service_CSFILES_CSC) /doc:Simias.Service.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Service.mDnsService.pdb
endif
endif

install-exec-local: Simias.Service.mDnsService.exe
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Simias.Service.mDnsService.exe $(DEBUG_FILES) $(DESTDIR)$(libdir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Simias.Service.mDnsService.exe $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(libdir)
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ServiceTest.test.xml $(NUNIT_TESTS)
	
	
CLEAN_FILES = Simias.Service.mDnsService.exe 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

