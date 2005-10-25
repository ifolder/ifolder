# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SyncService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SyncHandler.ashx.cs
SyncService_CSFILES_CSC := $(subst /,$(SEP),$(SyncService_CSFILES))
SyncService_INCLUDES =
SyncService_RESOURCES =
SyncService_FLAGS = $(CSC_LIBFLAG)
SyncService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll SimiasLib.dll
SyncService_LIBPATH = ../SimiasClient ../SimiasLib.dll

EXTRA_DIST = SyncHandler.ashx $(SyncService_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SyncService.Web.dll

SyncService.Web.dll: AssemblyInfo.cs $(SyncService_CSFILES) $(SyncService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncService_FLAGS) $(SIMIAS_LIBS) $(SyncService_LIBS:%=/r:%) $(SyncService_LIBPATH:%=/lib:%) $(SyncService_RESOURCES:%=$(ICON_FLAG)%) $(SyncService_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = SyncService.Web.pdb
endif
endif

install-exec-local: SyncService.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SyncHandler.ashx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) SyncService.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f SyncHandler.ashx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f SyncService.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = SyncService.Web.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

