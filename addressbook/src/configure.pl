#!/usr/bin/perl -w

#***********************************************************************
#*  $RCSfile$
#* 
#*  Copyright (C) 2004 Novell, Inc.
#*
#*  This library is free software; you can redistribute it and/or
#*  modify it under the terms of the GNU General Public
#*  License as published by the Free Software Foundation; either
#*  version 2 of the License, or (at your option) any later version.
#*
#*  This library is distributed in the hope that it will be useful,
#*  but WITHOUT ANY WARRANTY; without even the implied warranty of
#*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#*  Library General Public License for more details.
#*
#*  You should have received a copy of the GNU General Public
#*  License along with this library; if not, write to the Free
#*  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#*
#*  Author: Rob
#* 
#***********************************************************************/

use strict;
use English;
use Getopt::Long;
use POSIX qw(strftime);

# output
my $MAKEFILE = 'config.mk';         # configure makefile

# product information
my $product_name = 'denali';        # default product name
my $major_version = '0';            # default major version
my $minor_version = '5';            # default minor version (iteration)
my $patch_level = '0';              # default patch level
my $build_date = strftime("%Y%m%d", localtime);
                                    # default build date
my $product_version = '';           # default complete product version
my $package_release = '1';          # default package release
my $src_tag = 'src';                # tag for source distribution

# options
my $opt_cpu = '';                   # target cpu
my $opt_debug = '';                 # compile and build with debug flags
my $opt_no_doc = '';                # compile without comment doc
my $opt_platform = '';              # build platform
my $opt_shell = '';                 # build shell
my $opt_build_os = '';              # build os
my $opt_target_os = '';             # target os
my $opt_tools = '';                 # compile and build tool set
my $opt_help = '';                  # show usage

# install options (should match Autoconf configure --help options)
#my $prefix = '';          # PREFIX, architecture-independent files [/usr/local]
#my $exec-prefix = '';     # EPREFIX, architecture-dependent files [PREFIX]
#my $opt_bindir = '';      # user executables [EPREFIX/bin]
#my $sbindir = '';         # system admin executables [EPREFIX/sbin]
#my $libexecdir = '';      # program executables [EPREFIX/libexec]
#my $datadir = '';         # read-only arch.-independent data [PREFIX/share]
#my $sysconfdir = '';      # read-only single-machine data [PREFIX/etc]
#my $sharedstatedir = '';  # modifiable arch.-indedependent data [PREFIX/com]
#my $localstatedir = '';   # modifiable single-machine data [PREFIX/var]
#my $libdir = '';          # object code libraries [EPREFIX/lib]
#my $includedir = '';      # C header files [PREFIX/include]
#my $oldincludedir = '';   # C header files for non-gcc [/usr/include]
#my $infodir = '';         # info documentation [PREFIX/info]
#my $mandir = '';          # man documentation [PREFIX/man]

#-----------------------------------------------------------------------------
# variables
#-----------------------------------------------------------------------------
my %variables;

#-----------------------------------------------------------------------------
# header
#-----------------------------------------------------------------------------
print <<END;

--------------------------------------------------------------------------------
configure
--------------------------------------------------------------------------------
END

#-----------------------------------------------------------------------------
# show usage
#-----------------------------------------------------------------------------
sub usage()
{
   print <<END;
USAGE: configure.pl [options]

    PRODUCT INFORMATION

    --product-name [value]      Specify the product name
                                    i.e. 'denali'
    --major-version [value]     Specify the major version
                                    i.e. '3'
    --minor-version [value]     Specify the minor version
                                    i.e. '0'
    --patch-level [value]       Specify the patch level
                                    i.e. '0'
    --build-date [value]        Specify the build date
                                    i.e. '20031031'
    --product-version [value]   Specify the product version
                                    i.e. '3.0.0.20031031'
    --package-release [value]   Specify the package release
                                    i.e. '1'

    BUILD OPTIONS

    --debug                     Compile and build with debug flags
    --no-doc                    Compile without comment documentation
    --platform [value]          Specify the build platform
                                    value: mono, dotnet
    --target-os [value]         Specify the target operating system
                                    value: linux, windows, netware
    --tools [value]             Specify the build tools
                                    value: gnu, vs, mw
    -? | --help                 Show usage

END

   exit();
}

