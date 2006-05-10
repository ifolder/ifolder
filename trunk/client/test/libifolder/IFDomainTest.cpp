/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#include <ifolder-client.h>

#include "IFDomainTest.h"

IFDomainTest::IFDomainTest() :
	testRemoveAndKeepiFoldersDomain(NULL),
	testiFolder1(NULL),
	testiFolder2(NULL),
	testRemoveAndDeleteiFoldersDomain(NULL),
	testiFolder3(NULL),
	testiFolder4(NULL),
	testUserDataDomain(NULL)
{
}

IFDomainTest::~IFDomainTest()
{
}

void
IFDomainTest::setUp()
{
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_client_initialize() );

	// testRemove
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_add("192.168.1.1", "testRemoveAndKeepiFolder", "novell", true, &testRemoveAndKeepiFoldersDomain) ); // FIXME: modify the test harness so that we pass in credentials it should use to peform all the tests and then use them here
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_log_in(testRemoveAndKeepiFoldersDomain, "novell") );
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_create_ifolder(testRemoveAndKeepiFoldersDomain, "/tmp/testRemoveAndKeepiFolder1", "This is an iFolder used for unit testing", &testiFolder1) );
	ifolder_free(testiFolder1);
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_create_ifolder(testRemoveAndKeepiFoldersDomain, "/tmp/testRemoveAndKeepiFolder2", "This is an iFolder used for unit testing", &testiFolder2) );
	ifolder_free(testiFolder2);
	
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_add("192.168.1.1", "testRemoveAndDeleteiFolder", "novell", true, &testRemoveAndDeleteiFoldersDomain) );
	
	// testUserData
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_add("192.168.1.1", "tester", "novell", true, &testUserDataDomain) );
}

void
IFDomainTest::tearDown()
{
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_remove(testRemoveAndKeepiFoldersDomain, true) );
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_remove(testRemoveAndDeleteiFoldersDomain, true) );
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_domain_remove(testUserDataDomain, true) );
	CPPUNIT_ASSERT ( IFOLDER_SUCCESS == ifolder_client_uninitialize() );
}

void
IFDomainTest::testAdd()
{
	int err;
	iFolderDomain domain;
	
	err = ifolder_domain_add("193.97.125.1", "btimothy", "novell", true, &domain);
	if (err == IFOLDER_SUCCESS)
	{
		printf("New domain added\n");
		printf("\t%s\n", ifolder_domain_get_id(domain));
		printf("\t%s\n", ifolder_domain_get_name(domain));
		printf("\t%s\n", ifolder_domain_get_description(domain));
		printf("\t%s\n", ifolder_domain_get_version(domain));
		printf("\t%s\n", ifolder_domain_get_host_address(domain));
		printf("\t%s\n", ifolder_domain_get_machine_name(domain));
		printf("\t%s\n", ifolder_domain_get_os_version(domain));
		printf("\t%s\n", ifolder_domain_get_user_name(domain));
		printf("\t%s\n", ifolder_domain_get_is_default(domain) ? "true" : "false");
		printf("\t%s\n", ifolder_domain_get_is_active(domain) ? "true" : "false");
		
		ifolder_domain_free(domain);
	}
	else
	{
		printf("ifolder_domain_add() returned: %d\n", err);
	}
	
	CPPUNIT_ASSERT( err == IFOLDER_SUCCESS );
}

void
IFDomainTest::testRemove()
{
	int err;
	
	err = ifolder_domain_remove(testRemoveAndKeepiFoldersDomain, true);
}

void
IFDomainTest::testUserData()
{
	int val;
	int *testVal;
	int *testVal2;

	CPPUNIT_ASSERT( testUserDataDomain != NULL );
	
	// Test #1: user data should be NULL by default
	CPPUNIT_ASSERT ( ifolder_domain_get_user_data(testUserDataDomain) == NULL );

	// Test #2: make sure we can set a simple value and retrieve it	
	val = 1;
	ifolder_domain_set_user_data(testUserDataDomain, &val);
	testVal = (int *)ifolder_domain_get_user_data(testUserDataDomain);
	CPPUNIT_ASSERT ( testVal != NULL );
	CPPUNIT_ASSERT ( &val == testVal ); // make sure testVal still points to the address of val
	CPPUNIT_ASSERT ( *testVal == 1 ); // make sure the value hasn't been altered
	
	// Test #3: see if we can set a new value and retrieve it
	testVal = (int *)malloc(sizeof(int));
	*testVal = 2;
	
	ifolder_domain_set_user_data(testUserDataDomain, testVal);
	testVal2 = (int *)ifolder_domain_get_user_data(testUserDataDomain);
	CPPUNIT_ASSERT ( testVal2 != NULL );
	CPPUNIT_ASSERT ( testVal2 == testVal ); // testVal2 should still be pointing to testVal
	CPPUNIT_ASSERT ( *testVal2 == 2 );	// make sure the value hasn't been altered
	
	free(testVal);
	
	// Test #4: set the value to NULL and make sure it "sticks"
	ifolder_domain_set_user_data(testUserDataDomain, NULL);
	testVal = (int *)ifolder_domain_get_user_data(testUserDataDomain);
	CPPUNIT_ASSERT ( testVal == NULL );
}
