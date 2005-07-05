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
import java.net.URL;

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

	public static Browser_x0020_ServiceSoap getSoapService(String serviceURL) throws MalformedURLException
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
    		
			SimiasCredentials credentials = SimiasCredentials.getSimiasCredentials();
			if (credentials != null)
			{
				((org.apache.axis.client.Stub) service).setUsername(credentials.getUsername());
				((org.apache.axis.client.Stub) service).setPassword(credentials.getPassword());
				//((Stub) service).setUsername(credentials.getUsername());
				//((Stub) service).setPassword(credentials.getPassword());
			}
			
//    		((Stub) service).setUsername(username);
//    		((Stub) service).setPassword(password);
            
            // if we make it here without exceptions, we've found a valid server
            return service;
		}
    	catch (Exception e)
		{
            e.printStackTrace();
		}
    	
    	return null;
	}

	public static class SimiasCredentials
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
		
		public static SimiasCredentials getSimiasCredentials()
		{
			// Read the Simias Credentials from the disk and return
			// a SimiasCredentials Object

			return new SimiasCredentials("boyd", "c3dbd2b4-7ee6-4cd3-9f26-c0020719e65b96319ecf-92bf-450e-a2c8-cc007a41b81a");
			
//			return null;
		}
	}
}