#-----------------------------------------------------------------------------
# options
#-----------------------------------------------------------------------------
GetOptions(

    # product information
    'product-name=s'        => \$product_name,
    'major-version=s'       => \$major_version,
    'minor-version=s'       => \$minor_version,
    'patch-level=s'         => \$patch_level,
    'build-date=s'          => \$build_date,
    'product-version=s'     => \$product_version,
    'package-release=s'     => \$package_release,

    # build options
    'cpu=s'                 => \$opt_cpu,
    'debug'                 => \$opt_debug,
    'no-doc'                => \$opt_no_doc,
    'platform=s'            => \$opt_platform,
    'shell=s'               => \$opt_shell,
    'target-os=s'           => \$opt_target_os,
    'tools=s'               => \$opt_tools,
    'help|?'                => \$opt_help,
   );

# check help
usage() if $opt_help;

# lowercase values
$product_name = lc($product_name);
$opt_cpu = lc($opt_cpu);
$opt_platform = lc($opt_platform);
$opt_shell = lc($opt_shell);
$opt_target_os = lc($opt_target_os);
$opt_tools = lc($opt_tools);

# not implemented
if ($opt_cpu || $opt_platform || $opt_shell || $opt_tools)
{
    die "Some of your specified arguments are not implemented.";
}

#-----------------------------------------------------------------------------
# globals
#-----------------------------------------------------------------------------
$variables{'EMPTY'} = '';
$variables{'SPACE'} = '$(EMPTY) $(EMPTY)';

#-----------------------------------------------------------------------------
# product information
#-----------------------------------------------------------------------------
print "Product name: $product_name\n";
$variables{'PRODUCT_NAME'} = $product_name;

# calculate product version
($product_version = "$major_version.$minor_version.$patch_level.$build_date")
    unless $product_version;

print "Product version: $product_version\n";
$variables{'PRODUCT_VERSION'} = $product_version;
$variables{'MAJOR_VERSION'} = $major_version;
$variables{'MINOR_VERSION'} = $minor_version;
$variables{'PATCH_LEVEL'} = $patch_level;
$variables{'BUILD_DATE'} = $build_date;

print "Package release: $package_release\n";
$variables{'PACKAGE_RELEASE'} = $package_release;

$variables{'PRODUCT_NAME_VERSION'} = "$product_name-$product_version";
$variables{'PRODUCT_NAME_VERSION_RELEASE'} = "$product_name-$product_version-$package_release";
$variables{'SOURCE_DIST'} = "$product_name-$product_version-$package_release-$src_tag";
$variables{'ALL_SOURCE_DISTS'} = "$product_name-*-$src_tag";

#-----------------------------------------------------------------------------
# build operating system
#-----------------------------------------------------------------------------
$opt_build_os = lc($OSNAME);

# only use windows
if (($opt_build_os eq 'mswin32') || ($opt_build_os eq 'windows'))
{
   $opt_build_os = 'windows';
}

print "Build operating system: $opt_build_os\n";

$variables{'BUILD_OS'} = $opt_build_os;

#-----------------------------------------------------------------------------
# target operating system
#-----------------------------------------------------------------------------
($opt_target_os = $opt_build_os) unless $opt_target_os;

if ($opt_target_os eq 'linux')
{
   # Linux
   ($opt_cpu = 'i686') unless $opt_cpu;
   ($opt_platform = 'mono') unless $opt_platform;
   ($opt_shell = 'bash') unless $opt_shell;
   ($opt_tools = 'gnu') unless $opt_tools;
}
elsif ($opt_target_os eq 'windows')
{
   # Windows
   ($opt_cpu = 'i686') unless $opt_cpu;
   ($opt_platform = 'dotnet') unless $opt_platform;
   ($opt_shell = 'cmd') unless $opt_shell;
   ($opt_tools = 'vs') unless $opt_tools;
}
elsif ($opt_target_os eq 'netware')
{
   # NetWare
   ($opt_cpu = 'i686') unless $opt_cpu;
   ($opt_platform = 'dotnet') unless $opt_platform;
   ($opt_shell = 'cmd') unless $opt_shell;
   ($opt_tools = 'mw') unless $opt_tools;
}
elsif ($opt_target_os eq 'darwin')
{
   # OS X
   ($opt_cpu = 'powerpc') unless $opt_cpu;
   ($opt_platform = 'mint') unless $opt_platform;
   ($opt_shell = 'bash') unless $opt_shell;
   ($opt_tools = 'gnu') unless $opt_tools;
}
else
{
   die "ERROR: Unknown target operating system: $opt_target_os\n";
}

print "Target operating system: $opt_target_os\n";

$variables{'TARGET_OS'} = $opt_target_os;

#-----------------------------------------------------------------------------
# target cpu
#-----------------------------------------------------------------------------
print "Target CPU: $opt_cpu\n";

$variables{'CPU'} = $opt_cpu;

