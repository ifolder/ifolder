# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasWebService_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/Global.asax.cs $(srcdir)/Simias.asmx.cs $(srcdir)/SimiasBrowser.asmx.cs
SimiasWebService_CSFILES := $(subst /,$(SEP),$(SimiasWebService_CSFILES1))
SimiasWebService_INCLUDES =
SimiasWebService_RESOURCES =
SimiasWebService_FLAGS = $(CSC_LIBFLAG)
SimiasWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll Simias.dll
SimiasWebService_LIBPATH = ../SimiasClient ../Simias.dll

EXTRA_DIST = web.config Global.asax Simias.asmx SimiasBrowser.asmx $(SimiasWebService_CSFILES)

all: Simias.Web.dll

Simias.Web.dll: $(SimiasWebService_CSFILES) $(SimiasWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasWebService_FLAGS) $(SIMIAS_LIBS) $(SimiasWebService_LIBS:%=/r:%) $(SimiasWebService_LIBPATH:%=/lib:%) $(SimiasWebService_RESOURCES:%=$(ICON_FLAG)%) $(SimiasWebService_CSFILES) /doc:SimiasWebService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Web.pdb
endif
endif

install-exec-local: Simias.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) web.config Global.asax Simias.asmx SimiasBrowser.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f web.config Global.asax Simias.asmx SimiasBrowser.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Web.dll Simias.Web.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

