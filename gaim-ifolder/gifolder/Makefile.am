SUBDIRS = pixmaps
SIMIAS_GAIM_DOMAIN_WSDL = GaimDomainService.wsdl
GSOAP_PATH = gsoap/linux-2.7

plugindir = $(libdir)/gaim

gifolder_la_LDFLAGS = \
	-module -avoid-version

if PLUGINS

plugin_LTLIBRARIES = gifolder.la

gifolder_la_SOURCES = \
	envC.c \
	simiasgaimClientLib.c \
	stdsoap2.c \
	gifolder.c \
	event-handler.c \
	gaim-domain.c \
	simias-messages.c \
	simias-prefs.c \
	simias-util.c \
	buddy-profile.c

endif

gifolder_la_LIBADD =

AM_CPPFLAGS = \
	-DDATADIR=\"$(datadir)\" \
	-DVERSION=\"$(VERSION)\" \
	-DWITH_NONAMESPACES \
	-I$(top_srcdir)/src \
	-I$(top_srcdir)/src/protocols/oscar \
	$(DEBUG_CFLAGS) \
	$(GTK_CFLAGS)

envC.c: $(srcdir)/stdsoap2.c
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -c -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.c:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.c $(srcdir)

$(srcdir)/libsimiasgaim.h: $(srcdir)/stdsoap2.c $(SIMIAS_GAIM_DOMAIN_WSDL)
	$(GSOAP_PATH)/wsdl2h -c -o $@ $(SIMIAS_GAIM_DOMAIN_WSDL)

$(srcdir)/simiasgaimClientLib.c: $(srcdir)/libsimiasgaim.h
	$(GSOAP_PATH)/soapcpp2 -c -n -psimiasgaim $(srcdir)/libsimiasgaim.h
