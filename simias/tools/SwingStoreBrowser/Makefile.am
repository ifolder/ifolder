SUBDIRS = 

DIST_SUBDIRS = $(SUBDIRS)

SRC_FILES = \
	$(srcdir)/src/com/novell/simias/browser/*.java \
	$(srcdir)/src/com/novell/simias/storebrowser/*.java

EXTRA_DIST = \
	external \
	tools \
	$(srcdir)/manifest.in \
	$(SRC_FILES)

BUILD_DIR = ./build

CLASSES_DIR = ./classes

CP1 = \
	$(TOOLDIR)/axis-1_2RC2/lib/axis.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/commons-discovery.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/commons-loggin.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/jaxrpc.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/log4j-1.2.8.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/saaj.jar \
	$(TOOLDIR)/axis-1_2RC2/lib/wsdl4j.jar \
	$(CLASSES_DIR)

CP = $(subst $(SPACE),$(PATH_SEP),$(CP1))

all: SwingStoreBrowser.jar

SwingStoreBrowser.jar: $(SRC_FILES) 
	rm -rf $(CLASSES_DIR)
	$(mkinstalldirs) $(CLASSES_DIR)
	$(JAVAC) $(JAVACFLAGS) -d "$(CLASSES_DIR)" -classpath "$(CP)" $(SRC_FILES)
	$(JAR) cvfm $@ manifest -C $(CLASSES_DIR) .
	chmod 755 storebrowser

CLEAN_FILES = $(CLASSES_DIR) SwingStoreBrowser.jar

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache
