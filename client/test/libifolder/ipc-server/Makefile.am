INCLUDES = -I$(top_srcdir)/src/libifolder

bin_PROGRAMS = test-ipc-server

test_ipc_server_LDFLAGS = \
	$(top_srcdir)/src/libifolder/ipc-server/libipcserver/libipcserver.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsoap/libsoap.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsync/libsync.a \
	$(top_srcdir)/src/libifolder/ipc-server/libifolder-trayapp.a

test_ipc_server_SOURCES = \
	test-ipc-server.c
