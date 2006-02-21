SUBDIRS = NetworkManager-sharp library application bonjour-ui po

DIST_SUBDIRS = $(SUBDIRS)

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

IMAGE_DIR = $(srcdir)/../images

IMAGE_FILES =								\
	$(IMAGE_DIR)/ifolder16.png				\
	$(IMAGE_DIR)/ifolder22.png				\
	$(IMAGE_DIR)/ifolder24.png				\
	$(IMAGE_DIR)/ifolder48.png				\
	$(IMAGE_DIR)/ifolder128.png				\
	$(IMAGE_DIR)/ifolder-add-account48.png	\
	$(IMAGE_DIR)/ifolder-banner.png			\
	$(IMAGE_DIR)/ifolder-banner-scaler.png	\
	$(IMAGE_DIR)/ifolder-download16.png		\
	$(IMAGE_DIR)/ifolder-download22.png		\
	$(IMAGE_DIR)/ifolder-download48.png		\
	$(IMAGE_DIR)/ifolder-error16.png		\
	$(IMAGE_DIR)/ifolder-error22.png		\
	$(IMAGE_DIR)/ifolder-error48.png		\
	$(IMAGE_DIR)/ifolder-sync16.png			\
	$(IMAGE_DIR)/ifolder-sync22.png			\
	$(IMAGE_DIR)/ifolder-sync48.png			\
	$(IMAGE_DIR)/ifolder-upload16.png		\
	$(IMAGE_DIR)/ifolder-upload22.png		\
	$(IMAGE_DIR)/ifolder-upload48.png		\
	$(IMAGE_DIR)/ifolder-user-current16.png	\
	$(IMAGE_DIR)/ifolder-user-owner16.png	\
	$(IMAGE_DIR)/ifolder-waiting16.png		\
	$(IMAGE_DIR)/ifolder-waiting22.png		\
	$(IMAGE_DIR)/ifolder-waiting48.png		\
	$(IMAGE_DIR)/ifolder-warning16.png		\
	$(IMAGE_DIR)/ifolder-warning22.png		\
	$(IMAGE_DIR)/ifolder-warning48.png

schemasdir			= $(GCONF_SCHEMA_FILE_DIR)
schemas_DATA		= ifolder3.schemas

#schemas_in_files	= ifolder3.schemas.in
#schemas_DATA		= $(schemas_in_files:.schemas.in=.schemas)

$(schemas_DATA): $(schemas_DATA).in
	sed -e "s|\@DATADIR\@|$(datadir)|g" $< > $@

# INTLTOOL_SCHEMAS_RULE comes from intltool and merges in the localized strings
# from the *.po files for ifolder3.schemas.in.
# FIXME: We need to figure out how to include INTLTOOL without causing Windows
# and Mac from failing during autogen.sh.
#@INTLTOOL_SCHEMAS_RULE@

#if GCONF_SCHEMAS_INSTALL
install-data-local:
	GCONF_CONFIG_SOURCE="" \
	$(GCONFTOOL) --makefile-install-rule $(schemas_DATA)
#endif

EXTRA_DIST = $(schemas_DATA).in

CLEANFILES = $(schemas_DATA)

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
