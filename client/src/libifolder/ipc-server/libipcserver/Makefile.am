noinst_LIBRARIES = libipcserver.a
#lib_LTLIBRARIES=libipcserver.la

#override CFLAGS =

INCLUDES =			\
	-I$(srcdir)/../..	\
	-I$(top_srcdir)		\
	-I$(top_builddir)

DEFINES = \
	$(QT4_DEFINES)				\
	-DQT_NETWORK					\
	-DQT_CORE_LIB				\
	-DQT_SHARED

libipcserver_a_SOURCES = \
	IFIPCServer.cpp			\
	IFIPCServer.h

libipcserver_a_CXXFLAGS =	\
	$(DEFINES)					\
	$(QT4_CFLAGS)				\
	$(QT4_CORE_CFLAGS)

libipcserver_a_LIBADD =	\
	$(QT4_LIBS)				\
	$(QT4_CORE_LIBS)

#install-exec-local:
#	$(mkinstalldirs) $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolderStub.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolder.nsmap $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) stdsoap2.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolderH.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) libifsoap.h $(DESTDIR)$(includedir)/ifolder3

#uninstall-local:
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolderStub.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolder.nsmap
#	rm -f $(DESTDIR)$(includedir)/ifolder3/stdsoap2.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolderH.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/libifsoap.h

#clean-local:
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
