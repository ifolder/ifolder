# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SubscribeWS_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/SubscribeService.asmx.cs
SubscribeWS_CSFILES := $(subst /,$(SEP),$(SubscribeWS_CSFILES1))
SubscribeWS_INCLUDES =
SubscribeWS_RESOURCES =
SubscribeWS_FLAGS = $(CSC_LIBFLAG)
SubscribeWS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll
SubscribeWS_LIBPATH = ../Simias.dll

EXTRA_DIST = SubscribeService.asmx $(SubscribeWS_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.Subscribe.Web.dll

Simias.Subscribe.Web.dll: $(SubscribeWS_CSFILES) $(SubscribeWS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SubscribeWS_FLAGS) $(SIMIAS_LIBS) $(SubscribeWS_LIBS:%=/r:%) $(SubscribeWS_LIBPATH:%=/lib:%) $(SubscribeWS_RESOURCES:%=$(ICON_FLAG)%) $(SubscribeWS_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Subscribe.Web.pdb
endif
endif

install-exec-local: Simias.Subscribe.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SubscribeService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.Subscribe.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f  SubscribeService.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.Subscribe.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Subscribe.Web.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

