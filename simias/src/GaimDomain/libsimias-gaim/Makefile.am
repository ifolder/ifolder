SIMIAS_WSDL = ../GaimDomainService.wsdl

EXTRA_DIST =

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep simiasgaimClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`


lib_LTLIBRARIES = libsimiasgaim.la

override CFLAGS =

libsimiasgaim_la_SOURCES = \
 	envC.c simiasgaimClientLib.c stdsoap2.c

pkginclude_HEADERS = \
	simiasgaimStub.h simiasgaim.nsmap stdsoap2.h simiasgaimH.h

libsimiasgaim_la_CFLAGS = \
	-DWITH_NONAMESPACES

envC.c: $(srcdir)/stdsoap2.c
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -c -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.c:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.c $(srcdir)

$(srcdir)/libsimiasgaim.h: $(srcdir)/stdsoap2.c $(SIMIAS_WSDL)
	$(GSOAP_PATH)/wsdl2h -c -o $@ $(SIMIAS_WSDL)

$(srcdir)/simiasgaimClientLib.c: $(srcdir)/libsimiasgaim.h
	$(GSOAP_PATH)/soapcpp2 -c -n -psimiasgaim $(srcdir)/libsimiasgaim.h

clean-local:
	rm -f stdsoap2.* *.xml simias* env* soap*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
