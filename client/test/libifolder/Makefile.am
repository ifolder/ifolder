#.PHONY: test

#test:
#	echo "FIXME: Add code here to run the unit tests"

INCLUDES = \
	-I$(top_srcdir)/src/libifolder

test_libifolder_CXXFLAGS = \
	$(CPPUNIT_CFLAGS)

bin_PROGRAMS = test-libifolder

test_libifolder_SOURCES = \
	Main.cpp					\
	IFClientTest.h		\
	IFClientTest.cpp	\
	IFDomainTest.h			\
	IFDomainTest.cpp

test_libifolder_LDFLAGS = \
	-L$(top_srcdir)/src/libifolder/ipc-client/.libs \
	-lifolder \
	$(CPPUNIT_LIBS) \
	$(QT4_CORE_LIBS)
	

# get these added on for OS X
#	-F$(QT4_PATH)/lib
#	-framework QtCore

	
#	$(top_srcdir)/src/libifolder/common/libifolder-common.a
#	$(top_srcdir)/src/libifolder/ipc-server/libipcserver/libipcserver.a
#	$(top_srcdir)/src/libifolder/ipc-server/libsoap/libsoap.a
#	$(top_srcdir)/src/libifolder/ipc-server/libsync/libsync.a
#	$(top_srcdir)/src/libifolder/ipc-server/libifolder-trayapp.a

	
