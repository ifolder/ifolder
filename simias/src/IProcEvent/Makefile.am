# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

IProcEvent_CSFILES     := $(srcdir)/AssemblyInfo.cs $(srcdir)/IProcEventClient.cs
IProcEvent_CSFILES_CSC := $(subst /,$(SEP),$(IProcEvent_CSFILES))
IProcEvent_INCLUDES =
IProcEvent_RESOURCES =
IProcEvent_FLAGS = $(CSC_LIBFLAG)
IProcEvent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) log4net.dll Simias.dll
IProcEvent_LIBPATH = ../Simias.dll $(LOG4NET_DIR)

EXTRA_DIST = $(IProcEvent_CSFILES)

CUR_DIR := $(shell pwd)

all: IProcEvent.dll

IProcEvent.dll: $(IProcEvent_CSFILES) $(IProcEvent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(IProcEvent_FLAGS) $(IProcEvent_LIBS:%=/r:%) $(IProcEvent_LIBPATH:%=/lib:%) $(IProcEvent_CSFILES_CSC) /doc:IProcEvent.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = IProcEvent.pdb
endif
endif

install-exec-local: IProcEvent.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir) $(DESTDIR)$(datadir)/simias
	$(mkinstalldirs) $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) IProcEvent.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/../Simias.dll/Simias.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(MDNSRESPONDER_DIR)/m*.exe $(MDNSRESPONDER_DIR)/m*.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/../../external/SecureChannelSinks/*.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(STORELIB_PATH)/$(STORELIB) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(srcdir)/$(STORELIB_PATH)/$(STORELIB) $(DESTDIR)$(bindir)
	cp -f $(srcdir)/$(LOG4NET_DIR)/../../../../LICENSE.txt $(DESTDIR)$(datadir)/simias/log4net.LICENSE.txt
	$(INSTALL_PROGRAM) ../Common/Simias.remoting $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Common/Simias.log4net $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Common/Simias.config $(DESTDIR)$(sysconfdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f IProcEvent.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(libdir); rm -f $(STORELIB)
	cd $(DESTDIR)$(libdir); rm -f Novell.Security.SecureSink*.dll
	cd $(DESTDIR)$(libdir); rm -f log4net.dll
	cd $(DESTDIR)$(libdir); rm -f mDnsResponder*
	cd $(DESTDIR)$(libdir); rm -f Simias.dll
	cd $(DESTDIR)$(bindir); rm -f $(STORELIB)
	cd $(DESTDIR)$(sysconfdir); rm -f Simias.remoting Simias.log4net Simias.config
	cd $(DESTDIR)$(datadir)/simias; rm -f log4net.LICENSE.txt

CLEAN_FILES = IProcEvent.dll IProcEvent.doc.xml $(DEBUG_FILES)

clean-local:
	rm -rf $(CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

