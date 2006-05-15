INCLUDES = -I$(top_srcdir) -I$(top_srcdir)/src/libifolder

IMAGE_DIR = $(top_srcdir)/src/images
IMAGE_INSTALL_DIR = $(datadir)/icons/hicolor/22x22/apps

icondir = $(datadir)/icons/hicolor/22x22/apps
icon_DATA = \
	$(IMAGE_DIR)/22x22/ifolder-downloading.png \
	$(IMAGE_DIR)/22x22/ifolder-idle.png \
	$(IMAGE_DIR)/22x22/ifolder-shutting-down.png \
	$(IMAGE_DIR)/22x22/ifolder-starting-up.png \
	$(IMAGE_DIR)/22x22/ifolder-uploading.png
	

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

bin_PROGRAMS = ifolder3-applet

CC = g++

ifolder3_applet_CPPFLAGS = \
 	$(TRAYAPP_CFLAGS) \
	-DEGG_COMPILATION \
	-DG_DISABLE_DEPRECATED								\
	-DGDK_DISABLE_DEPRECATED								\
	-DGNOME_DISABLE_DEPRECATED
	
ifolder3_applet_SOURCES = \
	main.c					\
	applet.c				\
	applet.h				\
	preferences-window.c			\
	preferences-window.h			\
	util.c					\
	util.h					\
	eggtrayicon.c			\
	eggtrayicon.h

ifolder3_applet_LDFLAGS = \
	-L$(top_srcdir)/src/libifolder \
	$(TRAYAPP_LIBS)

ifolder3_applet_LDADD = \
	$(top_srcdir)/src/libifolder/libifolder-trayapp.a \
	$(top_srcdir)/src/core/libifolder-core.a

#install-exec-local:
#	$(mkinstalldirs) $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-downloading.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-idle.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-shutting-down.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-starting-up.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-uploading.png $(IMAGE_INSTALL_DIR)

install-data-hook:
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating Gtk icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi

uninstall-local:
	cd $(IMAGE_INSTALL_DIR); rm -f ifolder-*.png

maintainer-clean-local:
	rm -f Makefile.in
