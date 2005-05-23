# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

POBoxWS_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/POBoxService.asmx.cs
POBoxWS_CSFILES := $(subst /,$(SEP),$(POBoxWS_CSFILES1))
POBoxWS_INCLUDES =
POBoxWS_RESOURCES =
POBoxWS_FLAGS = $(CSC_LIBFLAG)
POBoxWS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll Simias.dll
POBoxWS_LIBPATH = ../SimiasClient ../Simias.dll

EXTRA_DIST = POService.asmx $(POBoxWS_CSFILES1) AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.POBox.Web.dll

Simias.POBox.Web.dll: $(POBoxWS_CSFILES) $(POBoxWS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(POBoxWS_FLAGS) $(SIMIAS_LIBS) $(POBoxWS_LIBS:%=/r:%) $(POBoxWS_LIBPATH:%=/lib:%) $(POBoxWS_RESOURCES:%=$(ICON_FLAG)%) $(POBoxWS_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.POBox.Web.pdb
endif
endif

install-exec-local: Simias.POBox.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) POService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.POBox.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f  POService.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.POBox.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.POBox.Web.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

