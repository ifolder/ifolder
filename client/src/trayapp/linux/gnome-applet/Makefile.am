INCLUDES = -I$(top_srcdir) -I$(top_srcdir)/src/libifolder

IMAGE_DIR = $(top_srcdir)/src/images
IMAGE_INSTALL_DIR = $(GNOME_ROOT)/share/icons/hicolor/22x22/apps

bin_PROGRAMS = ifolder3-applet

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
	eggtrayicon.c			\
	eggtrayicon.h

ifolder3_applet_LDFLAGS = \
	$(top_srcdir)/src/libifolder/ipc-server/libipcserver/libipcserver.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsoap/libsoap.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsync/libsync.a \
	$(top_srcdir)/src/libifolder/ipc-server/libifolder-trayapp.a \
	$(TRAYAPP_LIBS) \
	$(QT4_CORE_LIBS)

install-exec-local:
	$(mkinstalldirs) $(IMAGE_INSTALL_DIR)
	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-downloading.png $(IMAGE_INSTALL_DIR)
	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-idle.png $(IMAGE_INSTALL_DIR)
	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-shutting-down.png $(IMAGE_INSTALL_DIR)
	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-starting-up.png $(IMAGE_INSTALL_DIR)
	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-uploading.png $(IMAGE_INSTALL_DIR)

uninstall-local:
	cd $(IMAGE_INSTALL_DIR); rm -f ifolder-*.png

maintainer-clean-local:
	rm -f Makefile.in
