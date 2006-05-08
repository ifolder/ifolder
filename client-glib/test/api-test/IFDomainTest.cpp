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

#include <string.h>

#include <ifolder-client.h>

#include "IFDomainTest.h"

GString * IFDomainTest::customDataPath = NULL;
TestConfig * IFDomainTest::testConfig = NULL;

TestConfigDomain * IFDomainTest::validDomain = NULL;
TestConfigDomain * IFDomainTest::invalidDomain = NULL;
TestConfigUser * IFDomainTest::validUser = NULL;
TestConfigUser * IFDomainTest::invalidUser = NULL;

TestConfigDomain * IFDomainTest::validDomain1 = NULL;
TestConfigUser * IFDomainTest::validUser1 = NULL;

IFDomainTest::IFDomainTest()
{
	const gchar * tmpDir;
	printf("IFDomainTest::IFDomainTest()\n");

	// Initialize things ONCE so that every test doesn't have to
	// redo this over and over.
	tmpDir = g_get_tmp_dir();

	///
	/// customDataPath
	///
	if (customDataPath == NULL)
	{
		customDataPath = g_string_new(tmpDir);

		#ifdef WIN32
			g_string_append_printf(customDataPath, "\\ifolder-client-api-test-domain");
		#else
			g_string_append_printf(customDataPath, "/ifolder-client-api-test-domain");
		#endif
	}

	if (testConfig == NULL)
		testConfig = TestConfig::getTestConfig();

	CPPUNIT_ASSERT(testConfig != NULL);

	if (validDomain == NULL)
		validDomain = testConfig->getValidDomain(0);

	if (invalidDomain == NULL)
		invalidDomain = testConfig->getInvalidDomain(0);

	CPPUNIT_ASSERT(validDomain != NULL);
	CPPUNIT_ASSERT(invalidDomain != NULL);

	if (validUser == NULL)
		validUser = validDomain->getValidUser(0);
	if (invalidUser == NULL)
		invalidUser = validDomain->getInvalidUser(0);

	CPPUNIT_ASSERT(validUser != NULL);
	CPPUNIT_ASSERT(invalidUser != NULL);

	if (validDomain1 == NULL)
		validDomain1 = testConfig->getValidDomain(1);
	CPPUNIT_ASSERT(validDomain1 != NULL);

	if (validUser1 == NULL)
		validUser1 = validDomain1->getValidUser(0);
	CPPUNIT_ASSERT(validUser1 != NULL);
}

IFDomainTest::~IFDomainTest()
{
}

void
IFDomainTest::setUp()
{
	CPPUNIT_ASSERT(ifolder_client_initialize(customDataPath->str) == IFOLDER_SUCCESS);
}

void
IFDomainTest::tearDown()
{
	CPPUNIT_ASSERT(ifolder_client_uninitialize() == IFOLDER_SUCCESS);
}

void
IFDomainTest::testAddRemove()
{
	int err;
	iFolderDomain domain;

	///
	/// Test #1: Attempt to add a valid domain with valid credentials
	CPPUNIT_ASSERT(
		err = ifolder_domain_add(validDomain->getHostAddress(),
								 validUser->getUserName(),
								 validUser->getPassword(),
								 true,
								 &domain) == IFOLDER_SUCCESS
		);

	///
	/// Test #2: Attempt to re-add the domain with same credentials
	/// Expected error: IFOLDER_ERR_DOMAIN_ALREADY_EXISTS
	CPPUNIT_ASSERT(
		err = ifolder_domain_add(validDomain->getHostAddress(),
								 validUser->getUserName(),
								 validUser->getPassword(),
								 true,
								 &domain) == IFOLDER_ERR_DOMAIN_ALREADY_EXISTS
		);

	///
	/// Test #3: Remove the domain
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );

	///
	/// Test #4: Make sure we don't succeed removing the domain multiple times
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_ERR_DOMAIN_NOT_FOUND );

	ifolder_domain_free(domain);

	///
	/// Test #5: Attempt to add a domain with an invalid host address
	/// Expected error: IFOLDER_ERR_DOMAIN_NOT_FOUND
	CPPUNIT_ASSERT(
		err = ifolder_domain_add(invalidDomain->getHostAddress(),
								 validUser->getUserName(),
								 validUser->getPassword(),
								 true,
								 &domain) == IFOLDER_ERR_DOMAIN_NOT_FOUND
		);

	///
	/// Test #6: Test NULL parameters
	CPPUNIT_ASSERT(	err = ifolder_domain_add(NULL, NULL, NULL, true, &domain) == IFOLDER_ERR_INVALID_PARAMETER );
}

