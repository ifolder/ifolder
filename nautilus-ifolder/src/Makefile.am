INCLUDES =						\
	-DG_LOG_DOMAIN=\"Nautilus-iFolder\"		        \
	-DDATADIR=\"$(datadir)\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\" 	\
	-I$(top_srcdir)					\
	-I$(top_builddir)				\
	$(WARN_CFLAGS)                                  \
	-Werror                                         \
	$(DISABLE_DEPRECATED_CFLAGS)			\
	$(NAUTILUS_CFLAGS)

# FIXME: get this from the .pc file
nautilus_extensiondir=$(libdir)/nautilus/extensions-1.0

nautilus_extension_LTLIBRARIES=libnautilus-ifolder.la

libnautilus_ifolder_la_SOURCES =     \
	nautilus-ifolder.c

libnautilus_ifolder_la_LDFLAGS = -module -avoid-version
libnautilus_ifolder_la_LIBADD  = $(NAUTILUS_LIBS)