if ($opt_cpu eq 'i686')
{
   # i686
}
elsif ($opt_cpu eq 'powerpc')
{
   # powerpc
}
else
{
   die "ERROR: Unknown target cpu: $opt_cpu\n";
}

#-----------------------------------------------------------------------------
# build type
#-----------------------------------------------------------------------------
my $build_type = '';

# common
$variables{'CSCFLAGS'} = '/warn:4 /d:TRACE';

if ($opt_debug)
{
   # debug
   $build_type = "debug";

   $variables{'DEBUG'} = 'YES';
   $variables{'SLN_CONFIG'} = 'Debug';
   $variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /debug+ /d:DEBUG";
}
else
{
   # release
   $build_type = "final";

   $variables{'DEBUG'} = '';
   $variables{'SLN_CONFIG'} = 'Release';
   $variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /optimize+";
}

print "Build type: $build_type\n";

#-----------------------------------------------------------------------------
# doc
#-----------------------------------------------------------------------------
if ($opt_no_doc)
{
   $variables{'NDOC_FLAG'} = '';
}
else
{
   $variables{'NDOC_FLAG'} = 'true';
}

#-----------------------------------------------------------------------------
# build shell
#-----------------------------------------------------------------------------
print "Build shell: $opt_shell\n";

$variables{'SHELL'} = $opt_shell;
$variables{'CVS'} = 'cvs';

# bash
if ($opt_shell eq 'bash')
{
    # this needs to be changed but for now, assume the simias
    # diretory is up one directory from our root directory
    chdir('../..');
    my $cvs_root_dir = `pwd`;
    chomp($cvs_root_dir);

    # directories
    # chdir('..');
    my $root_dir = "$cvs_root_dir/addressbook";
    chomp($root_dir);
    $variables{'ROOTDIR'} = $root_dir;
    $variables{'SRCDIR'} = "$root_dir/src";
    $variables{'TOOLDIR'} = "$root_dir/tools";
    $variables{'DOCDIR'} = "$variables{'SRCDIR'}/doc";
    $variables{'APIDOCDIR'} = "$variables{'SRCDIR'}/api-doc";
    $variables{'STAGE_DIR'} = "$variables{'SRCDIR'}/stage";
    $variables{'SIMIAS_ROOT'} = "$cvs_root_dir/simias";
    chdir($variables{'SRCDIR'});

    # commands
    $variables{'RM'} = 'rm -f';
    $variables{'CP'} = 'cp -f';
    $variables{'CP_R'} = 'cp -f -r';
    $variables{'MV'} = 'mv -f';
    $variables{'MKDIR'} = 'mkdir';
    $variables{'RMDIR'} = 'rm -rf';
    $variables{'SEP'} = '/';
    $variables{'ECHO_BLANK_LINE'} = "\@echo ''";
}
# cmd
elsif ($opt_shell eq 'cmd')
{
    # this needs to be changed but for now, assume the simias
    # diretory is up one directory from our root directory
    chdir('..\\..');
    my $cvs_root_dir = `cd`;
    chomp($cvs_root_dir);

    # directories
    # chdir('..');
    my $root_dir = "$cvs_root_dir\\addressbook";
    chomp($root_dir);
    $variables{'ROOTDIR'} = $root_dir;
    $variables{'SRCDIR'} = "$root_dir\\src";
    $variables{'TOOLDIR'} = "$root_dir\\tools";
    $variables{'DOCDIR'} = "$variables{'SRCDIR'}\\doc";
    $variables{'APIDOCDIR'} = "$variables{'SRCDIR'}\\api-doc";
    $variables{'STAGE_DIR'} = "$variables{'SRCDIR'}\\stage";
    $variables{'SIMIAS_ROOT'} = "$cvs_root_dir\\simias";
    chdir($variables{'SRCDIR'});

    # commands
    $variables{'RM'} = 'del /f /q';
    $variables{'CP'} = 'xcopy /c /v /y';
    $variables{'CP_R'} = 'xcopy /c /v /y /s';
    $variables{'MV'} = 'call move';
    $variables{'MKDIR'} = 'mkdir';
    $variables{'RMDIR'} = 'call rmdir /s /q';
    $variables{'SEP'} = '$(EMPTY)\\$(EMPTY)';
    $variables{'ECHO_BLANK_LINE'} = '@call echo.';
}
else
{
   die "ERROR: Unknown build shell: $opt_shell\n";
}

print "Root directory: $variables{'ROOTDIR'}\n";
print "Simias root   : $variables{'SIMIAS_ROOT'}\n";

