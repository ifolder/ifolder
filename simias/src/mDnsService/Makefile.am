# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

mDnsService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/mDnsService.cs 
mDnsService_CSFILES_CSC := $(subst /,$(SEP),$(mDnsService_CSFILES))
mDnsService_INCLUDES = 
mDnsService_RESOURCES =
mDnsService_FLAGS = $(CSC_EXEFLAG)
mDnsService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll mDnsResponder.dll
mDnsService_LIBPATH = ../Simias.dll $(MDNSRESPONDER_DIR)

PROJECT_FILES = $(srcdir)/mDnsService.csproj

EXTRA_DIST = $(mDnsService_CSFILES) $(PROJECT_FILES)

CUR_DIR := $(shell pwd)

all: mDnsService.exe

mDnsService.exe: $(mDnsService_CSFILES) $(mDnsService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(mDnsService_FLAGS) $(mDnsService_LIBS:%=/r:%) $(mDnsService_LIBPATH:%=/lib:%) $(mDnsService_CSFILES_CSC) /doc:mDnsService.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = mDnsService.pdb
endif
endif

install-exec-local: mDnsService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) mDnsService.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(bindir)
endif
endif
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f mDnsService.exe $(DEBUG_FILES)
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(bindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif
	
CLEAN_FILES = mDnsService.exe $(DEBUG_FILES) 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

