# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Service_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/mDnsService.cs 
Service_CSFILES := $(subst /,$(SEP),$(Service_CSFILES1))
Service_INCLUDES = 
Service_RESOURCES =
Service_FLAGS = $(CSC_EXEFLAG)
Service_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll mDnsResponder.dll
Service_LIBPATH = ../Simias.dll $(MDNSRESPONDER_DIR)

EXTRA_DIST = $(Service_CSFILES)

CUR_DIR := $(shell pwd)

all: Simias.Service.mDnsService.exe

Simias.Service.mDnsService.exe: $(Service_CSFILES) $(Service_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Service_FLAGS) $(Service_LIBS:%=/r:%) $(Service_LIBPATH:%=/lib:%) $(Service_CSFILES) /doc:Simias.Service.mDnsService.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Service.mDnsService.pdb
endif
endif

install-exec-local: Simias.Service.mDnsService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) --mode=644 Simias.Service.mDnsService.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Simias.Service.mDnsService.exe $(DEBUG_FILES)
	
CLEAN_FILES = Simias.Service.mDnsService.exe 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