#-----------------------------------------------------------------------------
# build platform
#-----------------------------------------------------------------------------
print "Build platform: $opt_platform\n";

$variables{'PLATFORM'} = $opt_platform;

# common flags across Mono and DotNet
$variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /d:PRODUCT_NAME=$product_name /d:PRODUCT_VERSION=$product_version";

if ($opt_platform eq 'mono')
{
   # mono
   $variables{'CSC'} = 'mcs';

   # tools
   $variables{'FXCOP'} = '@echo FxCop not available on Mono \#';
   $variables{'NDOC'} = 'mono $(TOOLDIR)/NDoc/bin/NDocConsole.exe';
   
   if ($opt_debug)
   {
      $variables{'NUNIT'} = 'mono --debug $(TOOLDIR)/NUnit/bin/nunit-console.exe';
   }
   else
   {
      $variables{'NUNIT'} = 'mono $(TOOLDIR)/NUnit/bin/nunit-console.exe';
   }
   
   $variables{'SLN2MK'} = 'mono $(TOOLDIR)/sln2mk/sln2mk.exe';
   $variables{'REPORT'} = 'mono $(TOOLDIR)/Report/Report.exe';

   # flags
   $variables{'FXCOP_FLAGS'} = '';
   $variables{'NDOC_FLAGS'} = '';
   $variables{'NUNIT_FLAGS'} = '/nologo';
   $variables{'ICON_FLAG'} = '/resource:';
   $variables{'ICON_EXT'} = '.ico'; # '.xmp';

   # work-around casing issue
   $variables{'SYSTEM_XML'} = 'System.Xml.dll';

   # system libraries
   $variables{'VPATH'} = '/usr/lib /opt/gnome2/lib /usr/local/lib';

   # system path
   $variables{'MONO_PATH'} = '$(STAGE_DIR)';
   
   # add a mono define
   $variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /d:MONO";
   
   # set up file archiver commands
   $variables{'ZIP_CREATE'}  = 'tar -czf';
   $variables{'ZIP_EXTRACT'} = 'tar -xzf';
   $variables{'ZIP_LIST'}    = 'tar -tzvf';
   $variables{'ZIP_EXT'}     = 'tar.gz';
}
elsif ($opt_platform eq 'mint')
{
   # mint
   $variables{'CSC'} = 'mcs';

   # tools
   $variables{'FXCOP'} = '@echo FxCop not available on Mono \#';
   $variables{'NDOC'} = 'mint $(TOOLDIR)/NDoc/bin/NDocConsole.exe';
   $variables{'NUNIT'} = 'mint $(TOOLDIR)/NUnit/bin/nunit-console.exe';
   $variables{'SLN2MK'} = 'mint $(TOOLDIR)/sln2mk/sln2mk.exe';
   $variables{'REPORT'} = 'mint $(TOOLDIR)/Report/Report.exe';

   # flags
   $variables{'FXCOP_FLAGS'} = '';
   $variables{'NDOC_FLAGS'} = '';
   $variables{'NUNIT_FLAGS'} = '/nologo';
   $variables{'ICON_FLAG'} = '/resource:';
   $variables{'ICON_EXT'} = '.ico'; # '.xmp';

   # work-around casing issue
   $variables{'SYSTEM_XML'} = 'System.Xml.dll';

   # system libraries
   $variables{'VPATH'} = '/usr/mono/lib /opt/gnome2/lib';

   # system path
   $variables{'MONO_PATH'} = '$(STAGE_DIR)';
   
   # add a mono define
   $variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /d:MINT";
   
   # set up file archiver commands
   $variables{'ZIP_CREATE'}  = 'tar -cf';
   $variables{'ZIP_EXTRACT'} = 'tar -xf';
   $variables{'ZIP_LIST'}    = 'tar -tvf';
   $variables{'ZIP_EXT'}     = 'tar.gz';
}
elsif ($opt_platform eq 'dotnet')
{
   # dotnet
   $variables{'CSC'} = 'csc';

   # nologo
   $variables{'CSCFLAGS'} = "/nologo $variables{'CSCFLAGS'}";

   # tools
   $variables{'FXCOP'} = '$(TOOLDIR)\FxCop\FxCopCmd.exe';
   $variables{'NDOC'} = '$(TOOLDIR)\NDoc\bin\NDocConsole.exe';
   $variables{'NUNIT'} = '$(TOOLDIR)\NUnit\bin\nunit-console.exe';
   $variables{'SLN2MK'} = '$(TOOLDIR)\sln2mk\sln2mk.exe';
   $variables{'REPORT'} = '$(TOOLDIR)\Report\Report.exe';

   # flags
   $variables{'FXCOP_FLAGS'} = '/summary /rule:$(TOOLDIR)\FxCop\Rules';
   $variables{'NDOC_FLAGS'} = '';
   $variables{'NUNIT_FLAGS'} = '/nologo';
   $variables{'ICON_FLAG'} = '/win32icon:';
   $variables{'ICON_EXT'} = '.ico';

   # work-around casing issue
   $variables{'SYSTEM_XML'} = 'System.XML.dll';

   # system libraries
   $variables{'VPATH'} = '$(FrameworkDir)\$(FrameworkVersion)';
   
   # add a mono define
   $variables{'CSCFLAGS'} = "$variables{'CSCFLAGS'} /d:DOTNET";
   # set up file archiver commands
   $variables{'ZIP_CREATE'}  = '$(TOOLDIR)/7-Zip/7z.exe a -tzip -r';
   $variables{'ZIP_EXTRACT'} = '$(TOOLDIR)/7-Zip/7z.exe x';
   $variables{'ZIP_LIST'}    = '$(TOOLDIR)/7-Zip/7z.exe l';
   $variables{'ZIP_EXT'}     = 'zip';
}
else
{
   die "ERROR: Unknown build platform: $opt_platform\n";
}

