/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *-----------------------------------------------------------------------------
  *
  *                 $Author: aron Bockover (aaron@aaronbock.net)
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Collections;
using DBus;

namespace NetworkManager
{
    public enum DeviceType {
        Unknown,
        Wired,
        Wireless
    }
    
    /// <summary>
    /// class DeviceProxy
    /// </summary>
    [Interface("org.freedesktop.NetworkManager.Devices")]
    internal abstract class DeviceProxy
    {
        /* Unsupported methods: 
            
            getProperties
            setLinkActive
            getCapabilities
        */

        [Method] public abstract string getName();
        [Method] public abstract uint getMode();
        [Method] public abstract int getType();
        [Method] public abstract string getHalUdi();
        [Method] public abstract uint getIP4Address();
        [Method] public abstract string getHWAddress();
        [Method] public abstract bool getLinkActive();
        [Method] public abstract NetworkProxy getActiveNetwork();
        [Method] public abstract NetworkProxy [] getNetworks();
    }
    
    public class Device : IEnumerable
    {
        private DeviceProxy device;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device"></param>
        internal Device(DeviceProxy device)
        {
            this.device = device;
        }
        
        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("Name:             " + Name + "\n");
            builder.Append("Type:             " + Type + "\n");
            builder.Append("Mode:             " + Mode + "\n");
            builder.Append("HAL UDI:          " + HalUdi + "\n");
            builder.Append("IP4 Address:      " + IP4Address + "\n");
            builder.Append("Hardware Address: " + HardwareAddress + "\n");
            builder.Append("Link Active:      " + (IsLinkActive ? "Yes" : "No") + "\n");
            builder.Append("Networks: \n");
            
            int i = 0;
            foreach(Network network in Networks) {
                builder.Append("  [" + (i++) + "] Name:      " + network.Name + "\n");
                builder.Append("      Active:    " + (network.Equals(ActiveNetwork) ? "Yes" : "No") + "\n");
                builder.Append("      Strength:  " + network.Strength + "\n");
                builder.Append("      Frequency: " + network.Frequency + "\n");
                builder.Append("      Rate:      " + network.Rate + "\n");
                builder.Append("      Encrypted: " + (network.IsEncrypted ? "Yes" : "No") + "\n");
                builder.Append("      Mode:      " + network.Mode + "\n");
            }
            
            if(i == 0) {
                builder.Append("  (none)\n");
            }
            
            return builder.ToString();
        }
             
        /// <summary>
        /// Gets the device Name
        /// </summary>
        public string Name {
            get {
                return device.getName();
            }
        }
        
        /// <summary>
        /// Gets the device type
        /// </summary>
        public DeviceType Type {
            get {
                switch(device.getType()) {
                    case 1: return DeviceType.Wired;
                    case 2: return DeviceType.Wireless;
                    default: return DeviceType.Unknown;
                }
            }
        }
        
        /// <summary>
        /// Gets the Device mode
        /// </summary>
        public uint Mode {
            get {
                return device.getMode();
            }
        }

        /// <summary>
        /// Gets the HalUdi
        /// </summary>
        public string HalUdi {
            get {
                return device.getHalUdi();
            }
        }

        /// <summary>
        /// Gets the IP4Address
        /// </summary>
        public System.Net.IPAddress IP4Address {
            get {
                return new System.Net.IPAddress(device.getIP4Address());
            }
        }
        
        /// <summary>
        /// Gets the device hardwareaddress
        /// </summary>
        public string HardwareAddress {
            get {
                return device.getHWAddress();
            }
        }
        
        /// <summary>
        /// Gets the status of LinkActive
        /// </summary>
        public bool IsLinkActive {
            get {
                return device.getLinkActive();
            }
        }
        /// <summary>
        /// Gets the ActiveNetwork
        /// </summary>
        public Network ActiveNetwork {
            get {
                if(Type != DeviceType.Wireless) {
                    return null;
                }
                
                try {
                    return new Network(device.getActiveNetwork());
                } catch(DBusException) {
                    return null;
                }
            }
        }
        
        public IEnumerator GetEnumerator()
        {
            foreach(NetworkProxy network in device.getNetworks()) {
                yield return new Network(network);
            }
        }
        
        /// <summary>
        /// Initialize Networks
        /// </summary>
        public Network [] Networks {
            get {
                ArrayList list = new ArrayList();
                
                try {
                    foreach(NetworkProxy network in device.getNetworks()) {
                        list.Add(new Network(network));
                    }
                } catch(Exception) {
                }
                
                return list.ToArray(typeof(Network)) as Network [];
            }
        }
    }
}
