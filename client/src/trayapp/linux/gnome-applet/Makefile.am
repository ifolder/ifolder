INCLUDES = -I$(top_srcdir) -I$(top_srcdir)/src/libifolder

IMAGE_DIR = $(top_srcdir)/src/images
#IMAGE_INSTALL_DIR = $(datadir)/pixmaps/ifolder3

icondir = $(datadir)/pixmaps/ifolder3
icon_DATA = \
	$(IMAGE_DIR)/22x22/ifolder-downloading22.png \
	$(IMAGE_DIR)/22x22/ifolder-idle22.png \
	$(IMAGE_DIR)/22x22/ifolder-shutting-down22.png \
	$(IMAGE_DIR)/22x22/ifolder-starting-up22.png \
	$(IMAGE_DIR)/22x22/ifolder-uploading22.png \
	$(IMAGE_DIR)/48x48/ifolder48.png \
	$(IMAGE_DIR)/48x48/ifolder-error48.png \
	$(IMAGE_DIR)/48x48/ifolder-sync48.png \
	$(IMAGE_DIR)/48x48/ifolder-waiting48.png \
	$(IMAGE_DIR)/48x48/ifolder-warning48.png

#gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

bin_PROGRAMS = ifolder3-applet

CC = g++

ifolder3_applet_CPPFLAGS = \
 	$(TRAYAPP_CFLAGS) \
	-DEGG_COMPILATION \
	-DG_DISABLE_DEPRECATED								\
	-DGDK_DISABLE_DEPRECATED								\
	-DGNOME_DISABLE_DEPRECATED								\
	-DDATADIR=\""$(datadir)"\"
	
	
ifolder3_applet_SOURCES = \
	main.c					\
	applet.c				\
	applet.h				\
	account-properties-dialog.c		\
	account-properties-dialog.h		\
	account-wizard.c				\
	account-wizard.h				\
	main-window.c					\
	main-window.h					\
	preferences-window.c			\
	preferences-window.h			\
	util.c					\
	util.h					\
	eggtrayicon.c			\
	eggtrayicon.h					\
	wait-dialog.c					\
	wait-dialog.h

ifolder3_applet_LDFLAGS = \
	-L$(top_srcdir)/src/core/.libs \
	-L$(top_srcdir)/src/libifolder/.libs \
	-lifolder3-core -lifolder3 \
	$(TRAYAPP_LIBS)

#ifolder3_applet_LDADD = \
#	$(top_srcdir)/src/libifolder/libifolder-trayapp.a \
#	$(top_srcdir)/src/core/libifolder3-core.a

#install-exec-local:
#	$(mkinstalldirs) $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-downloading.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-idle.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-shutting-down.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-starting-up.png $(IMAGE_INSTALL_DIR)
#	$(INSTALL_PROGRAM) $(IMAGE_DIR)/22x22/ifolder-uploading.png $(IMAGE_INSTALL_DIR)

#install-data-hook:
#	@-if test -z "$(DESTDIR)"; then \
#		echo "Updating Gtk icon cache."; \
#		$(gtk_update_icon_cache); \
#	else \
#		echo "*** Icon cache not updated.  After install, run this:"; \
#		echo "***   $(gtk_update_icon_cache)"; \
#	fi

#uninstall-local:
#	cd $(IMAGE_INSTALL_DIR); rm -f ifolder-*.png

maintainer-clean-local:
	rm -f Makefile.in

EXTRA_DIST = $(icon_DATA)
