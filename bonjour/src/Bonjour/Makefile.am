# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Bonjour_CSFILES1 = $(srcdir)/Service.cs $(srcdir)/Domain.cs $(srcdir)/User.cs $(srcdir)/DomainProvider.cs $(srcdir)/Auth.cs  
Bonjour_CSFILES := $(subst /,$(SEP),$(Bonjour_CSFILES1))
Bonjour_INCLUDES =
Bonjour_RESOURCES =
Bonjour_FLAGS = $(CSC_LIBFLAG)
Bonjour_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll log4net.dll Novell.Security.ClientPasswordManager.NetCredential.dll
Bonjour_LIBPATH = ../SimiasClient ../Simias.dll ./external $(NOVELL_SECURITY_DIR) $(LOG4NET_DIR)

EXTRA_DIST = $(Bonjour_CSFILES1)

#if MAKE_DOC_FILE
#DOC_FILE = $(basename $@).doc.xml
#DOC_FILE_CMD = /doc:$(DOC_FILE)
#DOC_FILE_CLEAN = *.doc.xml
#endif

all: Simias.Bonjour.dll

Simias.Bonjour.dll: $(Bonjour_CSFILES) $(Bonjour_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Bonjour_FLAGS) $(SIMIAS_LIBS) $(Bonjour_LIBS:%=/r:%) $(Bonjour_LIBPATH:%=/lib:%) $(Bonjour_RESOURCES:%=$(ICON_FLAG)%) $(Bonjour_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Bonjour.pdb
endif
endif

if WINDOWS
OTHER_FILES = ../libsimdezvous/libsimdezvous.dll
endif

install-exec-local: Simias.Bonjour.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Bonjour.dll $(OTHER_FILES) $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.Bonjour.dll $(OTHER_FILES) $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Bonjour.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

