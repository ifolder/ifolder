# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Simdezvous_CSFILES = $(srcdir)/Service.cs $(srcdir)/Domain.cs $(srcdir)/User.cs $(srcdir)/DomainProvider.cs $(srcdir)/Auth.cs  
Simdezvous_CSFILES_CSC := $(subst /,$(SEP),$(Simdezvous_CSFILES))
Simdezvous_INCLUDES =
Simdezvous_RESOURCES =
Simdezvous_FLAGS = $(CSC_LIBFLAG)
Simdezvous_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll log4net.dll Novell.Security.ClientPasswordManager.NetCredential.dll
Simdezvous_LIBPATH = ../SimiasClient ../Simias.dll ./external $(NOVELL_SECURITY_DIR) $(LOG4NET_DIR)

EXTRA_DIST = $(Simdezvous_CSFILES)

#if MAKE_DOC_FILE
#DOC_FILE = $(basename $@).doc.xml
#DOC_FILE_CMD = /doc:$(DOC_FILE)
#DOC_FILE_CLEAN = *.doc.xml
#endif

all: Simdezvous.dll

Simdezvous.dll: $(Simdezvous_CSFILES) $(Simdezvous_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Simdezvous_FLAGS) $(SIMIAS_LIBS) $(Simdezvous_LIBS:%=/r:%) $(Simdezvous_LIBPATH:%=/lib:%) $(Simdezvous_RESOURCES:%=$(ICON_FLAG)%) $(Simdezvous_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simdezvous.pdb
endif
endif

if WINDOWS
OTHER_FILES = ../libsimdezvous/libsimdezvous.dll
endif

install-exec-local: Simdezvous.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simdezvous.dll $(OTHER_FILES) $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simdezvous.dll $(OTHER_FILES) $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simdezvous.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