#-----------------------------------------------------------------------------
# build tools
#-----------------------------------------------------------------------------
print "Build tools: $opt_tools\n";

$variables{'TOOLS'} = $opt_tools;

if ($opt_tools eq 'gnu')
{
   # gnu
   $variables{'CC'} = 'cc -c';
   $variables{'CDEF'} = '-D';
   $variables{'COUT'} = '-o';
   $variables{'CINC'} = '-I';
   $variables{'CXX'} = 'c++ -c';
   $variables{'LIBS'} = '';
   $variables{'LD'} = 'c++';
   $variables{'CFLAGS'} = "-pipe -Wall -W -DLINUX -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'CXXFLAGS'} = "-pipe -Wall -W -DLINUX -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'LDFLAGS'} = '';
   $variables{'LDOUT'} = '-o ';
   $variables{'LDINC'} = '-L';
   $variables{'LIBFLAG'} = '-l';
   $variables{'LIB_EXT'} = '';
   $variables{'EXE_EXT'} = '';
   $variables{'OBJ_EXT'} = '.o';
   $variables{'SHARED_LIB_EXT'} = '.so';
   $variables{'LIB_PRE'} = 'lib';
   $variables{'SHARED_LIB_FLAG'} = '-shared';

   if ($opt_debug)
   {
      # debug
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -g -DDEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -g -DDEBUG";
      $variables{'LDFLAGS'} = "$variables{'LDFLAGS'} -g";
   }
   else
   {
      # final
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -O2 -DNDEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -O2 -DNDEBUG";
   }
}
elsif ($opt_tools eq 'vs')
{
   # vs
   $variables{'CC'} = 'cl -c';
   $variables{'CDEF'} = '-D';
   $variables{'COUT'} = '-Fo';
   $variables{'CINC'} = '-I';
   $variables{'CXX'} = 'cl -c';
   $variables{'LIBS'} = 'kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib rpcrt4.lib ws2_32.lib imagehlp.lib';
   $variables{'LD'} = 'link';
   $variables{'CFLAGS'} = "-nologo -W3 -DWIN32 -D_WINDOWS -DWINDOWS -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'CXXFLAGS'} = "-nologo -GX -W3 -DWIN32 -D_WINDOWS -DWINDOWS -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'LDFLAGS'} = '-nologo -machine:X86';
   $variables{'LDOUT'} = '-out:';
   $variables{'LDINC'} = '-libpath:';
   $variables{'LIBFLAG'} = '';
   $variables{'LIB_EXT'} = '.lib';
   $variables{'EXE_EXT'} = '.exe';
   $variables{'OBJ_EXT'} = '.obj';
   $variables{'SHARED_LIB_EXT'} = '.dll';
   $variables{'LIB_PRE'} = '';
   $variables{'SHARED_LIB_FLAG'} = '-dll';
   $variables{'TLBX'} = 'tlbexp';
   $variables{'TYPE_LIB_EXT'} = '.tlb';
   $variables{'RES_EXT'} = '.res';
   $variables{'RC'} = 'rc';
   $variables{'RC_FLAGS'} = '-r';
   $variables{'DEF'} = '-def:';

   if ($opt_debug)
   {
      # debug
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -MDd -Od -Zi -RTC1 -DDEBUG -D_DEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -MDd -Od -Zi -RTC1 -DDEBUG -D_DEBUG";
      $variables{'LDFLAGS'} = "$variables{'LDFLAGS'} -DEBUG";
   }
   else
   {
      # final
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -MD -O2 -DNDEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -MD -O2 -DNDEBUG";
   }
}
elsif ($opt_tools eq 'mw')
{
   # vs
   $variables{'CC'} = 'mwccnlm -c';
   $variables{'CDEF'} = '-D';
   $variables{'COUT'} = '-Fo';
   $variables{'CINC'} = '-I';
   $variables{'CXX'} = 'cl -c';
   $variables{'LIBS'} = 'libc.imp';
   $variables{'LD'} = 'mwldnlm';
   $variables{'CFLAGS'} = "-nologo -W3 -DNETWARE -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'CXXFLAGS'} = "-nologo -GX -W3 -DNETWARE -DPRODUCT_NAME=$product_name -DPRODUCT_VERSION=$product_version";
   $variables{'LDFLAGS'} = '-nologo -machine:X86';
   $variables{'LDOUT'} = '-out:';
   $variables{'LDINC'} = '-libpath:';
   $variables{'LIBFLAG'} = '';
   $variables{'LIB_EXT'} = '.lib';
   $variables{'EXE_EXT'} = '.exe';
   $variables{'OBJ_EXT'} = '.obj';
   $variables{'SHARED_LIB_EXT'} = '.nlm';
   $variables{'LIB_PRE'} = '';
   $variables{'SHARED_LIB_FLAG'} = '';

   if ($opt_debug)
   {
      # debug
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -MDd -Od -Zi -RTC1 -DDEBUG -D_DEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -MDd -Od -Zi -RTC1 -DDEBUG -D_DEBUG";
      $variables{'LDFLAGS'} = "$variables{'LDFLAGS'} -DEBUG";
   }
   else
   {
      # final
      $variables{'CFLAGS'} = "$variables{'CFLAGS'} -MD -O2 -DNDEBUG";
      $variables{'CXXFLAGS'} = "$variables{'CXXFLAGS'} -MD -O2 -DNDEBUG";
   }
}
else
{
   die "ERROR: Unknown build tools: $opt_tools\n";
}

