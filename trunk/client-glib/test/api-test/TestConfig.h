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
#ifndef _TEST_CONFIG_H_
#define _TEST_CONFIG_H_

#ifndef WIN32
#include <stdbool.h>
#endif

#include <glib.h>

#define DEFAULT_CONFIG_FILE "TestConfig.ini"

#define GROUP_TEST_CONFIG	"TestConfig"
#define KEY_VALID_DOMAINS	"ValidDomains"
#define KEY_INVALID_DOMAINS	"InvalidDomains"
#define KEY_VALID_USERS		"ValidUsers"
#define KEY_INVALID_USERS	"InvalidUsers"
#define KEY_USERNAME		"UserName"
#define KEY_PASSWORD		"Password"

class TestConfigUser
{
public:
	TestConfigUser();
	virtual ~TestConfigUser();

	bool loadFromKeyFile(GKeyFile *keyFile, const gchar *groupName);

	const gchar *getUserName();
	const gchar *getPassword();

private:
	gchar *username;
	gchar *password;
};

class TestConfigDomain
{
public:
	TestConfigDomain();
	virtual ~TestConfigDomain();

	bool loadFromKeyFile(GKeyFile *keyFile, const gchar *groupName);

	GArray *getValidUsers();
	GArray *getInvalidUsers();

	const gchar *getHostAddress();
	TestConfigUser *getValidUser(int index);
	TestConfigUser *getInvalidUser(int index);

private:
	GArray * getUsersFromKeyFile(GKeyFile *keyFile, const gchar **userList);

	gchar *hostAddress;
	GArray *validUsers;
	GArray *invalidUsers;
};

class TestConfig
{
public:
	virtual ~TestConfig();

	// Singleton access
	static TestConfig *getTestConfig(const gchar *configFilePath = NULL);

	TestConfigDomain *getValidDomain(int domainIndex);
	TestConfigDomain *getInvalidDomain(int domainIndex);
private:
	TestConfig();

	bool loadConfig(const gchar *configFilePath);
	bool loadDomains(GKeyFile *keyFile);

	bool bInitialized;
	GKeyFile *keyFile;
	GArray *validDomains;
	GArray *invalidDomains;

	static TestConfig *testConfig;
};

#endif /*_TEST_CONFIG_H_*/