#! /bin/sh
                                                                                                                                
set -x
aclocal
autoheader
libtoolize --copy --force
automake --gnu --add-missing --copy
autoconf

