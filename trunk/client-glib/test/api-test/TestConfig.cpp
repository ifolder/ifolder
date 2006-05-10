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
#include "TestConfig.h"

///
/// Class: TestConfigUser
///
TestConfigUser::TestConfigUser() :
	username(NULL), password(NULL)
{
}

TestConfigUser::~TestConfigUser()
{
	if (username != NULL)
		g_free(username);
	if (password != NULL)
		g_free(password);
}

bool
TestConfigUser::loadFromKeyFile(GKeyFile *keyFile, const gchar *groupName)
{
	if (keyFile == NULL || groupName == NULL)
		return false;

	username = g_key_file_get_value(keyFile, groupName, KEY_USERNAME, NULL);
	if (username == NULL)
		return false;
	password = g_key_file_get_value(keyFile, groupName, KEY_PASSWORD, NULL);
	if (password == NULL)
		return false;

	return true;
}

const gchar *
TestConfigUser::getUserName()
{
	return username;
}

const gchar *
TestConfigUser::getPassword()
{
	return password;
}

///
/// Class: TestConfigDomain
///
TestConfigDomain::TestConfigDomain() :
	hostAddress(NULL), validUsers(NULL), invalidUsers(NULL)
{
}

TestConfigDomain::~TestConfigDomain()
{
	if (hostAddress != NULL)
		g_free(hostAddress);

	// FIXME: Free the TestConfigUser objects
	if (validUsers != NULL)
		g_array_free(validUsers, true);
	if (invalidUsers != NULL)
		g_array_free(invalidUsers, true);
}

bool
TestConfigDomain::loadFromKeyFile(GKeyFile *keyFile, const gchar *groupName)
{
	gchar **validUserList = NULL;
	gchar **invalidUserList = NULL;

	gchar *user = NULL;

	if (keyFile == NULL || groupName == NULL)
		return false;

	hostAddress = g_strdup(groupName);

	validUserList = g_key_file_get_string_list(keyFile, groupName, 
												KEY_VALID_USERS, NULL, NULL);
	invalidUserList = g_key_file_get_string_list(keyFile, groupName, 
												KEY_INVALID_USERS, NULL, NULL);

	if (validUserList)
	{
		validUsers = getUsersFromKeyFile(keyFile, (const gchar **)validUserList);
		g_strfreev(validUserList);
	}
	else
		return false;

	if (invalidUserList)
	{
		invalidUsers = getUsersFromKeyFile(keyFile, (const gchar **)invalidUserList);
		g_strfreev(invalidUserList);
	}

	return true;
}

GArray *
TestConfigDomain::getUsersFromKeyFile(GKeyFile *keyFile, const gchar **userList)
{
	GArray *userArray = NULL;
	GString *userGroupName;
	TestConfigUser *user;

	if (keyFile == NULL || userList == NULL)
		return NULL;

	userArray = g_array_new(true, false, sizeof(TestConfigUser *));

	userGroupName = g_string_new(hostAddress);

	for (int i = 0; userList[i] != NULL; i++)
	{
		g_string_printf(userGroupName, "%s_%s", hostAddress, userList[i]);

		user = new TestConfigUser();
		if (user->loadFromKeyFile(keyFile, userGroupName->str))
		{
			g_array_append_val(userArray, user);
		}
		else
			delete user;
	}

	return userArray;
}

const gchar *
TestConfigDomain::getHostAddress()
{
	return hostAddress;
}

TestConfigUser *
TestConfigDomain::getValidUser(int index)
{
	if (!validUsers)
		return NULL;

	return g_array_index(validUsers, TestConfigUser *, index);
}

TestConfigUser *
TestConfigDomain::getInvalidUser(int index)
{
	if (!invalidUsers)
		return NULL;

	return g_array_index(invalidUsers, TestConfigUser *, index);
}


///
/// Class: TestConfig
///

TestConfig * TestConfig::testConfig = NULL;

TestConfig::TestConfig() : bInitialized(false), keyFile(NULL)
{
}

TestConfig::~TestConfig()
{
}

TestConfig *
TestConfig::getTestConfig(const gchar *configFilePath)
{
	TestConfig *tmpConfig;

	if (testConfig == NULL)
	{
		tmpConfig = new TestConfig();
		if (tmpConfig->loadConfig(configFilePath))
		{
			tmpConfig->bInitialized = true;
			testConfig = tmpConfig;
		}
		else
			delete tmpConfig;
	}

	return testConfig;
}

TestConfigDomain *
TestConfig::getValidDomain(int index)
{
	if (validDomains == NULL)
		return NULL;

	return g_array_index(validDomains, TestConfigDomain *, index);
}

TestConfigDomain *
TestConfig::getInvalidDomain(int index)
{
	if (invalidDomains == NULL)
		return NULL;

	return g_array_index(invalidDomains, TestConfigDomain *, index);
}

bool
TestConfig::loadConfig(const gchar *configFilePath)
{
	if (!g_file_test(configFilePath, G_FILE_TEST_EXISTS))
	{
		g_critical("Specified config file does not exist: %s\n", configFilePath);
		return false;
	}

	keyFile = g_key_file_new();
	if (g_key_file_load_from_file(keyFile, configFilePath, G_KEY_FILE_NONE, NULL))
	{
		if (loadDomains(keyFile))
			return true;
	}

	return false;
}

bool
TestConfig::loadDomains(GKeyFile *keyFile)
{
	gchar **hostAddressList = NULL;
	TestConfigDomain *domain;
	bool bValidDomainAdded = false;
	bool bInvalidDomainAdded = false;

	if (!g_key_file_has_group(keyFile, GROUP_TEST_CONFIG))
		return false;

	hostAddressList = g_key_file_get_string_list(keyFile, GROUP_TEST_CONFIG, KEY_VALID_DOMAINS, NULL, NULL);

	if (hostAddressList == NULL)
		return false;

	validDomains = g_array_new(true, true, sizeof(TestConfigDomain *));

	for (int i = 0; hostAddressList[i] != NULL; i++)
	{
		domain = new TestConfigDomain();
		if (domain->loadFromKeyFile(keyFile, hostAddressList[i]))
		{
			g_array_append_val(validDomains, domain);
			bValidDomainAdded = true;
		}
		else
			delete domain;
	}

	g_strfreev(hostAddressList);

	hostAddressList = g_key_file_get_string_list(keyFile, GROUP_TEST_CONFIG, KEY_INVALID_DOMAINS, NULL, NULL);
	if (hostAddressList != NULL)
	{
		invalidDomains = g_array_new(true, true, sizeof(TestConfigDomain *));
		for (int i = 0; hostAddressList[i] != NULL; i++)
		{
			domain = new TestConfigDomain();
			if (domain->loadFromKeyFile(keyFile, hostAddressList[i]))
			{
				g_array_append_val(invalidDomains, domain);
				bInvalidDomainAdded = true;
			}
			else
				delete domain;
		}

		g_strfreev(hostAddressList);
	}

	if (!bValidDomainAdded)
	{
		g_critical("No valid domains found in config file\n");
		return false;
	}

	if (!bInvalidDomainAdded)
	{
		g_critical("No invalid domains found in config file\n");
		return false;
	}

	return true;
}
