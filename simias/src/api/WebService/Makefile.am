# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasWebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SimiasAPI.cs
SimiasWebService_CSFILES_CSC := $(subst /,$(SEP),$(SimiasWebService_CSFILES))
SimiasWebService_INCLUDES =
SimiasWebService_RESOURCES =
SimiasWebService_FLAGS = $(CSC_LIBFLAG)
SimiasWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll SimiasLib.dll Novell.Security.ClientPasswordManager.NetCredential.dll
SimiasWebService_LIBPATH = ../../SimiasClient ../../SimiasLib.dll $(NOVELL_SECURITY_DIR)

EXTRA_DIST = SimiasAPI.wsdl $(SimiasWebService_CSFILES) AssemblyInfo.cs.in

all: SimiasAPIService.Web.dll

SimiasAPIService.Web.dll: AssemblyInfo.cs $(SimiasWebService_CSFILES) $(SimiasWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasWebService_FLAGS) $(SIMIAS_LIBS) $(SimiasWebService_LIBS:%=/r:%) $(SimiasWebService_LIBPATH:%=/lib:%) $(SimiasWebService_RESOURCES:%=$(ICON_FLAG)%) $(SimiasWebService_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasAPIService.Web.pdb
endif
endif

install-exec-local: SimiasAPIService.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimiasAPI.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) SimiasAPIService.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f SimiasAPI.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f SimiasAPIService.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = SimiasAPIService.Web.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf AssemblyInfo.cs $(COMMON_MAINTAINER_CLEAN_FILES)

