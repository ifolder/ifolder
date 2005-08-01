# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SIMIAS_CSFILES     := $(shell cat $(srcdir)/Simias.dll.sources)
SIMIAS_CSFILES_SRC := $(SIMIAS_CSFILES:%=$(srcdir)/%)
SIMIAS_CSFILES_CSC := $(subst /,$(SEP),$(SIMIAS_CSFILES_SRC))
SIMIAS_INCLUDES =
SIMIAS_RESOURCES =
SIMIAS_FLAGS = $(CSC_LIBFLAG) /unsafe
SIMIAS_LIBS1 = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll System.Web.dll System.Web.Services.dll SimiasClient.dll log4net.dll Novell.Security.ClientPasswordManager.NetCredential.dll Novell.Security.Utilities.dll
SIMIAS_LIBPATH = $(LOG4NET_DIR) $(NOVELL_SECURITY_DIR) ../SimiasClient

if !WINDOWS
	SIMIAS_LIBS = $(SIMIAS_LIBS1) Mono.Posix.dll
else
	SIMIAS_LIBS = $(SIMIAS_LIBS1)
endif

EXTRA_DIST = $(srcdir)/Simias.dll.sources $(SIMIAS_CSFILES) ../Common/Simias.log4net ../Common/simias-client-bootstrap.config.in ../Common/MyDns.cs.in ../Login/Login.ashx Simias.dll.config.in

if WINDOWS
	FIXED_WEBDIR := $(shell cygpath --mixed $(webdir))
else
	FIXED_WEBDIR := $(webdir)
endif

../Common/simias-client-bootstrap.config: ../Common/simias-client-bootstrap.config.in
	sed -e 's,@_webdir_@,$(FIXED_WEBDIR),g' -e 's,@_StoreProviderAssembly_@,@StoreProviderAssembly@,g' -e 's,@_StoreProviderType_@,@StoreProviderType@,g' $< > ../Common/simias-client-bootstrap.config
	chmod +x $@
	
../Common/MyDns.cs: ../Common/MyDns.cs.in
	sed -e 's,@_webbindir_@,@webbindir@,g' $< > ../Common/MyDns.cs
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: Simias.dll ../Common/simias-client-bootstrap.config

Simias.dll: $(SIMIAS_CSFILES_SRC) ../Common/MyDns.cs $(SIMIAS_RESOURCES) $(srcdir)/Simias.dll.sources
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_FLAGS) $(SIMIAS_LIBS:%=/r:%) $(SIMIAS_LIBPATH:%=/lib:%) $(SIMIAS_CSFILES_CSC) $(subst /,$(SEP),../Common/MyDns.cs) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(sysconfdir) $(DESTDIR)$(datadir)/simias
	$(INSTALL_PROGRAM) Simias.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(srcdir)/$(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(srcdir)/$(NOVELL_SECURITY_DIR)/Novell.Security.ClientPasswordManager.NetCredential.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(srcdir)/$(NOVELL_SECURITY_DIR)/Novell.Security.Utilities.dll $(DESTDIR)$(webbindir)
	cp -f $(srcdir)/$(LOG4NET_DIR)/../../../../LICENSE.txt $(DESTDIR)$(datadir)/simias/log4net.LICENSE.txt
	$(INSTALL_PROGRAM) ../Common/Simias.log4net $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Common/simias-client-bootstrap.config $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Login/Login.ashx $(DESTDIR)$(webdir)
if LINUX
	$(INSTALL_PROGRAM) Simias.dll.config $(DESTDIR)$(bindir)
endif

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Novell.Security.SecureSink*.dll
	cd $(DESTDIR)$(webbindir); rm -f log4net.dll
	cd $(DESTDIR)$(webbindir); rm -f Novell.Security*.dll
	cd $(DESTDIR)$(webbindir); rm -f Simias.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(sysconfdir); rm -f Simias.log4net simias-client-bootstrap.config
	cd $(DESTDIR)$(datadir)/simias; rm -f log4net.LICENSE.txt
if LINUX
	cd $(DESTDIR)$(webbindir); rm -f Simias.dll.config
endif

CLEAN_FILES = Simias.dll $(DOC_FILE_CLEAN) $(DEBUG_FILES) ../Common/simias-client-bootstrap.config ../Common/MyDns.cs

clean-local:
	rm -rf $(CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

