# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SyncManagerService.cs
SyncService_CSFILES_CSC := $(subst /,$(SEP),$(SyncService_CSFILES))
SyncService_INCLUDES = 
SyncService_RESOURCES =
SyncService_FLAGS = $(CSC_EXEFLAG)
SyncService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll
SyncService_LIBPATH = ../Simias.dll

EXTRA_DIST = $(SyncService_CSFILES) $(srcdir)/remoting.config

all: SyncService.exe

SyncService.exe: $(SyncService_CSFILES) $(SyncService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncService_FLAGS) $(SyncService_LIBS:%=/r:%) $(SyncService_LIBPATH:%=/lib:%) $(SyncService_CSFILES_CSC) /doc:SyncService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = SyncService.pdb
DEBUG_CHECK_FILES = 
endif
endif

install-exec-local: SyncService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) SyncService.exe $(DEBUG_FILES) $(srcdir)/remoting.config $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SyncService.exe remoting.config $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
CLEAN_FILES = SyncService.exe SyncService.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

