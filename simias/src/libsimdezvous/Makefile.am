EXTRA_DIST =

libdir=${webbindir}

lib_LTLIBRARIES = libsimdezvous.la

override CFLAGS =

libsimdezvous_la_SOURCES = simdezvous.c simdezvous.h
 	
#libsimias_la_CFLAGS = \
#	-I$(top_srcdir)
install-exec-local:
	echo Installing
#	@list='$(lib_LTLIBRARIES)'; for p in $$list; do \
#	    p="`echo $$p | sed -e 's|^.*/||'`"; \
#	  echo "mv $(DESTDIR)$(libdir)/$$p" $(DESTDIR)$(webbindir); \
#	  mv $(DESTDIR)$(libdir)/$$p $(DESTDIR)$(webbindir); \
#	done
#mv $(DESTDIR)$(libdir)/$(LIB_PREF)simdezvous.* $(DESTDIR)$(webbindir)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
