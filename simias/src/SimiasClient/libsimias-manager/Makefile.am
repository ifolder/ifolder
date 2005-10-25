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
lib_LIBRARIES = libsimias-manager.a

libsimias_manager_a_SOURCES = 	simias-manager.c \
				simias-manager.h

if LINUX

libsimias_manager_a_CXXFLAGS = -c

install-exec-local: libsimias-manager.a
	$(mkinstalldirs) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f libsimias-manager.a

endif

if DARWIN
libsimias_manager_a_CXXFLAGS = -DUNIX -D_REENTRANT -DOSX -c

install-exec-local: libsimias-manager.a
	$(mkinstalldirs) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f libsimias-manager.a

endif

endif
	
