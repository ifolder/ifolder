# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

mDnsDomain_CSFILES1 = $(srcdir)/Service.cs $(srcdir)/Domain.cs $(srcdir)/User.cs $(srcdir)/DomainProvider.cs $(srcdir)/Auth.cs  
mDnsDomain_CSFILES := $(subst /,$(SEP),$(mDnsDomain_CSFILES1))
mDnsDomain_INCLUDES =
mDnsDomain_RESOURCES =
mDnsDomain_FLAGS = $(CSC_LIBFLAG)
mDnsDomain_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll mDnsResponder.dll mDnsResponderApi.dll log4net.dll Novell.Security.ClientPasswordManager.NetCredential.dll
mDnsDomain_LIBPATH = ../SimiasClient ../Simias.dll ./external $(MDNSRESPONDER_DIR) $(NOVELL_SECURITY_DIR)

EXTRA_DIST = $(mDnsDomain_CSFILES1)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.mDns.dll

Simias.mDns.dll: $(mDnsDomain_CSFILES) $(mDnsDomain_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(mDnsDomain_FLAGS) $(SIMIAS_LIBS) $(mDnsDomain_LIBS:%=/r:%) $(mDnsDomain_LIBPATH:%=/lib:%) $(mDnsDomain_RESOURCES:%=$(ICON_FLAG)%) $(mDnsDomain_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.mDns.pdb
endif
endif

if WINDOWS
OTHER_FILES = ../libsimdezvous/simdezvous.dll
endif

install-exec-local: Simias.mDns.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.mDns.dll $(OTHER_FILES) $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.mDns.dll $(OTHER_FILES) $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.mDns.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

