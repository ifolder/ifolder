# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SIMIAS_CSFILES1 := $(shell cat SimiasTests.dll.sources)
SIMIAS_CSFILES2 := $(SIMIAS_CSFILES1:%=$(srcdir)/%)
SIMIAS_CSFILES := $(subst /,$(SEP),$(SIMIAS_CSFILES2))
SIMIAS_INCLUDES =
SIMIAS_RESOURCES =
SIMIAS_FLAGS = $(CSC_LIBFLAG) /unsafe
SIMIAS_LIBS = Simias.dll nunit.util.dll nunit.framework.dll System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll mDnsResponder.dll mDnsResponderApi.dll log4net.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll 
SIMIAS_LIBPATH = ../Simias.dll $(NUNIT_LIBPATH) $(srcdir)/../../external/SecureChannelSinks $(LOG4NET_DIR) $(MDNSRESPONDER_DIR)

EXTRA_DIST = $(srcdir)/SimiasTests.dll.sources $(SIMIAS_CSFILES)

NUNIT_TESTS = SimiasTests.dll

CUR_DIR := $(shell pwd)

all:

SimiasTests.dll: $(SIMIAS_CSFILES) $(SIMIAS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_FLAGS) $(SIMIAS_LIBS:%=/r:%) $(SIMIAS_LIBPATH:%=/lib:%) $(SIMIAS_CSFILES) /doc:SimiasTests.doc.xml

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SimiasTests.pdb
endif
endif

installcheck-local: $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(libdir)
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)

CLEAN_FILES = Simias.dll $(NUNIT_TESTS) Simias.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

