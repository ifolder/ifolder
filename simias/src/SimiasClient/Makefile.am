# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasClient_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/CollectionSyncEventArgs.cs $(srcdir)/Configuration.cs $(srcdir)/DomainService.cs $(srcdir)/IProcEventClient.cs $(srcdir)/IProcEventData.cs $(srcdir)/IProcEventListener.cs $(srcdir)/IProcEventRegistration.cs $(srcdir)/IProcEventTypes.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/SimiasEventArgs.cs
SimiasClient_CSFILES := $(subst /,$(SEP),$(SimiasClient_CSFILES1))
SimiasClient_INCLUDES =
SimiasClient_RESOURCES =
SimiasClient_FLAGS = $(CSC_LIBFLAG)
SimiasClient_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
SimiasClient_LIBPATH =

EXTRA_DIST = $(SimiasClient_CSFILES)

all: SimiasClient.dll

SimiasClient.dll: $(SimiasClient_CSFILES) $(SimiasClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasClient_FLAGS) $(SIMIAS_LIBS) $(SimiasClient_LIBS:%=/r:%) $(SimiasClient_LIBPATH:%=/lib:%) $(SimiasClient_RESOURCES:%=$(ICON_FLAG)%) $(SimiasClient_CSFILES) /doc:SimiasClient.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasClient.pdb
endif
endif

install-exec-local: SimiasClient.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasClient.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SimiasClient.dll $(DEBUG_FILES)
	
installcheck-local: install
	
CLEAN_FILES = SimiasClient.dll SimiasClient.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

