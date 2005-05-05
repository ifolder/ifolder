EXTRA_DIST = libsimiasbonjour.dll

libdir=${webbindir}

lib_LTLIBRARIES = libsimiasbonjour.la

override CFLAGS =

libsimiasbonjour_la_SOURCES = simiasbonjour.c simiasbonjour.h

if LINUX
libsimiasbonjour_la_CFLAGS = \
	-I./external

libsimiasbonjour_la_LDFLAGS = \
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
