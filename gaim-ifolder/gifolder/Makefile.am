plugindir = $(libdir)/gaim

gifolder_la_LDFLAGS = \
	-module -avoid-version \
	$(SIMIAS_CLIENT_LIBS)

if PLUGINS

plugin_LTLIBRARIES = gifolder.la

gifolder_la_SOURCES = \
	gifolder.c \
	gaim-domain.c

endif

gifolder_la_LIBADD =

AM_CPPFLAGS = \
	-DDATADIR=\"$(datadir)\" \
	-DVERSION=\"$(VERSION)\" \
	-I$(top_srcdir)/src \
	$(SIMIAS_CLIENT_CFLAGS) \
	$(DEBUG_CFLAGS) \
	$(GTK_CFLAGS)
