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

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Comparator;
import java.util.Iterator;
import java.util.TreeSet;
import java.util.Collection;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;

public class Config {

	private TreeSet storeURLs = null;
	private String filePath = null;
	private Document dom = null;
	
	private Config(String configFilePath, Document configDom)
	{
		if (configFilePath == null)
		{
			throw new IllegalArgumentException("configFilePath is null");
		}
		if (configDom == null)
		{
			throw new IllegalArgumentException("configDom is null");
		}
		
		filePath = configFilePath;
		dom = configDom;
		
		// Cause the items to be read from the DOM
		getStoreURLs();
	}
	
	/**
	 * 
	 */
	public Config(String configFilePath) {
		if (configFilePath == null)
		{
			throw new IllegalArgumentException("configFilePath is null");
		}
		
		filePath = configFilePath;
	}

	public static Config loadConfig(String configFilePath)
		throws ParserConfigurationException, SAXException, IOException
	{
		if (configFilePath == null)
		{
			throw new IllegalArgumentException("configFilePath is null");
		}
		
		// Read in the file and if successful, new up a Config,
		// set the filePath
		
		File configFile = new File(configFilePath);

		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		DocumentBuilder builder = factory.newDocumentBuilder();

		Document dom = builder.parse(configFile);
		
		Config config = new Config(configFilePath, dom);

		return config;
	}
	
	public Collection getStoreURLs()
	{
		if (storeURLs == null)
		{
			storeURLs = new TreeSet(new Comparator()
				{
					public int compare(Object arg0, Object arg1) {
						return ((String)arg0).compareTo((String)arg1);
					}
				});
			
			if (dom != null)
			{
				// Parse the Store URLs from the dom
				NodeList urlNodes = dom.getElementsByTagName("url");
				if (urlNodes != null && urlNodes.getLength() > 0)
				{
					System.out.println("urlNodes.getLength() = " + String.valueOf(urlNodes.getLength()));
					for (int i = 0; i < urlNodes.getLength(); i++)
					{
						Node urlNode = urlNodes.item(i);
						try
						{
							String url = urlNode.getFirstChild().getNodeValue();
							storeURLs.add(url);
						}
						catch(NullPointerException e)
						{
							// Ignore null pointer exceptions in the case where getFirstChild() doesn't exist.
						}
					}
				}
				else
				{
					System.out.println("urlNodes is null!");
				}
			}
		}
		return storeURLs;
	}
	
	public void addStoreURL(String newURL) throws IOException
	{
		if (newURL == null) return;
		
		try {
			// Make sure this is a valid URL before storing it
			URL testURL = new URL(newURL);
		} catch (MalformedURLException e) {
			return;
		}

		TreeSet ts = (TreeSet)getStoreURLs();
		ts.add(newURL);
		
		serializeConfigToFile();
	}

	private void serializeConfigToFile() throws IOException {
		FileWriter w = new FileWriter(filePath);
		
		if (w == null) return;
		
		w.write("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n\n");
		
		w.write("<swing-store-browser>\n");

		if (storeURLs != null)
		{
			w.write("\t<store-urls>\n");
			Iterator i = storeURLs.iterator();
			while(i.hasNext())
			{
				String url = (String)i.next();
				w.write("\t\t<url>");
				w.write(url);
				w.write("</url>\n");
			}
			w.write("\t</store-urls>\n");
		}
		
		w.write("</swing-store-browser>\n");
		
		w.close();
	}
}
