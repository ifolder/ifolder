#=============================================================================
#  denali makefile variables
#=============================================================================

define HEADER
$(ECHO_BLANK_LINE)
@echo --------------------------------------------------------------------------------
@echo $(1)
@echo --------------------------------------------------------------------------------
endef

export ALL_SOURCE_DISTS = denali-*-src
export APIDOCDIR = /home/calvin/code/simias/src/api-doc
export BUILD_DATE = 20040220
export BUILD_OS = linux
export CC = cc -c
export CDEF = -D
export CFLAGS = -pipe -Wall -W -DLINUX -DPRODUCT_NAME=denali -DPRODUCT_VERSION=0.5.0.20040220 -O2 -DNDEBUG
export CINC = -I
export COUT = -o
export CP = cp -f
export CPU = i686
export CP_IF_EXISTS = -@$(CP) $(1) $(2)
export CP_R = cp -f -r
export CSC = mcs
export CSCFLAGS = /warn:4 /d:TRACE /optimize+ /d:PRODUCT_NAME=denali /d:PRODUCT_VERSION=0.5.0.20040220 /d:MONO
export CVS = cvs
export CXX = c++ -c
export CXXFLAGS = -pipe -Wall -W -DLINUX -DPRODUCT_NAME=denali -DPRODUCT_VERSION=0.5.0.20040220 -O2 -DNDEBUG
export DEBUG = 
export DOCDIR = /home/calvin/code/simias/src/doc
export ECHO_BLANK_LINE = @echo ''
export EMPTY = 
export EXE_EXT = 
export FXCOP = @echo FxCop not available on Mono \#
export FXCOP_FLAGS = 
export ICON_EXT = .ico
export ICON_FLAG = /resource:
export LD = c++
export LDFLAGS = 
export LDINC = -L
export LDOUT = -o 
export LIBFLAG = -l
export LIBS = 
export LIB_EXT = 
export LIB_PRE = lib
export MAJOR_VERSION = 0
export MINOR_VERSION = 5
export MKDIR = mkdir
export MONO_PATH = $(STAGE_DIR)
export MV = mv -f
export NDOC = mono $(TOOLDIR)/NDoc/bin/NDocConsole.exe
export NDOC_FLAG = true
export NDOC_FLAGS = 
export NUNIT = mono $(TOOLDIR)/NUnit/bin/nunit-console.exe
export NUNIT_FLAGS = /nologo
export OBJ_EXT = .o
export PACKAGE_RELEASE = 1
export PATCH_LEVEL = 0
export PLATFORM = mono
export PRODUCT_NAME = denali
export PRODUCT_NAME_VERSION = denali-0.5.0.20040220
export PRODUCT_NAME_VERSION_RELEASE = denali-0.5.0.20040220-1
export PRODUCT_VERSION = 0.5.0.20040220
export REPORT = mono $(TOOLDIR)/Report/Report.exe
export RM = rm -f
export RMDIR = rm -rf
export RM_IF_EXISTS = -@$(RM) $(1)
export ROOTDIR = /home/calvin/code/simias
export SEP = /
export SHARED_LIB_EXT = .so
export SHARED_LIB_FLAG = -shared
export SHELL = bash
export SLN2MK = mono $(TOOLDIR)/sln2mk/sln2mk.exe
export SLN_CONFIG = Release
export SOURCE_DIST = denali-0.5.0.20040220-1-src
export SPACE = $(EMPTY) $(EMPTY)
export SRCDIR = /home/calvin/code/simias/src
export STAGE_DIR = /home/calvin/code/simias/src/stage
export SYSTEM_XML = System.Xml.dll
export TARGET_OS = linux
export TOOLDIR = /home/calvin/code/simias/tools
export TOOLS = gnu
export VPATH = /usr/lib /opt/gnome2/lib /usr/local/lib
export ZIP_CREATE = tar -czf
export ZIP_EXT = tar.gz
export ZIP_EXTRACT = tar -xzf
export ZIP_LIST = tar -tzvf
