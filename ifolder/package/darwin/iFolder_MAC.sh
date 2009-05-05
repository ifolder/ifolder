#!/bin/bash
export dst_root=/Users/Administrator/sandbox/ifolder
MODULE_LIST="Simias NovelliFolder"
SVN_URL=https://svn.provo.novell.com/svn/ifolder/trunk
TMP_DIR="/tmp/ifolder"

if [ -d $dst_root ]
	then
	rm -rf $dst_root
	fi
if [ -d $TMP_DIR ]
	then
	rm -rf $TMP_DIR
	fi

mkdir -p $TMP_DIR
mkdir -p $dst_root

cd $dst_root
for i in $MODULE_LIST
svn co $SVN_URL/$i
done

BUILDLOG="$TMP_DIR/Simias.log"
pushd Simias
make clean;
./autogen.sh --prefix=/opt/novell/ifolder3 --with-runasclient
make > $BUILDLOG 2>$1
err=`cat $BUILDLOG | grep "error" | wc -l`
if [ "$err" -ne 0 ]
     then
     echo " Simias compilation failed. Exiting...."
     exit
     else
     echo " Make succeeded. Proceeding to make install"
     fi
make install >> $BUILDLOG 2>$1
err=`cat $BUILDLOG | grep "error" | wc -l`
if [ "$err" -ne 0 ]
     then
     echo " Make install failed. Exiting....."
     exit
     else
     echo " Simias succeeded. Starting NovelliFolder compilation...."
     popd
     fi

BUILDLOG="$TMP_DIR/NovelliFolder.log"	     
pushd NovelliFolder
make clean; make dist clean
./autogen.sh --prefix=/opt/novell/ifolder3
make > $BUILDLOG 2>$1
err=`cat $BUILDLOG | grep "error" | wc -l`
if [ "$err" -ne 0 ]
     then
     echo " NovelliFolder compilation failed. Exiting...."
     exit
     else
     echo " Make succeeded. Proceeding to make install"
     fi
make install >> $BUILDLOG 2>$1
err=`cat $BUILDLOG | grep "error" | wc -l`
if [ "$err" -ne 0 ]
     then
     echo " Make install failed. Exiting....."
     exit
     else
     echo " NovelliFolder succeeded"
     popd
     fi

if [ -d /ifoldertemp ]
	then
	rm -rf /ifoldertemp
	fi

mkdir -p /ifoldertemp
mv /opt /ifoldertemp/. 


            


