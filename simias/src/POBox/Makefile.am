# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

POService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/POService.cs $(srcdir)/POManager.cs $(srcdir)/POBoxManager.cs $(srcdir)/SubscriptionThread.cs
POService_CSFILES_CSC := $(subst /,$(SEP),$(POService_CSFILES))
POService_INCLUDES = 
POService_RESOURCES =
POService_FLAGS = $(CSC_EXEFLAG)
POService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll
POService_LIBPATH = ../Simias.dll

EXTRA_DIST = $(POService_CSFILES)

CUR_DIR := $(shell pwd)

all: POService.exe

POService.exe: $(POService_CSFILES) $(POService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(POService_FLAGS) $(POService_LIBS:%=/r:%) $(POService_LIBPATH:%=/lib:%) $(POService_CSFILES_CSC) /doc:POService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = POService.pdb
DEBUG_CHECK_FILES = 
endif
endif

install-exec-local: POService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) POService.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f POService.exe $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
CLEAN_FILES = POService.exe POService.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