void
IFDomainTest::testRemoveAndDeleteiFolders()
{
	int err;
	iFolderDomain domain;
	iFolder ifolder0;
	iFolder ifolder1;
	iFolderEnumeration ifolder_enum;

	// Setup:
	//		1. Add a new account
	//		2. Add two iFolders
	// Test:
	//		3. Remove the account w/ delete iFolders set to true
	//		4. Add the account again
	//		5. Check that the account has NO iFolders
	// Cleanup:
	//		6. Remove the account

	// 1. Add a new account
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// 2. Add two new iFolders
	CPPUNIT_ASSERT( err = ifolder_domain_create_ifolder(domain, "IFDomainTest.testRemoveAndDeleteiFolders0", "This was created by a unit test.", &ifolder0) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( err = ifolder_domain_create_ifolder(domain, "IFDomainTest.testRemoveAndDeleteiFolders1", "This was created by a unit test.", &ifolder1) == IFOLDER_SUCCESS );

	// 3. Remove the account w/ delete iFolders set to true
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, true) == IFOLDER_SUCCESS );

	ifolder_domain_free(domain);
	domain = NULL;

	// 4. Add the account again
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// 5. Check that the account has NO iFolders
	CPPUNIT_ASSERT( err = ifolder_domain_get_all_ifolders(domain, 0, 10, &ifolder_enum) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_enumeration_has_more(ifolder_enum) == false );
	ifolder_enumeration_free(ifolder_enum);

	// 6. Remove the account
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testUserData()
{
	int err;
	iFolderDomain domain;
	int val;
	int *testVal;
	int *testVal2;

	///
	/// Setup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );
	
	///
	/// Test
	///
	// Test #1: user data should be NULL by default
	CPPUNIT_ASSERT ( ifolder_domain_get_user_data(domain) == NULL );

	// Test #2: make sure we can set a simple value and retrieve it	
	val = 1;
	ifolder_domain_set_user_data(domain, &val);
	testVal = (int *)ifolder_domain_get_user_data(domain);
	CPPUNIT_ASSERT ( testVal != NULL );
	CPPUNIT_ASSERT ( &val == testVal ); // make sure testVal still points to the address of val
	CPPUNIT_ASSERT ( *testVal == 1 ); // make sure the value hasn't been altered
	
	// Test #3: see if we can set a new value and retrieve it
	testVal = (int *)malloc(sizeof(int));
	*testVal = 2;
	
	ifolder_domain_set_user_data(domain, testVal);
	testVal2 = (int *)ifolder_domain_get_user_data(domain);
	CPPUNIT_ASSERT ( testVal2 != NULL );
	CPPUNIT_ASSERT ( testVal2 == testVal ); // testVal2 should still be pointing to testVal
	CPPUNIT_ASSERT ( *testVal2 == 2 );	// make sure the value hasn't been altered
	
	free(testVal);
	
	// Test #4: set the value to NULL and make sure it "sticks"
	ifolder_domain_set_user_data(domain, NULL);
	testVal = (int *)ifolder_domain_get_user_data(domain);
	CPPUNIT_ASSERT ( testVal == NULL );

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testLogInAndOut()
{
	int err;
	iFolderDomain domain;

	///
	/// Setup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// Test: Attempt to log in with a NULL domain
	CPPUNIT_ASSERT( err = ifolder_domain_log_in(NULL, validUser->getPassword()) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Log in with valid credentials
	CPPUNIT_ASSERT( err = ifolder_domain_log_in(domain, validUser->getPassword()) == IFOLDER_SUCCESS );

	// Test: Attempt to log in again even though we're already logged-in
	// Expected return: IFOLDER_SUCCESS (calling login again shouldn't really do anything)
	CPPUNIT_ASSERT( err = ifolder_domain_log_in(domain, validUser->getPassword()) == IFOLDER_SUCCESS );
	
	// Test: Attempt to log out with a NULL domain
	CPPUNIT_ASSERT( err = ifolder_domain_log_out(NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Log out
	CPPUNIT_ASSERT( err = ifolder_domain_log_out(domain) == IFOLDER_SUCCESS );

	// Test: Attempt to log in with a NULL password
	CPPUNIT_ASSERT( err = ifolder_domain_log_in(domain, NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Attempt to log in with a wrong password
	GString *badPassword = g_string_new(validUser->getPassword());
	g_string_printf(badPassword, "%s%s", validUser->getPassword());
	CPPUNIT_ASSERT( err = ifolder_domain_log_in(domain, badPassword->str) != IFOLDER_SUCCESS );
	g_string_free(badPassword, true);

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testActivateAndInactivate()
{
	int err;
	iFolderDomain domain;

	///
	/// Setup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// Test: A domain should be active immediately after it's added
	CPPUNIT_ASSERT( ifolder_domain_get_is_active(domain) == true );

	// Test: Set the domain as active even though it already is and make sure it remains active
	CPPUNIT_ASSERT( err = ifolder_domain_activate(domain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_active(domain) == true );

	// Test: Inactivate the domain
	CPPUNIT_ASSERT( err = ifolder_domain_inactivate(domain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_active(domain) == false );

	// Test: Inactivate the domain even though it is already inactive and make sure its state does not change
	CPPUNIT_ASSERT( err = ifolder_domain_inactivate(domain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_active(domain) == false );

	// Test: Activate the domain
	CPPUNIT_ASSERT( err = ifolder_domain_activate(domain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_active(domain) == true );

	// Test: Call activate with NULL
	CPPUNIT_ASSERT( err = ifolder_domain_activate(NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Call inactivate with NULL
	CPPUNIT_ASSERT( err = ifolder_domain_inactivate(NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testChangeHostAddress()
{
	int err;
	iFolderDomain domain;

	///
	/// Setup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// Test: Attempt to change the domain host address with a null domain
	CPPUNIT_ASSERT( err = ifolder_domain_change_host_address(NULL, invalidDomain->getHostAddress()) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Attempt to change the domain host address with a null address
	CPPUNIT_ASSERT( err = ifolder_domain_change_host_address(domain, NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Attempt to change the domain host address to be the exact same address
	// Expected result: ???
	CPPUNIT_ASSERT( err = ifolder_domain_change_host_address(domain, validDomain->getHostAddress()) != IFOLDER_SUCCESS );

	// Test: Make sure we get an error when attempting to change the host address
	// since we know we're working with a valid server.
	ifolder_domain_change_host_address(domain, NULL);
	CPPUNIT_ASSERT( err = ifolder_domain_change_host_address(domain, invalidDomain->getHostAddress()) != IFOLDER_SUCCESS );

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testDefault()
{
	int err;
	iFolderDomain domain0;
	iFolderDomain domain1;
	iFolderDomain tmpDomain;

	// Test: Add the first domain, specifying that the domain should NOT be set up
	// as the default.  Since it's the very first domain to be added, however, the
	// API should automatically set it up as the default.
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), false, &domain0) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_default(domain0) == true );
	CPPUNIT_ASSERT( err = ifolder_domain_get_default(&tmpDomain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( strcmp(ifolder_domain_get_id(domain0), ifolder_domain_get_id(tmpDomain)) == 0 );
	ifolder_domain_free(tmpDomain);

	// Test: Add on the second domain, specigying that it SHOULD be the default.
	// This should make the second domain the default and unmark the original
	// domain as the default.
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain1->getHostAddress(), validUser1->getUserName(), validUser1->getPassword(), true, &domain1) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( ifolder_domain_get_is_default(domain1) == true );
	CPPUNIT_ASSERT( err = ifolder_domain_get_default(&tmpDomain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( strcmp(ifolder_domain_get_id(domain1), ifolder_domain_get_id(tmpDomain)) == 0 );
	ifolder_domain_free(tmpDomain);

	// Test: Mark the first domain as the default now
	CPPUNIT_ASSERT( err = ifolder_domain_set_default(domain0) );
	CPPUNIT_ASSERT( ifolder_domain_get_is_default(domain0) == true );
	CPPUNIT_ASSERT( err = ifolder_domain_get_default(&tmpDomain) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( strcmp(ifolder_domain_get_id(domain0), ifolder_domain_get_id(tmpDomain)) == 0 );
	ifolder_domain_free(tmpDomain);

	// Test: Attempt to set a default using a NULL domain
	CPPUNIT_ASSERT( err = ifolder_domain_set_default(NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain0, false) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain1, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain0);
	ifolder_domain_free(domain1);
}

void
IFDomainTest::testGetAuthenticatedUser()
{
	int err;
	iFolderDomain domain;
	iFolderUser user;

	///
	/// Setup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_add(validDomain->getHostAddress(), validUser->getUserName(), validUser->getPassword(), true, &domain) == IFOLDER_SUCCESS );

	// Test: Attempt to call with NULL parameters
	CPPUNIT_ASSERT( err = ifolder_domain_get_authenticated_user(NULL, &user) == IFOLDER_ERR_INVALID_PARAMETER );
	CPPUNIT_ASSERT( err = ifolder_domain_get_authenticated_user(domain, NULL) == IFOLDER_ERR_INVALID_PARAMETER );

	// Test: Verify that the user returned from the get_authenticated_user()
	// call shares the same username with the one we used to add the domain.
	CPPUNIT_ASSERT( err = ifolder_domain_get_authenticated_user(domain, &user) == IFOLDER_SUCCESS );
	CPPUNIT_ASSERT( strcmp(validUser->getUserName(), ifolder_user_get_user_name(user)) == 0 );
	ifolder_user_free(user);

	///
	/// Cleanup
	///
	CPPUNIT_ASSERT( err = ifolder_domain_remove(domain, false) == IFOLDER_SUCCESS );
	ifolder_domain_free(domain);
}

void
IFDomainTest::testGetAll()
{
}

void
IFDomainTest::testGetAllActive()
{
}

void
IFDomainTest::testGetUser()
{
}

void
IFDomainTest::testGetUsers()
{
}

void
IFDomainTest::testGetUsersBySearch()
{
}

void
IFDomainTest::testCreateAndDeleteiFolderFromPath()
{
}

void
IFDomainTest::testCreateAndDeleteiFolder()
{
}

void
IFDomainTest::testConnectAndDisconnectiFolder()
{
}

void
IFDomainTest::testGetAlliFolders()
{
}

void
IFDomainTest::testGetConnectediFolders()
{
}

void
IFDomainTest::testGetDisconnectediFolders()
{
}

void
IFDomainTest::testGetiFolderById()
{
}

void
IFDomainTest::testGetiFoldersByName()
{
}

/**
 * The purpose of this test is to test all valid domains and make sure that we
 * are able to connect to and log in to them.  All extended ascii character user
 * names, etc. should be added into the TestConfig.ini file for this test.
 */
void
IFDomainTest::testValidDomains()
{
	int err;
	TestConfigDomain *domain;
	TestConfigUser *user;
	iFolderDomain ifolderDomain;

	// Test all valid domains
	for (int i = 0; ; i++)
	{
		domain = testConfig->getValidDomain(i);
		if (domain == NULL)
			break;

		// Test valid users
		for (int j = 0; ; j++)
		{
			user = domain->getValidUser(i);
			if (user == NULL)
				break;

			// Add
			CPPUNIT_ASSERT(
				err = ifolder_domain_add(domain->getHostAddress(),
				user->getUserName(),
				user->getPassword(),
				true,
				&ifolderDomain) == IFOLDER_SUCCESS
				);

			// Log in
			CPPUNIT_ASSERT(
				err = ifolder_domain_log_in(ifolderDomain, user->getPassword()) == IFOLDER_SUCCESS
				);

			// Log out
			CPPUNIT_ASSERT(
				err = ifolder_domain_log_out(ifolderDomain) == IFOLDER_SUCCESS
				);

			// Remove
			CPPUNIT_ASSERT(
				err = ifolder_domain_remove(ifolderDomain, false) == IFOLDER_SUCCESS
				);

			ifolder_domain_free(ifolderDomain);
		}

		// Test invalid users
		for (int j = 0; ; j++)
		{
			user = domain->getInvalidUser(i);
			if (user == NULL)
				break;

			// Attempt to add (should NOT get IFOLDER_SUCCESS)
			CPPUNIT_ASSERT(
				err = ifolder_domain_add(domain->getHostAddress(),
										 user->getUserName(),
										 user->getPassword(),
										 true,
										 &ifolderDomain) != IFOLDER_SUCCESS
				);
		}
	}
}

/**
 * This will test all the invalid domains specified in the TestConfig.ini file
 * and verify that we are never successful connecting to any of them.
 */
void
IFDomainTest::testInvalidDomains()
{
	int err;
	TestConfigDomain *domain;
	TestConfigUser *user;
	iFolderDomain ifolderDomain;

	// Test all invalid domains
	for (int i = 0; ; i++)
	{
		domain = testConfig->getInvalidDomain(i);
		if (domain == NULL)
			break;

		// Test valid users
		for (int j = 0; ; j++)
		{
			user = domain->getValidUser(i);
			if (user == NULL)
				break;

			// Attempt to add (should NOT get IFOLDER_SUCCESS)
			CPPUNIT_ASSERT(
				err = ifolder_domain_add(domain->getHostAddress(),
										 user->getUserName(),
										 user->getPassword(),
										 true,
										 &ifolderDomain) != IFOLDER_SUCCESS
				);
		}

		// Test invalid users
		for (int j = 0; ; j++)
		{
			user = domain->getInvalidUser(i);
			if (user == NULL)
				break;

			// Attempt to add (should NOT get IFOLDER_SUCCESS)
			CPPUNIT_ASSERT(
				err = ifolder_domain_add(domain->getHostAddress(),
										 user->getUserName(),
										 user->getPassword(),
										 true,
										 &ifolderDomain) != IFOLDER_SUCCESS
				);
		}
	}
}
