INCLUDES =			\
	-I$(srcdir)/../..	\
	-I$(top_srcdir)		\
	-I$(top_builddir)

noinst_LIBRARIES = libipcserver.a

#override CFLAGS =

DEFINES = \
	$(QT4_DEFINES)				\
	-DQT_CORE_LIB				\
	-DQT_SHARED

libipcserver_a_CXXFLAGS =	\
	$(DEFINES)					\
	$(QT4_CFLAGS)				\
	$(QT4_CORE_CFLAGS)

libipcserver_a_LIBFLAGS =	\
	$(QT4_LIBS)				\
	$(QT4_CORE_LIBS)

libipcserver_a_SOURCES = \
	IFIPCServer.cpp			\
	IFIPCServer.h

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
