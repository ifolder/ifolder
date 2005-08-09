# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasWebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Global.asax.cs $(srcdir)/Simias.asmx.cs
SimiasWebService_CSFILES_CSC := $(subst /,$(SEP),$(SimiasWebService_CSFILES))
SimiasWebService_INCLUDES =
SimiasWebService_RESOURCES =
SimiasWebService_FLAGS = $(CSC_LIBFLAG)
SimiasWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll Simias.dll Novell.Security.ClientPasswordManager.NetCredential.dll
SimiasWebService_LIBPATH = ../SimiasClient ../Simias.dll $(NOVELL_SECURITY_DIR)

EXTRA_DIST = win.web.config mono.web.config Global.asax Simias.asmx Simias.wsdl $(SimiasWebService_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.Web.dll

Simias.Web.dll: AssemblyInfo.cs $(SimiasWebService_CSFILES) $(SimiasWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasWebService_FLAGS) $(SIMIAS_LIBS) $(SimiasWebService_LIBS:%=/r:%) $(SimiasWebService_LIBPATH:%=/lib:%) $(SimiasWebService_RESOURCES:%=$(ICON_FLAG)%) $(SimiasWebService_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Web.pdb
endif
endif

install-exec-local: Simias.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Global.asax Simias.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
if WINDOWS
	$(INSTALL_PROGRAM) win.web.config $(DESTDIR)$(webdir)/web.config
else
	$(INSTALL_PROGRAM) mono.web.config $(DESTDIR)$(webdir)/web.config
endif	
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f web.config Global.asax Simias.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Web.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

