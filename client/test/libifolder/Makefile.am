#.PHONY: test

#test:
#	echo "FIXME: Add code here to run the unit tests"

INCLUDES = \
	-I$(top_srcdir)/src/libifolder \
	$(CPPUNIT_CFLAGS)

bin_PROGRAMS = test-libifolder

test_libifolder_SOURCES = \
	Main.cpp					\
	IFiFolderClientTest.h		\
	IFiFolderClientTest.cpp	\
	IFDomainTest.h			\
	IFDomainTest.cpp

test_libifolder_LDFLAGS = \
	$(top_srcdir)/src/libifolder/ipc-server/libipcserver/libipcserver.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsoap/libsoap.a \
	$(top_srcdir)/src/libifolder/ipc-server/libsync/libsync.a \
	$(top_srcdir)/src/libifolder/ipc-server/libifolder-trayapp.a \
	$(CPPUNIT_LIBS) \
	-F$(QT4_PATH)/lib \
	-framework QtCore
