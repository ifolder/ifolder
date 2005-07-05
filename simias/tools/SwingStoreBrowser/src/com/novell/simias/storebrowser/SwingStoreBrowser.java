/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

package com.novell.simias.storebrowser;

import java.net.MalformedURLException;
import java.text.MessageFormat;
import java.net.URL;
import java.io.*;

import com.novell.simias.browser.Browser_x0020_ServiceLocator;
import com.novell.simias.browser.Browser_x0020_ServiceSoap;

/**
 * @author Boyd Timothy <boyd@timothy.ws>
 */
public class SwingStoreBrowser {

	public SwingStoreBrowser()
	{
		
	}

	public static void main(String[] args) {
		StoreBrowserWindow sbWin = new StoreBrowserWindow();
		sbWin.setSize(800,600);
		//sbWin.pack();
		sbWin.setVisible(true);
	}

	public static Browser_x0020_ServiceSoap getSoapService(String serviceURL, String username, String password) throws MalformedURLException
	{
		String protocol;

	    Browser_x0020_ServiceLocator locator =
	    	new Browser_x0020_ServiceLocator();

	    URL url = new URL(serviceURL);

	    Browser_x0020_ServiceSoap service = null;
    	try
		{
    		// make sure that the axis client knows to use a session
    		locator.setMaintainSession(true);

    		service = locator.getBrowser_x0020_ServiceSoap(url);

			if (username != null)
				((org.apache.axis.client.Stub) service).setUsername(username);
			if (password != null)
				((org.apache.axis.client.Stub) service).setPassword(password);
			
            // if we make it here without exceptions, we've found a valid server
            return service;
		}
    	catch (Exception e)
		{
            e.printStackTrace();
		}
    	
    	return null;
	}
	
	public static Browser_x0020_ServiceSoap getLocalSoapService()
	{
		String username = null;
		String password = null;
		
	    URL url = getLocalServiceURL();
		if (url == null)
			return null;

		SimiasCredentials credentials = SimiasCredentials.getLocalSimiasCredentials();
		if (credentials != null)
		{
			username = credentials.getUsername();
			password = credentials.getPassword();
		}
		
		try
		{
			return getSoapService(url.toString(), username, password);
		}
		catch (MalformedURLException e)
		{
			// Intentionally left blank
		}
		
		return null;
	}
	
	private static URL getLocalServiceURL()
	{
		String userHomePath = System.getProperty("user.home");
		String fileSeparator = System.getProperty("file.separator");
		
		if (userHomePath == null || fileSeparator == null)
			return null;
		
		String configFilePath =
			MessageFormat.format("{0}{1}.local{1}share{1}simias{1}Simias.config",
								 new Object[] {userHomePath, fileSeparator});

		try
		{
			BufferedReader br = new BufferedReader(new FileReader(configFilePath));
			
			String line = br.readLine();
			while (line != null)
			{
				int settingPos = line.indexOf("<setting name=\"WebServiceUri\"");
				if (settingPos >= 0)
				{
					// Attempt to parse out the URL
					int valuePos = line.indexOf("value=\"", settingPos + 1);
					if (valuePos >= 0)
					{
						int closingQuotePos = line.indexOf("\"", valuePos + 7);
						if (closingQuotePos >= 0)
						{
							String urlString = line.substring(valuePos + 7, closingQuotePos);
							try
							{
								return new URL(urlString + fileSeparator + "SimiasBrowser.asmx");
							}
							catch (MalformedURLException e)
							{
								System.err.println("Couldn't parse WebServiceUri from Simias.config");
								return null;
							}
						}
					}
				}
				
				line = br.readLine();
			}
			
			br.close();
		}
		catch (IOException ioe)
		{
			System.err.println("Couldn't open: " + configFilePath);
			ioe.printStackTrace();
			return null;
		}
		catch(Exception e)
		{
			System.err.println("Parse error: " + configFilePath);
			e.printStackTrace();
		}

		return null;
	}

	private static class SimiasCredentials
	{
		String username;
		String password;
		
		public SimiasCredentials(String username, String password)
		{
			this.username = username;
			this.password = password;
		}
		
		public String getUsername()
		{
			return username;
		}
		
		public String getPassword()
		{
			return password;
		}
		
		public static SimiasCredentials getLocalSimiasCredentials()
		{
			// Read the Simias Credentials from the disk and return
			// a SimiasCredentials Object
			String username = System.getProperty("user.name");
			String userHomePath = System.getProperty("user.home");
			String fileSeparator = System.getProperty("file.separator");
			String password = null;
			
			if (username == null || userHomePath == null || fileSeparator == null)
				return null;

			String passwordFilePath =
				MessageFormat.format("{0}{1}.local{1}share{1}simias{1}.local.if",
									 new Object[] {userHomePath, fileSeparator});

			try
			{
				BufferedReader br = new BufferedReader(new FileReader(passwordFilePath));
				
				password = br.readLine();
				
				br.close();
			}
			catch (Exception e)
			{
				return null;
			}

			return new SimiasCredentials(username, password);
		}
	}
}
