INCLUDES = `pkg-config --cflags glib-2.0 gnome-vfs-module-2.0`

PREFIX=`pkg-config --variable=prefix gnome-vfs-module-2.0`
CONFDIR=/etc$(PREFIX)/gnome-vfs-2.0/modules
MODDIR=$(PREFIX)/lib/gnome-vfs-2.0/modules

EXTRA_DIST = ifolders.conf

lib_LTLIBRARIES = libifolders.la

libifolders_la_SOURCES = ifolders-method.c
libifolders_la_LDFLAGS = `pkg-config --libs glib-2.0 gnome-vfs-module-2.0`
libdir = $(prefix)/lib/gnome-vfs-2.0/modules

conf_DATA = ifolders.conf
confdir = $(sysconfdir)/gnome-vfs-2.0/modules

CLEAN_FILES = 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

