INCLUDES = \
	$(GLIB_CFLAGS) \
	$(GNOME_VFS_MODULE_CFLAGS) \
	$(SIMIAS_CLIENT_C_CFLAGS) \
	$(IFOLDER_C_CFLAGS)

GPREFIX=`pkg-config --variable=prefix gnome-vfs-module-2.0`

EXTRA_DIST = ifolders.conf

mod_LTLIBRARIES = libifolders.la

libifolders_la_SOURCES = ifolders-method.c
libifolders_la_LDFLAGS = \
	$(GLIB_LIBS) \
	$(GNOME_VFS_MODULE_LIBS) \
	$(SIMIAS_CLIENT_C_LIBS) \
	$(IFOLDER_C_LIBS)
moddir = $(GPREFIX)/lib/gnome-vfs-2.0/modules

conf_DATA = ifolders.conf
confdir = /etc$(GPREFIX)/gnome-vfs-2.0/modules

bin_PROGRAMS = ifolders-test
ifolders_test_SOURCES = ifolders-test.c ifolders-method.c
ifolders_test_LDFLAGS = \
	$(GLIB_LIBS) \
	$(GNOME_VFS_MODULE_LIBS) \
	$(SIMIAS_CLIENT_C_LIBS) \
	$(IFOLDER_C_LIBS)