#-----------------------------------------------------------------------------
# enviornment checks
#-----------------------------------------------------------------------------

# perl version
my $perl_version = '?';
my $output = `perl -V:version`;
if ($output =~ m/version='([.,0-9]+)';/g)
   { $perl_version = $1; }

print "Perl version: $perl_version\n";

# compiler version
my $csc_version = '?';

if ($opt_platform eq 'mono')
{
   # mono
   my $output = `$variables{'CSC'} --version`;
   if ($output =~ m/compiler version ([.,0-9]+)/g)
      { $csc_version = $1; }
}
elsif ($opt_platform eq 'dotnet')
{
   # dotnet
   my $output = `$variables{'CSC'} -?`;
   if ($output =~ m/Compiler version ([.,0-9]+)/g)
      { $csc_version = $1; }
}

print "C# compiler version: $csc_version\n";

my $ccversion = '?';
my $cxxversion = '?';

if ($opt_tools eq 'gnu')
{
   $ccversion = `$variables{'CC'} -dumpversion`;
   chomp($ccversion);

   $cxxversion = `$variables{'CXX'} -dumpversion`;
   chomp($cxxversion);
}
elsif ($opt_tools eq 'vs')
{
   my $output = `$variables{'CC'} 2>&1`;
   if ($output =~ m/Compiler Version ([.,0-9]+)/g)
      { $cxxversion = $ccversion = $1; }
}
elsif ($opt_tools eq 'mw')
{
   my $output = `$variables{'CC'} 2>&1`;
   if ($output =~ m/Compiler Version ([.,0-9]+)/g)
      { $cxxversion = $ccversion = $1; }
}

print "C compiler version: $ccversion\n";
print "C++ compiler version: $cxxversion\n";

#-----------------------------------------------------------------------------
# functions
#-----------------------------------------------------------------------------

# copy the file $(1) to the directory $(2) if the file $(1) exists
#$variables{'CP_IF_EXISTS'} = '$(if $(wildcard $(1)),@$(CP) $(1) $(2),)';
$variables{'CP_IF_EXISTS'} = '-@$(CP) $(1) $(2)';

# remove (delete) the file $(1) if the file $(1) exists
#$variables{'RM_IF_EXISTS'} = '$(if $(wildcard $(1)),@$(RM) $(1),)';
$variables{'RM_IF_EXISTS'} = '-@$(RM) $(1)';

