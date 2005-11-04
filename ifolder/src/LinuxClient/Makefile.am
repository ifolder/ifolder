SUBDIRS = library application po

DIST_SUBDIRS = $(SUBDIRS)

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

IMAGE_DIR = $(srcdir)/../images

IMAGE_FILES = $(IMAGE_DIR)/ifolder128.png $(IMAGE_DIR)/inviteduser.png $(IMAGE_DIR)/currentuser.png $(IMAGE_DIR)/ifolderuser.png $(IMAGE_DIR)/serverifolder24.png $(IMAGE_DIR)/ifolder24.png $(IMAGE_DIR)/ifolder-banner-scaler.png $(IMAGE_DIR)/ifolder-banner.png $(IMAGE_DIR)/conflict32.png $(IMAGE_DIR)/ifolder-shutdown.png $(IMAGE_DIR)/ifolder-startup.png $(IMAGE_DIR)/ifolder24.gif $(IMAGE_DIR)/ifolder24-upload.gif $(IMAGE_DIR)/ifolder-crash.png $(IMAGE_DIR)/ifolder-crash.gif $(IMAGE_DIR)/newifolder32.png $(IMAGE_DIR)/newifolder24.png $(IMAGE_DIR)/setup24.png $(IMAGE_DIR)/share24.png $(IMAGE_DIR)/conflict24.png $(IMAGE_DIR)/sync24.png $(IMAGE_DIR)/ifolder48.png $(IMAGE_DIR)/add-account.png $(IMAGE_DIR)/add-emblem.png $(IMAGE_DIR)/add-folder-to-synchronize.png $(IMAGE_DIR)/folder.png $(IMAGE_DIR)/remove-emblem.png $(IMAGE_DIR)/remove-folder.png $(IMAGE_DIR)/search-folder.png $(IMAGE_DIR)/share-emblem.png $(IMAGE_DIR)/share-folder.png $(IMAGE_DIR)/sync-emblem.png $(IMAGE_DIR)/sync-folder.png

EXTRA_DIST =

install-exec-local: $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(datadir)/ifolder3/images $(DESTDIR)$(datadir)/pixmaps
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder3/images

uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder3/images; rm -f $(IMAGE_FILES)
	cd $(DESTDIR)$(datadir)/pixmaps; rm -f ifolder24.png

clean-local:
	rm -rf *.dbg $(COMMON_CLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
