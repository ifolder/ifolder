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

if WINDOWS
if DEBUG
CXX_FLAGS=/nologo /MTd /W3 /Gm /GX /ZI /Od /I "..\..\..\external\flaim\include" /D "WIN32" /D "DEBUG" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "FLAIMWRAPPER_EXPORTS" /FD /GZ /c 
LDFLAGS=rpcrt4.lib ws2_32.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib imagehlp.lib flm.lib /nologo /dll /incremental:yes /pdb:"FlaimWrapper.pdb" /debug /machine:I386 /nodefaultlib:"LIBCMTD.lib" /implib:"FlaimWrapper.lib" /libpath:"..\..\..\external\flaim\nt\debug"
else 
CXX_FLAGS=/nologo /MT /W3 /GX /O2 /I "..\..\..\external\flaim\include" /D "WIN32" /D "_MBCS" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "FLAIMWRAPPER_EXPORTS" /FD /c 
LDFLAGS=rpcrt4.lib ws2_32.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib flm.lib /nologo /dll /incremental:no /pdb:"FlaimWrapper.pdb" /machine:I386 /nodefaultlib:"LIBCMT.lib" /implib:"FlaimWrapper.lib" /libpath:"..\..\..\external\flaim\nt\release"
endif
else

if LINUX
if DEBUG
	build_os = linux
	platform = unix
CXX_FLAGS= -I "../../../external/flaim/include" -fshort-wchar -DDEBUG -DUNIX -D_REENTRANT -DFLAIMWRAPPER_EXPORTS -c
LDFLAGS=-L "../../../external/flaim/lxx86/gcc3/debug" -lflm -lstdc++ -lpthread -shared
else
CXX_FLAGS= -I "../../../external/flaim/include" -fshort-wchar -DUNIX -D_REENTRANT -DFLAIMWRAPPER_EXPORTS -c
LDFLAGS=-L "../../../external/flaim/lxx86/gcc3/release" -lflm -lstdc++ -lpthread -shared
endif
endif
endif

FlaimWrapper_Target = $(LIB_PREF)FlaimWrapper$(SHARED_LIB_EXT)
TARGETS = $(FlaimWrapper_Target)
OBJS = $(FlaimWrapper_OBJFILES)

all: $(TARGETS)

$(FlaimWrapper_Target): $(OBJS)
	$(LD) $^ $(LDOUT)$@ $(LDFLAGS)

%$(OBJ_EXT) : %.cpp 
	$(CXX) $(CXX_FLAGS) $<

install-exec-local: $(FlaimWrapper_Target)
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir) $(DESTDIR)$(datadir)/simias
	$(INSTALL_PROGRAM) $(FlaimWrapper_Target) $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f $(FlaimWrapper_Target) $(DEBUG_FILES)

CLEAN_FILES = $(FlaimWrapper_Target) $(OBJS)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