#-----------------------------------------------------------------------------
# create makefile
#-----------------------------------------------------------------------------

print "Generating makefile include: $MAKEFILE\n";

open(OUT, ">$MAKEFILE")
   || die "ERROR: Unable to create file $MAKEFILE.\n";

print OUT <<END;
#=============================================================================
#  $product_name makefile variables
#=============================================================================

END

print OUT <<END;
define HEADER
\$(ECHO_BLANK_LINE)
\@echo --------------------------------------------------------------------------------
\@echo \$(1)
\@echo --------------------------------------------------------------------------------
endef

END

foreach my $key (sort keys %variables)
{
   print OUT "export $key = $variables{$key}\n";
}

close(OUT);

#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.1  2004/02/21 19:01:16  cgaisford
# Initial revision
#
# Revision 1.2  2004/02/21 18:33:20  cgaisford
# Updated themakefiles to use the Simias namespace and find simias using the SIMIAS_ROOT variable
#
# Revision 1.1.1.1  2004/02/21 18:18:17  cgaisford
# Inital checkin
#
# Revision 1.42  2004/02/20 18:11:52  rlyon
# Added and replaced headers.
#
# Revision 1.41  2004/02/20 17:24:26  pthomas
# added SOURCE_DIST variable
#
# Revision 1.40  2004/02/18 21:58:49  rlyon
# Backed-out file check.
#
# Revision 1.39  2004/02/18 21:25:58  rlyon
# Minor bug fixes to the make system.
#
# Revision 1.38  2004/02/18 17:21:27  rlyon
# 1. set the warning level for Linux and Windows C# compiling
# 2. showing the Perl version
#
# Revision 1.37  2004/02/17 21:09:01  pthomas
# incremented minor_version for iteration 5
#
# Revision 1.36  2004/02/12 21:13:33  pthomas
# Added CP_R, MV, ZIP_*, CVS, PRODUCT_NAME_* variables
#
# Revision 1.35  2004/02/03 23:46:24  pthomas
# added APIDOCDIR variable
#
# Revision 1.34  2004/02/03 21:27:53  rlyon
# Added a debug flag for Mono when running NUnit test cases.
#
# Revision 1.33  2004/02/02 23:17:12  rlyon
# Fixed a bug in InviteAgent and SyncManager.
#
# Revision 1.32  2004/01/29 03:55:46  rlyon
# Added the temporary collection work-around for deleting stores.
# Added TRACE to release builds to be match VS.NET.
#
# Revision 1.31  2004/01/28 22:36:20  pthomas
# changed minor_version to 4 to match iteration
#
# Revision 1.30  2004/01/22 01:21:41  rlyon
# 1.  Refactored InviteAgent and added validation.
# 2.  Modification for the AddressBook changes.
# 3.  Update InviteAgent dependant code.
#
# Revision 1.29  2004/01/09 19:20:22  dolds
# add /usr/local/bin to vpath on linux. This is useful when using mono
# built using default paths.
#
# Revision 1.28  2003/12/18 22:07:40  rlyon
# Resources and we are now using the assembly name.
#
# Revision 1.27  2003/12/17 00:16:25  rlyon
# Removed Suse fix, Makefile bug, added mkfile-clean to dist-clean.
#
# Revision 1.26  2003/12/17 00:01:55  rlyon
# A test for Suse
#
# Revision 1.25  2003/12/15 21:53:47  rlyon
# Follow-up build changes for Linux.
#
# Revision 1.24  2003/12/15 21:12:34  rlyon
# Rework of stage for easier compiling on Linux.
#
# Revision 1.23  2003/11/21 16:43:28  cgaisford
# updated the makefile and configure.pl to handle the darwin (OSX) platform
# I also had to move the os independent projects to os dependent because they
# do not build on OS X.
#
# Revision 1.22  2003/11/20 18:25:15  bgetter
# Added imagehlp.lib to libs since this library was added to the filter list in sln2mk.  FlaimWrapper has a dependency on this library for debug builds.
#
# Revision 1.21  2003/11/20 18:08:37  rlyon
# A few minor modifications to sln2mk.exe.
#
# Revision 1.20  2003/11/13 00:00:36  bgetter
# Added variables for .tlb, .rc, and .def files.
#
# Revision 1.19  2003/11/07 06:54:52  rlyon
# Added nodes.
# NetWare enhancements.
#
# Revision 1.18  2003/11/05 21:52:46  rlyon
# Added the Suse Mono Path to the Make VPATH.
#
# Revision 1.17  2003/10/28 00:20:23  rlyon
# Fixed a bug on the location of the api doc area.
#
# Revision 1.16  2003/10/24 18:54:30  rlyon
# still setting up machines.
#
# Revision 1.15  2003/10/24 18:52:50  rlyon
# Another bug fix.
#
# Revision 1.14  2003/10/24 18:46:25  rlyon
# Bug fix in echo.
#
# Revision 1.13  2003/10/24 18:25:47  rlyon
# Cosmetic change to support blank lines on Windows and Linux.
#
# Revision 1.12  2003/10/21 21:53:28  rlyon
# Clean-up the directories from configure.pl.
#
# Revision 1.11  2003/10/15 16:43:38  rlyon
# Fixed the time format to also work on Windows.
#
# Revision 1.10  2003/10/15 16:06:29  rlyon
# Fixed a few bugs in the new product information code.
#
# Revision 1.9  2003/10/15 04:58:53  rlyon
# 1.  Added product information per Paul's proposal.
# 2.  Cleaned up the target headers and consolidated.
# 3.  Removed the compiler checking and the --skip-checks options.
# 4.  Cleaned up the Makefile targets.
#
# Revision 1.8  2003/10/09 23:12:57  rlyon
# Added the report tool.
#
# Revision 1.7  2003/10/06 23:14:52  rlyon
# Updated NUnit reporting for the stage paradigm.
#
# Revision 1.6  2003/10/06 22:55:40  rlyon
# Bug fixes for test cases on Linux.
#
# Revision 1.5  2003/10/06 21:02:05  rlyon
# Created a unit test case stagging area.
#
# Revision 1.4  2003/10/03 23:12:11  rlyon
# Fixes for the fixes.
#
# Revision 1.3  2003/10/03 22:59:01  rlyon
# Linux bug fixes.
#
# Revision 1.2  2003/10/03 22:01:04  rlyon
# Finished .vcproj conversion project.
# Added new mailing lists to website.
#
# Revision 1.1  2003/09/30 02:59:51  rlyon
# ..in with the new.
#
# Revision 1.24  2003/09/29 22:12:51  ryoung
# Added variables LDOUT and SHARED_LIB_EXE
#
# Revision 1.23  2003/09/26 20:27:38  rlyon
# Step one in auto-generating VC++ makefiles.
#
# Revision 1.22  2003/09/26 16:59:55  rlyon
# Created BUILD_OS and TARGET_OS.
#
# Revision 1.21  2003/09/25 22:46:21  pthomas
# Added OSNAME to config.mk.  Fixed copy/paster bug where opt_platform
# was being set to lc($opt_target) when it should have been lc($opt_platform).
#
# Revision 1.20  2003/09/24 20:46:12  rlyon
# Removed funny NDoc directory.
#
# Revision 1.19  2003/09/24 20:35:50  rlyon
# Updated the Example Project to use the Novell.iFolder namespace.
# Added NDoc to the make system.
#
# Revision 1.18  2003/09/24 17:06:33  rlyon
# Fixed some dependencies.
#
# Revision 1.17  2003/09/18 18:00:40  rlyon
# Renamed test and check xml files to Test.xml and Check.xml respectively.
#
# Revision 1.16  2003/09/18 17:53:17  rlyon
# Upgrade to NUnit v2.1
#
# Revision 1.15  2003/09/17 23:01:20  rlyon
# Modifications for FxCop
#
# Revision 1.14  2003/09/17 16:53:10  rlyon
# Added /d:DEBUG and /d:TRACE to a C# debug build.
#
# Revision 1.13  2003/09/16 21:02:41  rlyon
# ExampleApp Win32 bug fixes
#
# Revision 1.12  2003/09/16 16:52:52  rlyon
# Renamed to nightbuild
#
# Revision 1.11  2003/09/15 23:31:31  rlyon
# Platform bug fixes.
#
# Revision 1.10  2003/09/15 20:56:58  rlyon
# System.XML kludge
#
# Revision 1.9  2003/09/15 20:52:24  rlyon
# Wrong directory for sln2mk
#
# Revision 1.8  2003/09/15 20:43:28  rlyon
# Updates to the build and configure steps for C#
#
# Revision 1.7  2003/09/12 17:07:34  rlyon
# Added makefile generation.
#
# Revision 1.6  2003/09/12 15:51:48  rlyon
# Still trying to change the mode.
#
# Revision 1.4  2003/09/12 15:29:21  rlyon
# Changed mode for execution.
#
# Revision 1.2  2003/09/12 15:21:19  rlyon
# Linux Bugfixes
#
# Revision 1.1  2003/09/11 23:29:51  rlyon
# Changed configure to a Perl script.
#
#
#=============================================================================


