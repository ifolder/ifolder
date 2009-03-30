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
using Mono.Unix;

namespace NetworkManager
{
    public delegate void NetworkStateChangedHandler(object o, NetworkStateChangedArgs args);
    
    /// <summary>
    /// class NetworkState Changed Args
    /// </summary>
    public class NetworkStateChangedArgs : EventArgs
    {
        public bool Connected;
    }

    /// <summary>
    /// class networkDetect
    /// </summary>
    public class NetworkDetect : IDisposable
    {
        public event NetworkStateChangedHandler StateChanged;
        
        private Manager nm_manager;
        private State last_state;
        
        private static NetworkDetect instance;

        /// <summary>
        /// Gets / Sets Network instance
        /// </summary>
        public static NetworkDetect Instance {
            get {
                if(instance == null) {
                    instance = new NetworkDetect();
                }
                
                return instance;
            }
        }
        
        /// <summary>
        /// Dispose the objects
        /// </summary>
        public void Dispose()
        {
            	if(nm_manager != null) 
		{
			nm_manager.StateChange -= OnNetworkManagerEvent;
		        nm_manager.DeviceNowActive -= OnNetworkManagerEvent;
		        nm_manager.DeviceNoLongerActive -= OnNetworkManagerEvent;

                	nm_manager.Dispose();
            	}
        }
	
        /// <summary>
        /// Check if network manager is null
        /// </summary>
        /// <returns>true if null</returns>
	public bool isNull()       
	{
		if( this.nm_manager == null )
			return true;
		return false;
	}
 
        /// <summary>
        /// Constructor
        /// </summary>
        private NetworkDetect()
        {
            try {
                ConnectToNetworkManager();
           } catch(Exception e) {
                nm_manager = null;
//                Console.WriteLine("Cannot connect to NetworkManager");
//                Console.WriteLine("An available, working network connection will be assumed");
//                LogCore.Instance.PushWarning(
//                    Catalog.GetString("Cannot connect to NetworkManager"),
//                    Catalog.GetString("An available, working network connection will be assumed"),
//                    false);
           }
        }

        /// <summary>
        /// Connect to Networkmanager
        /// </summary>
        private void ConnectToNetworkManager()
        {
            nm_manager = new Manager();
            nm_manager.StateChange += OnNetworkManagerEvent;
            nm_manager.DeviceNowActive += OnNetworkManagerEvent;
            nm_manager.DeviceNoLongerActive += OnNetworkManagerEvent;
            last_state = nm_manager.State;
        }
        
        /// <summary>
        /// Event Handler for Netowrk manager event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void OnNetworkManagerEvent(object o, EventArgs args)
        {
            try {
                State new_state = nm_manager.State;
                if(new_state != last_state && (new_state == State.Connected || new_state == State.Disconnected)) {
                    last_state = new_state;
                    
                    NetworkStateChangedHandler handler = StateChanged;
                    if(handler != null) {
                        NetworkStateChangedArgs state_changed_args = new NetworkStateChangedArgs();
                        state_changed_args.Connected = Connected;
                        handler(this, state_changed_args);
                    }
                    
                    Device active_device = nm_manager.ActiveDevice;
                    
                    if(Connected && active_device != null) {
//                    	Console.WriteLine("Network Connection Established: {0} ({1})",
//                    					  active_device.Name, active_device.IP4Address);
//                        LogCore.Instance.PushDebug("Network Connection Established", String.Format("{0} ({1})", 
//                            active_device.Name, active_device.IP4Address));
                    } else if(Connected) {
//                    	Console.WriteLine("Network Connection Established: Active Device Unknown");
//                        LogCore.Instance.PushDebug("Network Connection Established", "Active Device Unknown");
                    } else {
//                    	Console.WriteLine("Network Connection Unavailable: Disconnected");
//                        LogCore.Instance.PushDebug("Network Connection Unavailable", "Disconnected");
                    }
                }
            } catch(Exception) {
            }
        }
        
        /// <summary>
        /// Gets if connected
        /// </summary>
        public bool Connected {
            get {
                try {
                    return nm_manager == null ? true : nm_manager.State == State.Connected;
                } catch(Exception) {
                    return true;
                }
            }
        }
        
        /// <summary>
        /// gets a network manager object
        /// </summary>
        public Manager Manager {
            get {
                return nm_manager;
            }
        }
    }
}
