# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

mDnsDomain_CSFILES1 = $(srcdir)/Service.cs $(srcdir)/Domain.cs $(srcdir)/User.cs 
mDnsDomain_CSFILES := $(subst /,$(SEP),$(mDnsDomain_CSFILES1))
mDnsDomain_INCLUDES =
mDnsDomain_RESOURCES =
mDnsDomain_FLAGS = $(CSC_LIBFLAG)
mDnsDomain_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll mDnsResponderApi.dll log4net.dll
mDnsDomain_LIBPATH = ../SimiasClient ../Simias.dll $(MDNSRESPONDER_DIR)

EXTRA_DIST = 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.Domain.mDns.dll

Simias.Domain.mDns.dll: $(mDnsDomain_CSFILES) $(mDnsDomain_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(mDnsDomain_FLAGS) $(SIMIAS_LIBS) $(mDnsDomain_LIBS:%=/r:%) $(mDnsDomain_LIBPATH:%=/lib:%) $(mDnsDomain_RESOURCES:%=$(ICON_FLAG)%) $(mDnsDomain_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Domain.mDns.pdb
endif
endif

install-exec-local: Simias.Domain.mDns.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Domain.mDns.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.Domain.mDns.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Domain.mDns.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

