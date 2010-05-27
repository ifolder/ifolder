#!/bin/sh

###
#
# Script to generate tarball for ifolder3 server and building the RPM
# using autobuild
#
###

### TODO
#
# 1. Provide option to run mbuild
# 2. Allow options to be passed to 'build'
# 3. Smart handling of PEBKAC

#
# Stop on errors
set -e

# Generate the build number
BUILDNUM=`expr \`date +%G%j\` - 2000000`

# This script is for packaging sources that will be
# delivered to autobuild.

# This the script should be execute from the directory
# workarea/versioning/trunk/ark-iman/install
PACKAGE_DIR=../
PACKAGE_VER=${PACKAGE_VER:="3.7.1"}
PACKAGE=${PACKAGE:="novell-ifolder-enterprise-plugins"}
SRC_DIR=plugins
TARBALL_NAME=$PACKAGE
NPS_BUILDNUM=`printf "%x%s\n" \`date +%_m\` \`date +%d\` | tr [:lower:] [:upper:]`
RPM_DIR="../rpms/$NPS_BUILDNUM"
HOST_ARCH=`uname -i`
OES2=${OES2:="sles10-oes2-sp2"}

PUB_DIR=x86_64
[ "$HOST_ARCH" = "i386" ] && PUB_DIR=i586

# Env variables for autobuild
#  - Check if BUILD_ROOT and BUILD_DIST have already been set
#  - If they are set, use them 
#  - else, define our own
HOST_DIST=`echo ${BUILD_DIST:="$OES2-$HOST_ARCH"}`
HOST_ROOT=`echo ${BUILD_ROOT:="/tmp/$TARBALL_NAME"}`

mkdir -p $RPM_DIR/{i586,x86_64}

./enterprise-plugins.sh
pushd $PACKAGE

# Check if autobuild is available, else point to wiki
if [ -x /opt/SuSE/bin/build ]
then
	. /opt/SuSE/bin/.profile
	echo "Running autobuild..."
	echo "You might want to set BUILD_DIST and BUILD_ROOT env variables"
	echo "before starting this build. (Refer to README.build for more info)"
	export BUILD_ROOT="$HOST_ROOT"
	export BUILD_DIST="$HOST_DIST"

	# If we are running on a 64bit machine, then build the 32bit RPMs
	# too
	if [ "$HOST_ARCH" = "x86_64" ]
	then
		export BUILD_DIST="$OES2-i386"
		linux32 build $PACKAGE.spec --prefer-rpms=../$RPM_DIR/i586 $ABUILD_OPTS
		cp `find $BUILD_ROOT/usr/src/packages/RPMS/ -name *.rpm` ../$RPM_DIR/i586
		rm -rf $BUILD_ROOT

		# Set BUILD_DIST back to its original value
		export BUILD_DIST="$HOST_DIST"
	fi

	build $PACKAGE.spec --prefer-rpms=../$RPM_DIR/$PUB_DIR $ABUILD_OPTS
else
	echo "##################################################################"
	echo "# You don't have autobuild setup on your machine. Please refer   #"
	echo "# to README.build for the pre-requisites for running this script #"
	echo "##################################################################"
	exit -1
fi
popd

cp `find $BUILD_ROOT/usr/src/packages/RPMS/ -name *.rpm` $RPM_DIR/$PUB_DIR

