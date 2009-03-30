#!/bin/bash
#/*****************************************************************************
#*
#* Copyright (c) [2009] Novell, Inc.
#* All Rights Reserved.
#*
#* This program is free software; you can redistribute it and/or
#* modify it under the terms of version 2 of the GNU General Public License as
#* published by the Free Software Foundation.
#*
#* This program is distributed in the hope that it will be useful,
#* but WITHOUT ANY WARRANTY; without even the implied warranty of
#* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
#* GNU General Public License for more details.
#*
#* You should have received a copy of the GNU General Public License
#* along with this program; if not, contact Novell, Inc.
#*
#* To contact Novell about this file by physical or electronic mail,
#* you may find current contact information at www.novell.com
#*
#*-----------------------------------------------------------------------------
#*
#*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
#*                 $Modified by: <Modifier>
#*                 $Mod Date: <Date Modified>
#*                 $Revision: 0.0
#*-----------------------------------------------------------------------------
#* This module is used to:
#*        <Description of the functionality of the file >
#*
#*****************************************************************************/


MAX_MANDATORY_ARGS=8
USAGE_ERROR=1
INVALID_ARG=2
SUCCESS=0
LDAP_ERROR=1

IP_ADDRESS=""
PORT=""
ADMIN_DN=""
ADMIN_PWD=""
USER_DN=""
SURNAME=""
USER_PWD=""
IFOLDER_SERV=""
TMP_LDIFF_FILE=tmp.ldiff
LDAP_ERROR_FILE=error.ldapadd

usage()
{
    echo "Usage: $0 -h <Ldap URL> -d <admin DN> -w <admin password> -u <user DN> [-s <surname>] [-c <user password>] [-i <iFolder Home Server>]"
    echo ""
    echo "Example Usage: $0 -h ldaps://10.10.10.10 -d admin,o=novell -w secret -u cn=abc,o=novell -s xyz  -c secret -i 10.10.10.10 "
    echo "        Usage: $0 -h ldap://10.10.10.10 -d admin,o=novell -w secret -u cn=mmm,o=novell -s nnn  -c secret"
    exit $USAGE_ERROR
}

find_arg()
{
    args="-h -d -w -u -s -c -i"
    for arg in $args
    do
        if [ $arg = "$1" ] ; then
            return 0
        fi
    done
    return $INVALID_ARG
}

check_arg()
{
    echo "Arg to function find arg is $1"
    

}

if [ $# -eq 1 ] && [ $1 != "--help" ]; then
    usage
fi

if [ $# -ne 1 ] && [ $# -lt  $MAX_MANDATORY_ARGS ]; then
    usage
fi


index=1
while [ $index -lt $# ]
do
    eval find_arg \${$index}
    
    if [ $? -ne 0 ] ; then
        usage
    fi
    index=`expr $index + 2`
done 


args="-h -d -w -u"
for arg in $args
do
    index=1
    found=0
#    echo "Current iter is for arg $arg"
    while [ $index -lt $# ]
    do
        eval echo \${$index} > .tmp.tmp
        var=`cat .tmp.tmp`
#        echo "Current arg is $var "
        arg_index=`expr $index + 1`
#        echo "Current arg_index is $arg_index Index is $index"
        eval echo \${$arg_index} > .tmp.tmp
        found=1
        if [ "$var" = "$arg" ]; then
            case $var
            in
                -h) 
                     IP_ADDRESS=`cat .tmp.tmp`;;
                -d)
                    ADMIN_DN=`cat .tmp.tmp`;;
                -w)
                    ADMIN_PWD=`cat .tmp.tmp`;;
                -u)
                    USER_DN=`cat .tmp.tmp`;;
            esac    
        else
            case $var
            in
                -h);;
                -d);;
                -w);;
                -u);;
                -s)
                    SURNAME=`cat .tmp.tmp`;;
                -c)
                    USER_PWD=`cat .tmp.tmp`;;
                -i)
                    IFOLDER_SERV=`cat .tmp.tmp`;;
                 *)
                    found=0
                    echo "Invalid Argument"
                    usage
                    ;;
            esac        
        fi    
         index=`expr $index + 2`
    done
    if [ $found -ne 1 ]; then
        echo "Mandatory arg $arg is missing.."
        usage
    fi
done 

CN=`echo $USER_DN | cut -d= -f2 | cut -d, -f1`

if [ "$SURNAME" = "" ]; then
   SURNAME=$CN 
fi

if [ "$USER_PWD" = "" ]; then
   USER_PWD="novell" 
