# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderService.cs $(srcdir)/iFolder.cs $(srcdir)/DiskSpace.cs $(srcdir)/iFolderUser.cs $(srcdir)/Conflict.cs $(srcdir)/SyncSize.cs $(srcdir)/ClientUpgrade.cs $(srcdir)/ClientUpdate.cs
iFolderWebService_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebService_CSFILES))
iFolderWebService_INCLUDES =
iFolderWebService_RESOURCES =
iFolderWebService_FLAGS = $(CSC_LIBFLAG)
iFolderWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll Novell.Security.ClientPasswordManager.NetCredential.dll
iFolderWebService_LIBPATH = $(LOG4NET_DIR)

EXTRA_DIST = iFolder.asmx $(iFolderWebService_CSFILES) $(srcdir)/iFolderWebService.wsdl AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Novell.iFolder.Web.dll

Novell.iFolder.Web.dll: $(iFolderWebService_CSFILES) $(iFolderWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebService_FLAGS) $(SIMIAS_LIBS) $(SIMIAS_CLIENT_LIBS) $(iFolderWebService_LIBS:%=/r:%) $(iFolderWebService_LIBPATH:%=/lib:%) $(iFolderWebService_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebService_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.Web.pdb
endif
endif

install-exec-local: Novell.iFolder.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) iFolder.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Novell.iFolder.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f iFolder.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolder.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Novell.iFolder.Web.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

