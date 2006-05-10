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
#ifndef _IFOLDER_DOMAIN_TEST_H_
#define _IFOLDER_DOMAIN_TEST_H_

#include <cppunit/extensions/HelperMacros.h>
#include <cppunit/TestPath.h>
#include <cppunit/TestCase.h>

#include "TestConfig.h"
#include "ifolder-domain.h"

class IFDomainTest : public CPPUNIT_NS::TestFixture {

CPPUNIT_TEST_SUITE( IFDomainTest );
CPPUNIT_TEST( testAddRemove );
CPPUNIT_TEST( testRemoveAndDeleteiFolders );
CPPUNIT_TEST( testUserData );

CPPUNIT_TEST( testLogInAndOut );
CPPUNIT_TEST( testActivateAndInactivate );

CPPUNIT_TEST( testChangeHostAddress );

CPPUNIT_TEST( testDefault );
CPPUNIT_TEST( testGetAuthenticatedUser );

// Domains API
CPPUNIT_TEST( testGetAll );
CPPUNIT_TEST( testGetAllActive );

// User Management
CPPUNIT_TEST( testGetUser );
CPPUNIT_TEST( testGetUsers );
CPPUNIT_TEST( testGetUsersBySearch );

// iFolder Management
CPPUNIT_TEST( testCreateAndDeleteiFolderFromPath );
CPPUNIT_TEST( testCreateAndDeleteiFolder );
CPPUNIT_TEST( testConnectAndDisconnectiFolder );
CPPUNIT_TEST( testGetAlliFolders );
CPPUNIT_TEST( testGetConnectediFolders );
CPPUNIT_TEST( testGetDisconnectediFolders );
CPPUNIT_TEST( testGetiFolderById );
CPPUNIT_TEST( testGetiFoldersByName );

// Other
CPPUNIT_TEST( testValidDomains );
CPPUNIT_TEST( testInvalidDomains );
CPPUNIT_TEST_SUITE_END();

public:
	// Constructor
	IFDomainTest();

	// Destructor
	virtual ~IFDomainTest();

	void setUp();
	void tearDown();

	void testAddRemove();
	void testRemoveAndDeleteiFolders();
	void testUserData();

	void testLogInAndOut();
	void testActivateAndInactivate();

	void testChangeHostAddress();

//	void testSetCredentials(); // FIXME: Figure out how to test this

	void testDefault();
	void testGetAuthenticatedUser();

	// Domains API
	void testGetAll();
	void testGetAllActive();

	// User Management
	void testGetUser();
	void testGetUsers();
	void testGetUsersBySearch();

	// iFolder Management
	void testCreateAndDeleteiFolderFromPath();
	void testCreateAndDeleteiFolder();
	void testConnectAndDisconnectiFolder();
	void testGetAlliFolders();
	void testGetConnectediFolders();
	void testGetDisconnectediFolders();
	void testGetiFolderById();
	void testGetiFoldersByName();

	// Other
	void testValidDomains();
	void testInvalidDomains();

private:

	static GString *customDataPath;
	static TestConfig *testConfig;

	static TestConfigDomain *validDomain;
	static TestConfigDomain *invalidDomain;
	static TestConfigUser *validUser;
	static TestConfigUser *invalidUser;

	static TestConfigDomain *validDomain1;
	static TestConfigUser *validUser1;
};

#endif // _IFOLDER_DOMAIN_TEST_H_

