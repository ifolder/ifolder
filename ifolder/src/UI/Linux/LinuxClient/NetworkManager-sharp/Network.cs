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
using DBus;

namespace NetworkManager
{
    /// <summary>
    /// clas NetworkProxy
    /// </summary>
    [Interface("org.freedesktop.NetworkManager.Devices")]
    internal abstract class NetworkProxy
    {
        /* Unsupported methods: 
            
            getProperties
        */

        [Method] public abstract string getName();
        //[Method] public abstract string getAddress(); Calling this crashes NM for me
        [Method] public abstract int getStrength();
        [Method] public abstract double getFrequency();
        [Method] public abstract int getRate();
        [Method] public abstract bool getEncrypted();
        [Method] public abstract uint getMode();
    }
    
    /// <summary>
    /// class Network
    /// </summary>
    public class Network
    {
        private NetworkProxy network;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">NetworkProxy</param>
        internal Network(NetworkProxy network)
        {
            this.network = network;
        }
        
        /// <summary>
        /// Gets the Network Name
        /// </summary>
        public string Name {
            get {
                return network.getName();
            }
        }
        
        /*public string Address {
            get {
                return network.getAddress();
            }
        }*/
        
        /// <summary>
        /// Gets the Network strength
        /// </summary>
        public int Strength {
            get {
                return network.getStrength();
            }
        }
        
        /// <summary>
        /// Gets the Network Frequency
        /// </summary>
        public double Frequency {
            get {
                return network.getFrequency();
            }
        }
        
        /// <summary>
        /// Gets the Network rate
        /// </summary>
        public int Rate {
            get {
                return network.getRate();
            }
        }
        
        /// <summary>
        /// Gets the Network Encryption status
        /// </summary>
        public bool IsEncrypted {
            get {
                return network.getEncrypted();
            }
        }
        
        /// <summary>
        /// Gets the Network mode
        /// </summary>
        public uint Mode {
            get {
                return network.getMode();
            }
        }

        public override bool Equals(object o)
        {
            Network compare = o as Network;
            if(compare == null) {
                return false;
            }
            
            return Name == compare.Name;
        }
        
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
