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

import java.io.IOException;
import java.io.StringReader;
import java.util.ArrayList;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.FactoryConfigurationError;
import javax.xml.parsers.ParserConfigurationException;

import org.w3c.dom.Document;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

/**
 * @author Administrator
 *
 * TODO To change the template for this generated type comment go to
 * Window - Preferences - Java - Code Style - Code Templates
 */
public class SimiasNode {

	private String name = "<UNKNOWN>";
	private String id = "";
	private ArrayList properties = new ArrayList();
	
	/**
	 * @param nodeData
	 */
	public SimiasNode(String xml) {
		Document dom = getDocument(xml);
		if (dom != null)
		{
			String nodeName = parseName(dom);
			if (nodeName != null)
			{
				name = nodeName;
			}
			
			ArrayList nodeProperties = parseProperties(dom);
			if (nodeProperties != null)
			{
				properties.addAll(nodeProperties);
			}
		}
	}

	private Document getDocument(String xml)
	{
		try {
			DocumentBuilder domBuilder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
			
			InputSource xmlInputSource = new InputSource(new StringReader(xml));

			return domBuilder.parse(xmlInputSource);
		} catch (ParserConfigurationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (FactoryConfigurationError e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (SAXException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return null;
	}
	
	private String parseName(Document rootNode)
	{
		String nodeName = null;
		
		NodeList nodeList = rootNode.getElementsByTagName("Object");
		if (nodeList != null && nodeList.getLength() > 0)
		{
			Node node = nodeList.item(0);
			NamedNodeMap attribs = node.getAttributes();
			if (attribs != null && attribs.getLength() > 0) {
				Node nameNode = attribs.getNamedItem("name");
				if (nameNode != null)
				{
					nodeName = nameNode.getNodeValue();
					SimiasNodeProperty p = new SimiasNodeProperty("Name", "String", "", nameNode.getNodeValue());
					properties.add(p);
				}
				
				// Get the other properties while we have the Object tag
				Node idNode = attribs.getNamedItem("id");
				if (idNode != null)
				{
					id = idNode.getNodeValue(); 
					SimiasNodeProperty p = new SimiasNodeProperty("ID", "String", "", idNode.getNodeValue());
					properties.add(p);
				}
				
				Node typeNode = attribs.getNamedItem("type");
				if (typeNode != null)
				{
					SimiasNodeProperty p = new SimiasNodeProperty("Type", "String", "", typeNode.getNodeValue());
					properties.add(p);
				}
			}
		}
		
		return nodeName;
	}
	
	private ArrayList parseProperties(Document rootNode)
	{
		ArrayList propsList = new ArrayList();
		
		NodeList nodeList = rootNode.getElementsByTagName("Property");
		if (nodeList != null && nodeList.getLength() > 0)
		{
			for (int i = 0; i < nodeList.getLength(); i++)
			{
				Node node = nodeList.item(i);
				SimiasNodeProperty p = parseProperty(node);
				if (p != null)
				{
					propsList.add(p);
				}
			}
		}
		
		return propsList;
	}
	
	private SimiasNodeProperty parseProperty(Node node)
	{
		SimiasNodeProperty p = new SimiasNodeProperty();
		
		NamedNodeMap attribs = node.getAttributes();
		if (attribs != null && attribs.getLength() > 0) {
			Node nameNode = attribs.getNamedItem("name");
			if (nameNode != null)
			{
				p.setName(nameNode.getNodeValue());
			}
			
			// Get the other properties while we have the Object tag
			Node typeNode = attribs.getNamedItem("type");
			if (typeNode != null)
			{
				p.setType(typeNode.getNodeValue());
			}

			Node flagsNode = attribs.getNamedItem("flags");
			if (flagsNode != null)
			{
				// TODO: Interpret the node flags
				p.setFlags(flagsNode.getNodeValue());
			}
		}

		Node firstChild = node.getFirstChild();
		if (firstChild != null)
		{
			String nodeValue = firstChild.getNodeValue();
			if (nodeValue != null)
			{
				p.setValue(nodeValue);
			}
		}
		
		return p;
	}
	/**
	 * @return Returns the name.
	 */
	public String getName() {
		return name;
	}
	/**
	 * @return Returns the properties.
	 */
	public ArrayList getProperties() {
		return properties;
	}
	
	/* Override toString() to return the name */
	public String toString()
	{
		return name;
	}

	/**
	 * @return Returns the id.
	 */
	public String getId() {
		return id;
	}
	
	/**
	 * Returns true if the node has Types = Collections
	 * @return
	 */
	public boolean isCollection()
	{
		for (int i = 0; i < properties.size(); i++)
		{
			SimiasNodeProperty p = (SimiasNodeProperty)properties.get(i);
			String name = p.getName();
			String value = p.getValue();
			
			if (name.equals("Types") && value.equals("Collection"))
			{
				return true;
			}
		}
		
		return false;
	}
}
