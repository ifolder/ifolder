#!/bin/sh
# Run this to generate all the initial makefiles, etc.

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

ORIGDIR=`pwd`
cd $srcdir
PROJECT=iFolder
TEST_TYPE=-f
FILE=src/WebService/iFolder.cs

DIE=0

# cleanup configure stuff because this failes on Mac
rm -f aclocal.m4
rm -f config.cache
rm -rf autom4te.cache
rm -f config.guess
rm -f config.status
rm -rf config.sub
rm -f config.log

(autoconf --version) < /dev/null > /dev/null 2>&1 || {
	echo
	echo "You must have autoconf installed to compile $PROJECT."
	echo "Download the appropriate package for your distribution,"
	echo "or get the source tarball at ftp://ftp.gnu.org/pub/gnu/"
	DIE=1
}

AUTOMAKE=automake-1.9
ACLOCAL=aclocal-1.9

($AUTOMAKE --version) < /dev/null > /dev/null 2>&1 || {
        AUTOMAKE=automake
        ACLOCAL=aclocal
}

($AUTOMAKE --version) < /dev/null > /dev/null 2>&1 || {
	echo
	echo "You must have automake installed to compile $PROJECT."
	echo "Download the appropriate package for your distribution,"
	echo "or get the source tarball at ftp://ftp.gnu.org/pub/gnu/"
	DIE=1
}

if test "$DIE" -eq 1; then
	exit 1
fi

test $TEST_TYPE $FILE || {
	echo "You must run this script in the top-level $PROJECT directory"
	exit 1
}

if test -z "$*"; then
	echo "I am going to run ./configure with no arguments - if you wish "
        echo "to pass any to it, please specify them on the $0 command line."
fi

case $CC in
*xlc | *xlc\ * | *lcc | *lcc\ *) am_opt=--include-deps;;
esac

for coin in `find $srcdir -name configure.in -maxdepth 1 -print`
do 
  dr=`dirname $coin`
  if test -f $dr/NO-AUTO-GEN; then
    echo skipping $dr -- flagged as no auto-gen
  else
    echo processing $dr
    macrodirs=`sed -n -e 's,AM_ACLOCAL_INCLUDE(\(.*\)),\1,gp' < $coin`
    ( cd $dr
      aclocalinclude="$ACLOCAL_FLAGS"
      for k in $macrodirs; do
  	if test -d $k; then
          aclocalinclude="$aclocalinclude -I $k"
  	##else 
	##  echo "**Warning**: No such directory \`$k'.  Ignored."
        fi
      done
		case $OSTYPE in
			darwin*)
				aclocalinclude="$aclocalinclude -I tools/aclocal"
			;;
		esac
      if grep "^AM_GNU_GETTEXT" configure.in >/dev/null; then
	if grep "sed.*POTFILES" configure.in >/dev/null; then
	  : do nothing -- we still have an old unmodified configure.in
	else
	  echo "Creating $dr/aclocal.m4 ..."
	  test -r $dr/aclocal.m4 || touch $dr/aclocal.m4
	  echo "Running gettextize...  Ignore non-fatal messages."
	  echo "no" | gettextize --force --copy
	  echo "Making $dr/aclocal.m4 writable ..."
	  test -r $dr/aclocal.m4 && chmod u+w $dr/aclocal.m4
        fi
      fi
      if grep "^AM_GNOME_GETTEXT" configure.in >/dev/null; then
	echo "Creating $dr/aclocal.m4 ..."
	test -r $dr/aclocal.m4 || touch $dr/aclocal.m4
	echo "Running gettextize...  Ignore non-fatal messages."
	echo "no" | gettextize --force --copy
	echo "Making $dr/aclocal.m4 writable ..."
	test -r $dr/aclocal.m4 && chmod u+w $dr/aclocal.m4
      fi
      if grep "^AM_GLIB_GNU_GETTEXT" configure.in >/dev/null; then
	echo "Creating $dr/aclocal.m4 ..."
	test -r $dr/aclocal.m4 || touch $dr/aclocal.m4
	echo "Running gettextize...  Ignore non-fatal messages."
	echo "no" | glib-gettextize --force --copy
	echo "Making $dr/aclocal.m4 writable ..."
	test -r $dr/aclocal.m4 && chmod u+w $dr/aclocal.m4
      fi
      if grep "^AM_PROG_LIBTOOL" configure.in >/dev/null; then
	echo "Running libtoolize..."
		case $OSTYPE in
			darwin*)
				glibtoolize --force --copy
			;;
			*)
				libtoolize --force --copy
			;;
		esac
      fi
      echo "Running $ACLOCAL $aclocalinclude ..."
      $ACLOCAL $aclocalinclude
      if grep "^AM_CONFIG_HEADER" configure.in >/dev/null; then
	echo "Running autoheader..."
	autoheader || exit 1
      fi
      echo "Running $AUTOMAKE --gnu $am_opt ..."
      $AUTOMAKE --add-missing --gnu $am_opt
      echo "Running autoconf ..."
      autoconf || exit 1
    )
  fi
done

conf_flags="--config-cache --enable-maintainer-mode --enable-compile-warnings" #--enable-iso-c

cd "$ORIGDIR"

if test x$NOCONFIGURE = x; then
  echo Running $srcdir/configure $conf_flags "$@" ...
  $srcdir/configure $conf_flags "$@" \
  && echo Now type \`make\' to compile $PROJECT  || exit 1
else
  echo Skipping configure process.
fi
