# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SIMIAS_CSFILES     := $(shell cat Simias.dll.sources)
SIMIAS_CSFILES_SRC := $(SIMIAS_CSFILES:%=$(srcdir)/%)
SIMIAS_CSFILES_CSC := $(subst /,$(SEP),$(SIMIAS_CSFILES_SRC))
SIMIAS_INCLUDES =
SIMIAS_RESOURCES =
SIMIAS_FLAGS = $(CSC_LIBFLAG) /unsafe
SIMIAS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll nunit.framework.dll nunit.util.dll mDnsResponder.dll mDnsResponderApi.dll log4net.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll 
SIMIAS_LIBPATH = $(NUNIT_LIBPATH) $(srcdir)/../../external/SecureChannelSinks $(LOG4NET_DIR) $(MDNSRESPONDER_DIR)

EXTRA_DIST = $(srcdir)/Simias.dll.sources $(SIMIAS_CSFILES) ../Common/Library/SimiasSetup.cs.in

CUR_DIR := $(shell pwd)

all: Simias.dll

Simias.dll: $(SIMIAS_CSFILES_SRC) $(SIMIAS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_FLAGS) $(SIMIAS_LIBS:%=/r:%) $(SIMIAS_LIBPATH:%=/lib:%) $(SIMIAS_CSFILES_CSC) /doc:Simias.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: Simias.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(MDNSRESPONDER_DIR)/m*.exe $(MDNSRESPONDER_DIR)/m*.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/../../external/SecureChannelSinks/*.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(SQLITELIB_PATH)/$(SQLITELIB) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(SQLITELIB_PATH)/$(SQLITELIB) $(DESTDIR)$(bindir)
	cp -f $(srcdir)/$(LOG4NET_DIR)/../../../../LICENSE.txt $(DESTDIR)$(prefix)/log4net.LICENSE.txt
if DOTNET
	$(INSTALL_PROGRAM) $(srcdir)/$(NUNIT_LIBPATH)/nunit.core.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(NUNIT_LIBPATH)/nunit.framework.dll $(DESTDIR)$(libdir)
	cp -f $(srcdir)/$(NUNIT_LIBPATH)/../license.rtf $(DESTDIR)$(prefix)/nunit.LICENSE.rtf
endif

installcheck-local: install
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:Simias.test.xml Simias.dll

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f $(SQLITELIB)
	cd $(DESTDIR)$(libdir); rm -f Novell.Security.SecureSink*.dll
	cd $(DESTDIR)$(libdir); rm -f log4net.dll
	cd $(DESTDIR)$(libdir); rm -f mDnsResponder*
	cd $(DESTDIR)$(libdir); rm -f Simias.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f $(SQLITELIB)
	cd $(DESTDIR)$(prefix); rm -f log4net.LICENSE.txt
if DOTNET
	cd $(DESTDIR)$(libdir); rm -f nunit.core.dll nunit.framework.dll
	cd $(DESTDIR)$(prefix); rm -f nunit.LICENSE.rtf
endif

CLEAN_FILES = Simias.dll $(NUNIT_TESTS) Simias.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

