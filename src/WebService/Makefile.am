# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebService_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderService.cs $(srcdir)/iFolder.cs $(srcdir)/DiskSpace.cs $(srcdir)/iFolderUser.cs $(srcdir)/iFolderSettings.cs
iFolderWebService_CSFILES := $(subst /,$(SEP),$(iFolderWebService_CSFILES1))
iFolderWebService_INCLUDES = 
iFolderWebService_RESOURCES =  
iFolderWebService_FLAGS = $(CSC_LIBFLAG)
iFolderWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
iFolderWebService_LIBPATH = $(LOG4NET_DIR)

EXTRA_DIST = iFolder.asmx $(iFolderWebService_CSFILES)

all: Novell.iFolder.Web.dll

Novell.iFolder.Web.dll: $(iFolderWebService_CSFILES) $(iFolderWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebService_FLAGS) $(SIMIAS_LIBS) $(iFolderWebService_LIBS:%=/r:%) $(iFolderWebService_LIBPATH:%=/lib:%) $(iFolderWebService_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebService_CSFILES) /doc:iFolderWebService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.Web.pdb
endif
endif

install-exec-local: Novell.iFolder.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(mkinstalldirs) $(DESTDIR)$(bindir)/bin
	$(INSTALL_PROGRAM) iFolder.asmx $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.iFolder.Web.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)/bin
	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolder.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir)/bin; rm -f Novell.iFolder.Web.dll $(DEBUG_FILES)
    
installcheck-local: install
	
CLEAN_FILES = Novell.iFolder.Web.dll Novell.iFolder.Web.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