fi

echo ""
#echo "IP $IP_ADDRESS"
#echo "Port $PORT"
#echo "Admin DN $ADMIN_DN"
#echo "Admin pwd $ADMIN_PWD"
echo "User DN: $USER_DN"
echo "Surname: $SURNAME"
echo "CN:  $CN"
echo "SN:  $SURNAME"
#echo "User Pwd $USER_PWD"
if [ "$IFOLDER_SERV" != "" ]; then
	echo "iFolder Home Server: $IFOLDER_SERV"
fi
echo ""


rm .tmp.tmp

echo "dn: $USER_DN" > $TMP_LDIFF_FILE 
echo "cn: $CN" >> $TMP_LDIFF_FILE 
echo "sn: $SURNAME" >> $TMP_LDIFF_FILE 
echo -e "objectClass: inetOrgPerson\nobjectClass: OrganizationalPerson\nobjectClass: Person\nobjectClass: iFolderUserProvision" >> $TMP_LDIFF_FILE
echo "userpassword: $USER_PWD" >> $TMP_LDIFF_FILE
if [ "$IFOLDER_SERV" != "" ]; then
    echo "iFolderHomeServer: $IFOLDER_SERV" >> $TMP_LDIFF_FILE 
fi

result=`ldapadd -H $IP_ADDRESS -x -Z -D $ADMIN_DN -w $ADMIN_PWD -f $TMP_LDIFF_FILE 2> $LDAP_ERROR_FILE`
exitstatus=$?
if [ $exitstatus -eq 0 ]; then
    echo "User created successfully with iFolderUserProvision object class and iFolderHomeServer attributes."
    echo ""
    exit $SUCCESS
fi
if [ $exitstatus -eq 68 ] ; then
    echo "dn: $USER_DN" > $TMP_LDIFF_FILE 
    echo -e "Changetype: modify\nadd: ObjectClass\nObjectClass: iFolderUserProvision" >> $TMP_LDIFF_FILE
    if [ "$IFOLDER_SERV" != "" ]; then
        echo -e "-\nreplace: iFolderHomeServer\niFolderHomeServer: $IFOLDER_SERV" >> $TMP_LDIFF_FILE 
    fi    
    result=`ldapmodify -H $IP_ADDRESS -x -Z -D $ADMIN_DN -w $ADMIN_PWD -f $TMP_LDIFF_FILE 2> $LDAP_ERROR_FILE`
    exitstatus=$?
    if [ $exitstatus -eq 0 ]; then
    	if [ "$IFOLDER_SERV" != "" ]; then
        	echo "iFolderUserProvision object class and iFolderHomeServer attributes are added successfully to $USER_DN."
	else
        	echo "iFolderUserProvision object class added successfully to $USER_DN."
	fi
	rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
    	echo ""
    	exit $SUCCESS
    fi
    if [ $exitstatus -eq 20 ] ; then
    	if [ "$IFOLDER_SERV" != "" ]; then
    		echo "dn: $USER_DN" > $TMP_LDIFF_FILE 
    		echo -e "Changetype: modify\nreplace: iFolderHomeServer\niFolderHomeServer: $IFOLDER_SERV" >> $TMP_LDIFF_FILE
    		result=`ldapmodify -H $IP_ADDRESS -x -Z -D $ADMIN_DN -w $ADMIN_PWD -f $TMP_LDIFF_FILE 2> $LDAP_ERROR_FILE`
    		exitstatus=$?
    		if [ $exitstatus -eq 0 ]; then
        		echo "iFolderHomeServer attributes added successfully to $USER_DN"
			rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
    			echo ""
			exit $SUCCESS
    		fi
		echo "iFolderUserProvision object class is already part of $USER_DN,  \nFailed to add iFolderHomeServer attribute value, ldap command returned the following error message."
   		cat $LDAP_ERROR_FILE
		rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
    		echo ""
		exit $LDAP_ERROR
	else
        	echo "iFolderUserProvision object class is already part of $USER_DN"
		rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
    		echo ""
		exit $LDAP_ERROR
    	fi    
   fi
   echo "Failed to add iFolderUserProvision object class to  $USER_DN, ldap command returned the following error message."
   cat $LDAP_ERROR_FILE
   rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
   echo ""
   exit $LDAP_ERROR
fi
echo "Failed to add user $USER_DN, ldap command returned the following error message."
cat $LDAP_ERROR_FILE
rm $TMP_LDIFF_FILE $LDAP_ERROR_FILE
echo ""
exit $LDAP_ERROR
