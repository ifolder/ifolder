#!/bin/bash

run_versioned() {
    local P
    type -p "$1-$2" &> /dev/null && P="$1-$2" || local P="$1"

    shift 2
    "$P" "$@"
}

if [ "x$1" = "xam" ] ; then
    set -ex
    run_versioned automake 1.7 -a -c --foreign
    ./config.status
else 
    set -ex

    rm -rf autom4te.cache
    rm -f config.cache

    run_versioned aclocal 1.7
    libtoolize -c --force
    intltoolize -c --force
    aclocal
    autoheader
    run_versioned automake 1.7 -a -c --foreign
    autoconf -Wall

    CFLAGS="-g -O0" ./configure --prefix=/usr --sysconfdir=/etc "$@"

    make clean
fi
