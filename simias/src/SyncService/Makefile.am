# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncService_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/SyncService.cs $(srcdir)/SyncService.asmx.cs $(srcdir)/ServerFile.cs
SyncService_CSFILES := $(subst /,$(SEP),$(SyncService_CSFILES1))
SyncService_INCLUDES =
SyncService_RESOURCES =
SyncService_FLAGS = $(CSC_LIBFLAG)
SyncService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll Simias.dll
SyncService_LIBPATH = ../SimiasClient ../Simias.dll

EXTRA_DIST = SyncService.asmx $(SyncService_CSFILES)

all: SyncService.Web.dll

SyncService.Web.dll: $(SyncService_CSFILES) $(SyncService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncService_FLAGS) $(SIMIAS_LIBS) $(SyncService_LIBS:%=/r:%) $(SyncService_LIBPATH:%=/lib:%) $(SyncService_RESOURCES:%=$(ICON_FLAG)%) $(SyncService_CSFILES) /doc:SyncService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = SyncService.Web.pdb
endif
endif

install-exec-local: SyncService.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SyncService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) SyncService.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f SyncService.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f SyncService.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = SyncService.Web.dll SyncService.Web.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

