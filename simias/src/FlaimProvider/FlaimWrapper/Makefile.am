if WINDOWS
CUR_DIR := $(shell pwd)

FlaimWrapper_HFILES = \
	CSPObjectIterator.h \
	CSPropertyIterator.h \
	CSPStore.h \
	CSPStoreObject.h \
	CSPType.h \
	FlaimWrapper.h \
	resource.h
	
FlaimWrapper_SRCFILES = \
	$(FlaimWrapper_HFILES) \
	CSPObjectIterator.cpp \
	CSPropertyIterator.cpp \
	CSPStore.cpp \
	CSPStoreObject.cpp \
	FlaimWrapper.cpp

FlaimWrapper_OBJFILES = \
	CSPObjectIterator$(OBJ_EXT) \
	CSPropertyIterator$(OBJ_EXT) \
	CSPStore$(OBJ_EXT) \
	CSPStoreObject$(OBJ_EXT) \
	FlaimWrapper$(OBJ_EXT)

EXTRA_DIST = $(FlaimWrapper_SRCFILES)


FlaimWrapper_Target = $(LIB_PREF)FlaimWrapper$(SHARED_LIB_EXT)
TARGETS = $(FlaimWrapper_Target)
OBJS = $(FlaimWrapper_OBJFILES)

all: $(TARGETS)


$(FlaimWrapper_Target): $(OBJS)
	$(LD) $^ $(LDOUT)$@ $(MYLDFLAGS)

%$(OBJ_EXT) : %.cpp 
	$(CXX) $(CXX_FLAGS) $<

install-exec-local: $(FlaimWrapper_Target)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(FlaimWrapper_Target) $(DEBUG_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f $(FlaimWrapper_Target) $(DEBUG_FILES)

CLEAN_FILES = $(FlaimWrapper_Target) $(OBJS)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
 
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

if DEBUG
CXX_FLAGS=/nologo /MTd /W3 /Gm /GX /ZI /Od /I "..\..\..\external\flaim\include" /D "WIN32" /D "DEBUG" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "FLAIMWRAPPER_EXPORTS" /FD /GZ /c 
MYLDFLAGS=rpcrt4.lib ws2_32.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib imagehlp.lib flm.lib /nologo /dll /incremental:yes /pdb:"FlaimWrapper.pdb" /debug /machine:I386 /nodefaultlib:"LIBCMTD.lib" /implib:"FlaimWrapper.lib" /libpath:"..\..\..\external\flaim\nt\release"
else 
CXX_FLAGS=/nologo /MT /W3 /GX /O2 /I "..\..\..\external\flaim\include" /D "WIN32" /D "_MBCS" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "FLAIMWRAPPER_EXPORTS" /FD /c 
MYLDFLAGS=rpcrt4.lib ws2_32.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib flm.lib /nologo /dll /incremental:no /pdb:"FlaimWrapper.pdb" /machine:I386 /nodefaultlib:"LIBCMT.lib" /implib:"FlaimWrapper.lib" /libpath:"..\..\..\external\flaim\nt\release"
endif


else
lib_LTLIBRARIES = libFlaimWrapper.la

libFlaimWrapper_la_SOURCES = 	CSPObjectIterator.cpp \
								CSPropertyIterator.cpp \
								CSPStore.cpp \
								CSPStoreObject.cpp \
								FlaimWrapper.cpp \
								CSPObjectIterator.h \
								CSPropertyIterator.h \
								CSPStore.h \
								CSPStoreObject.h \
								CSPType.h \
								FlaimWrapper.h \
								resource.h

if LINUX
libFlaimWrapper_la_CXXFLAGS = -I "../../../external/flaim/include" -fshort-wchar -DFLAIMWRAPPER_EXPORTS -DUNIX -D_REENTRANT -c

libFlaimWrapper_la_LDFLAGS = -L"../../../external/flaim/lxx86/gcc3/release" -lflm

install-exec-local: libFlaimWrapper.la
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	if test -e $(DESTDIR)$(webbindir)/FlaimWrapper$(SHARED_LIB_EXT); then \
		rm $(DESTDIR)$(webbindir)/FlaimWrapper$(SHARED_LIB_EXT); \
	fi;
	ln -s $(libdir)/libFlaimWrapper$(SHARED_LIB_EXT) $(DESTDIR)$(webbindir)/FlaimWrapper$(SHARED_LIB_EXT)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f FlaimWrapper$(SHARED_LIB_EXT)

endif

if DARWIN
libFlaimWrapper_la_CXXFLAGS = -I"../../../external/flaim/macosx/include" -fshort-wchar -DFLAIMWRAPPER_EXPORTS -DUNIX -D_REENTRANT -DOSX -c

libFlaimWrapper_la_LDFLAGS = -L"../../../external/flaim/macosx/release" -lflm

install-exec-local: libFlaimWrapper.la
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	if test -e $(DESTDIR)$(webbindir)/FlaimWrapper; then \
		rm $(DESTDIR)$(webbindir)/FlaimWrapper; \
	fi;
	ln -s $(DESTDIR)$(libdir)/libFlaimWrapper.dylib $(DESTDIR)$(webbindir)/FlaimWrapper

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f FlaimWrapper

endif

endif
	
