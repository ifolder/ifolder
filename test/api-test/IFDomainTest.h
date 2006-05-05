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

#include "ifolder-domain.h"

class IFDomainTest : public CPPUNIT_NS::TestFixture {

CPPUNIT_TEST_SUITE( IFDomainTest );
CPPUNIT_TEST( testAdd );
CPPUNIT_TEST( testRemove );
CPPUNIT_TEST( testUserData );
CPPUNIT_TEST_SUITE_END();

public:
	// Constructor
	IFDomainTest();

	// Destructor
	virtual ~IFDomainTest();

	void setUp();
	void tearDown();

	void testAdd();
	void testRemove();
	void testUserData();

private:

	// testRemove
	iFolderDomain testRemoveAndKeepiFoldersDomain;
	iFolder testiFolder1;
	iFolder testiFolder2;
	
	iFolderDomain testRemoveAndDeleteiFoldersDomain;
	iFolder testiFolder3;
	iFolder testiFolder4;

	// testUserData
	iFolderDomain testUserDataDomain;
};

#endif // _IFOLDER_DOMAIN_TEST_H_

