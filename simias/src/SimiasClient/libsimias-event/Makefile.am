lib_LTLIBRARIES = libsimias-event.la

# Override CFLAGS so that -nologo and -W3 are NOT included
override CFLAGS =

libsimias_event_la_SOURCES = \
	simias-event-client.c simias-event-client.h

pkginclude_HEADERS = \
	simias-event-client.h

libsimias_event_la_CFLAGS = \
	$(XML2_CFLAGS)

libsimias_event_la_LDFLAGS = \
	$(XML2_LIBS)

