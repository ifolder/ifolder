# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SIMIAS_CSFILES1 := $(shell cat Simias.dll.sources)
SIMIAS_CSFILES2 := $(SIMIAS_CSFILES1:%=$(srcdir)/%)
SIMIAS_CSFILES := $(subst /,$(SEP),$(SIMIAS_CSFILES2))
SIMIAS_INCLUDES =
SIMIAS_RESOURCES =
SIMIAS_FLAGS = $(CSC_LIBFLAG) /unsafe
SIMIAS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll NUnit.Framework.dll NUnit.Util.dll mDnsResponder.dll mDnsResponderApi.dll log4net.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll 
SIMIAS_LIBPATH = $(NUNIT_LIBPATH) $(srcdir)/../../external/SecureChannelSinks $(LOG4NET_DIR) $(MDNSRESPONDER_DIR)

EXTRA_DIST = $(srcdir)/Simias.dll.sources $(SIMIAS_CSFILES)

CUR_DIR := $(shell pwd)

all: Simias.dll

Simias.dll: $(SIMIAS_CSFILES) $(SIMIAS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_FLAGS) $(SIMIAS_LIBS:%=/r:%) $(SIMIAS_LIBPATH:%=/lib:%) $(SIMIAS_CSFILES) /doc:Simias.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: Simias.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Simias.dll ../../external/SecureChannelSinks/*.dll $(LOG4NET_DIR)/log4net.dll $(MDNSRESPONDER_DIR)/m*.exe $(MDNSRESPONDER_DIR)/m*.dll $(DEBUG_FILES) $(srcdir)/$(SQLITELIB_PATH)/$(SQLITELIB) $(DESTDIR)$(libdir)

installcheck-local: install
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:Simias.test.xml Simias.dll

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Simias.dll Novell.Security.SecureSink*.dll log4net.dll mDnsResponder* $(SQLITELIB) $(NUNIT_TESTS); rm -rf .cstore $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

CLEAN_FILES = Simias.dll $(NUNIT_TESTS) Simias.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

