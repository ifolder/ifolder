if WINDOWS
CUR_DIR := $(shell pwd)

simias_manager_HFILES = \
	simias-manager.h
	
simias_manager_SRCFILES = \
	$(simias_manager_HFILES) \
	simias-manager.c

simias_manager_OBJFILES = \
	simias-manager$(OBJ_EXT)

EXTRA_DIST = $(simias_manager_SRCFILES)


simias_manager_Target = $(LIB_PREF)simias-manager.lib
TARGETS = $(simias_manager_Target)
OBJS = $(simias_manager_OBJFILES)

all: $(TARGETS)


$(simias_manager_Target): $(OBJS)
	-lib $^ $(LDOUT)$@ $(MYLDFLAGS)

%$(OBJ_EXT) : %.c
	$(CXX) $(CXX_FLAGS) $<
	
CLEAN_FILES = $(simias_manager_Target) $(OBJS) *.idb *.pdb

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

if DEBUG
CXX_FLAGS=/Od /D "WIN32" /D "_DEBUG" /D "_LIB" /D "_MBCS" /MTd /W3 /nologo /c /ZI /TC
MYLDFLAGS=/nologo /machine:I386 /nodefaultlib:"LIBCMTD.lib"
else
CXX_FLAGS=/O2 /D "WIN32" /D "NDEBUG" /D "_LIB" /D "_MBCS" /MT /W3 /nologo /c /Zi /TC
MYLDFLAGS=/nologo /machine:I386 /nodefaultlib:"LIBCMT.lib"
endif


else
lib_LTLIBRARIES = libsimias-manager.la

libsimias_manager_la_SOURCES = 	simias-manager.c \
				simias-manager.h

if LINUX
libsimias_manager_la_CXXFLAGS = -I "../../../external/flaim/include" -fshort-wchar -c

libsimias_manager_la_LDFLAGS = -L"../../../external/flaim/lxx86/gcc3/release"

install-exec-local: libsimias-manager.la
	$(mkinstalldirs) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f libsimias-manager$(SHARED_LIB_EXT)

endif

#if DARWIN
#libFlaimWrapper_la_CXXFLAGS = -I"../../../external/flaim/macosx/include" -fshort-wchar -DFLAIMWRAPPER_EXPORTS -DUNIX -D_REENTRANT -DOSX -c
#
#libFlaimWrapper_la_LDFLAGS = -L"../../../external/flaim/macosx/release" -lflm
#
#install-exec-local: libFlaimWrapper.la
#	$(mkinstalldirs) $(DESTDIR)$(webbindir)
#	if test -e $(DESTDIR)$(webbindir)/FlaimWrapper; then \
#		rm $(DESTDIR)$(webbindir)/FlaimWrapper; \
#	fi;
#	ln -s $(DESTDIR)$(libdir)/libFlaimWrapper.dylib $(DESTDIR)$(webbindir)/FlaimWrapper
#
#uninstall-local:
#	cd $(DESTDIR)$(webbindir); rm -f FlaimWrapper
#
#endif

endif
	
