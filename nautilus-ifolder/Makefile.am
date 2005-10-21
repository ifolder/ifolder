# @INTLTOOL_DESKTOP_RULE@

SUBDIRS = po src

DIST_SUBDIRS = $(SUBDIRS) package

EXTRA_DIST = \
	external \
	intltool-extract.in \
	intltool-merge.in \
	intltool-update.in

DISTCLEANFILES = intl-extract intltool-update intltool-merge

installcheck-summary:
	$(MAKE) -C src $@

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure nautilus-ifolder-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status config.h.in
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache

.PHONY: package package-clean

package:
	$(MAKE) -C package all

package-clean:
	$(MAKE) -C package clean

#	
# Create the ChangeLog(s) directly from the CVS archive
# The ChangeLog should be generated and checked-in manually
# each time a release is made by typing: make ChangeLog
#
# nautilus-ifolder/ChangeLog contains all changes except
# for changes made to nautilus-ifolder/po/*.po files.
#
# nautilus-ifolder/po/ChangeLog contains only changes made
# to nautilus-ifolder/po/*.po files.
#
CVS2CL = external/cvs2cl/cvs2cl.pl
USERMAP = external/cvs2cl/usermap.txt
$(srcdir)/ChangeLog: $(srcdir)/Makefile.am
	if (cd $(srcdir); $(CVS2CL) --separate-header --no-wrap --revisions --utc --usermap $(USERMAP) --ignore ^po\/.*\.po$ ); then \
	rm -f $(srcdir)/ChangeLog.bak \
	else true; fi
	if (cd $(srcdir)/po; ../$(CVS2CL) --separate-header --no-wrap --revisions --utc --usermap ../$(USERMAP) --regexp .*\.po$ ); then \
	rm -f $(srcdir)/po/ChangeLog.bak \
	else true; fi
