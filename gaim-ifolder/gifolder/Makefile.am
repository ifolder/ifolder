SUBDIRS = pixmaps

plugindir = $(libdir)/gaim

gifolder_la_LDFLAGS = \
	-module -avoid-version \
	$(SIMIAS_CLIENT_LIBS)

if PLUGINS

plugin_LTLIBRARIES = gifolder.la

gifolder_la_SOURCES = \
	gifolder.c \
	event-handler.c \
	gaim-domain.c \
	simias-invitation-store.c \
	simias-messages.c \
	simias-prefs.c \
	simias-util.c \
	buddy-profile.c

endif

gifolder_la_LIBADD =

AM_CPPFLAGS = \
	-DDATADIR=\"$(datadir)\" \
	-DVERSION=\"$(VERSION)\" \
	-I$(top_srcdir)/src \
	-I$(top_srcdir)/src/protocols/oscar \
	$(SIMIAS_CLIENT_CFLAGS) \
	$(DEBUG_CFLAGS) \
	$(GTK_CFLAGS)
