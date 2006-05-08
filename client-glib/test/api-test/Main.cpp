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

#include <stdio.h>
#include <cppunit/ui/text/TestRunner.h>
#include "IFClientTest.h"
#include "IFDomainTest.h"

void printSyntax();

int
main(int argc, char *argv[])
{
	char *testConfigFileName = NULL;
	TestConfig *testConfig;

	if (argc > 1)
		testConfigFileName = argv[1];
	else
		testConfigFileName = DEFAULT_CONFIG_FILE;

	testConfig = TestConfig::getTestConfig(testConfigFileName);
	if (testConfig == NULL)
	{
		printf("Could not load config file \"%s\"\n\n", testConfigFileName);
		printSyntax();
		return -1;
	}

	CppUnit::TextUi::TestRunner runner;
	runner.addTest( IFClientTest::suite() );
	runner.addTest( IFDomainTest::suite() );
	runner.run();
	return 0;
}

void printSyntax()
{
	printf("Syntax: api-test [Test Config INI file]\n");
}
