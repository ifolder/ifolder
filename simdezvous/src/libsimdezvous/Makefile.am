EXTRA_DIST = libsimdezvous.dll

libdir=${webbindir}

lib_LTLIBRARIES = libsimdezvous.la

override CFLAGS =

libsimdezvous_la_SOURCES = simdezvous.c simdezvous.h

if LINUX
libsimdezvous_la_CFLAGS = \
	-I./external

libsimdezvous_la_LDFLAGS = \
	-ldns_sd
endif

#libsimias_la_CFLAGS = \
#	-I$(top_srcdir)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
