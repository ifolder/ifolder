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


if DARWIN
$(FlaimWrapper_Target): $(OBJS)
	ranlib $(FLAIMLIBDIR)/libflm.a
	ranlib $(FLAIMLIBDIR)/libsmiflm.a
	$(CXX) -dynamiclib $^ $(LDOUT)$@ $(MYLDFLAGS)
else
$(FlaimWrapper_Target): $(OBJS)
	$(LD) $^ $(LDOUT)$@ $(MYLDFLAGS)
endif



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

if WINDOWS
CXX_FLAGS=/nologo /MT /W3 /GX /O2 /I "..\..\..\external\flaim\include" /D "WIN32" /D "_MBCS" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "FLAIMWRAPPER_EXPORTS" /FD /c 
MYLDFLAGS=rpcrt4.lib ws2_32.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib flm.lib /nologo /dll /incremental:no /pdb:"FlaimWrapper.pdb" /machine:I386 /nodefaultlib:"LIBCMT.lib" /implib:"FlaimWrapper.lib" /libpath:"..\..\..\external\flaim\nt\release"
endif


if LINUX
	build_os = linux
	platform = unix
CXX_FLAGS= -I "../../../external/flaim/include" -fshort-wchar -DUNIX -D_REENTRANT -DFLAIMWRAPPER_EXPORTS -c
MYLDFLAGS=-L "../../../external/flaim/lxx86/gcc3/release" -lflm -lstdc++ -lpthread -shared
endif


if DARWIN
FLAIMLIBDIR="../../../external/flaim/macosx/release"
CXX_FLAGS= -I"../../../external/flaim/macosx/include" -fshort-wchar -DUNIX -D_REENTRANT -DFLAIMWRAPPER_EXPORTS -DOSX -c
MYLDFLAGS=-dynamiclib -L$(FLAIMLIBDIR) -lflm -lstdc++ -lpthread -dylib
endif



