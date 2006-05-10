INCLUDES = \
	$(GLIB_CFLAGS) \
	$(GNOME_VFS_MODULE_CFLAGS) \
	$(SIMIAS_CLIENT_C_CFLAGS) \
	$(IFOLDER_C_CFLAGS)

GPREFIX=`pkg-config --variable=prefix gnome-vfs-module-2.0`

EXTRA_DIST = $(conf_DATA)

mod_LTLIBRARIES = libifolder.la

libifolder_la_SOURCES = ifolder-method.c
libifolder_la_LDFLAGS = \
	$(GLIB_LIBS) \
	$(GNOME_VFS_MODULE_LIBS) \
	$(SIMIAS_CLIENT_C_LIBS) \
	$(IFOLDER_C_LIBS)
moddir = $(GPREFIX)/lib/gnome-vfs-2.0/modules

conf_DATA = ifolder.conf
confdir = /etc$(GPREFIX)/gnome-vfs-2.0/modules

