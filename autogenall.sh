#! /bin/sh
INSTALLDIR=$PWD
cd simias;      ./autogen.sh --enable-debug --prefix=$INSTALLDIR; cd ..
cd addressbook; ./autogen.sh --enable-debug --prefix=$INSTALLDIR; cd ..
cd ifolder;     ./autogen.sh --enable-debug --prefix=$INSTALLDIR; cd ..
