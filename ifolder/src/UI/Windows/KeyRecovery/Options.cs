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
*
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Novell.iFolder.Utility
{
	/// <summary>
	/// Command-Line Option Handling
	/// </summary>
	public class Options
	{
		/// <summary>
		/// Constructor
		/// </summary>
		private Options()
		{
		}

		/// <summary>
		/// Parse the Command-Line Arguments
		/// </summary>
		/// <param name="obj">The Object with the Public Option Fields.</param>
		/// <param name="args">The Command-Line Arguments.</param>
		public static void ParseArguments(object obj, string[] args)
		{
			// create a hash map of the options
			Hashtable map = new Hashtable();

			Option[] options = GetOptions(obj);
 
			foreach(Option o in options)
			{
				foreach(string name in o.Names)
				{
					map.Add(name, o);
				}
			}

			// regular expresstion to parse arguments
			Regex parse = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			
			// regular expressiont to trim values
			Regex trim = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			Option option = null;

			// loop through arguments
			foreach(string arg in args)
			{
				string[] parts = parse.Split(arg, 3);

				switch(parts.Length)
				{
					// value only
					case 1:
						if (option != null)
						{
							// assign value to previous option
							option.Value = trim.Replace(parts[0], "$1");

							// clear option
							option = null;
						}
						else
						{
							// bad argument
							throw new BadArgumentException(arg);
						}
						break;

					// option only
					case 2:
						// find option
						option = (Option)map[parts[1]];
						
						if (option != null)
						{
							// mark the option as set
							option.Value = "true";
						}
						else
						{
							// unknown option
							throw new UnknownOptionException(arg);
						}
						break;

					// option and value
					case 3:
						// find option
						option = (Option)map[parts[1]];

						if (option != null)
						{
							// assign value to option
							option.Value = trim.Replace(parts[2], "$1");;

							// clear option
							option = null;
						}
						else
						{
							// unknown option
							throw new UnknownOptionException(arg);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Check Required Options
		/// </summary>
		/// <param name="obj">The Object with the Public Object Methods.</param>
		public static void CheckRequiredOptions(object obj)
		{
			Option[] options = Options.GetOptions(obj);

			foreach(Option o in options)
			{
				if (o.Required && !o.Assigned)
				{
					// required option not assigned
					throw new RequiredOptionNotAssignedException(o.Name);
				}
			}
		}

		/// <summary>
		/// Write out the options and values.
		/// </summary>
		/// <param name="obj">The Object with the Public Object Methods.</param>
		/// <param name="writer">The TextWriter Object.</param>
		public static void WriteOptions(object obj, TextWriter writer)
		{
			writer.WriteLine("Options:");
			writer.WriteLine();

			Option[] options = Options.GetOptions(obj);

			foreach(Option o in options)
			{
				writer.WriteLine(o);
			}
		}

		/// <summary>
		/// Get a List of the Public Option Fields for the Object.
		/// </summary>
		/// <param name="obj">The Object with the Public Option Fields.</param>
		/// <returns>An Array of Option objects.</returns>
		public static Option[] GetOptions(object obj)
		{
			ArrayList result = new ArrayList();

			// get all the public Option fields
			FieldInfo[] fields = obj.GetType().GetFields();
			
			foreach(FieldInfo field in fields)
			{
				// only add the Option fields
				if (field.FieldType.Equals(typeof(Option)) || field.FieldType.IsSubclassOf(typeof(Option)))
				{
					result.Add(field.GetValue(obj));
				}
			}

			return (Option[])result.ToArray(typeof(Option));
		}
	}

	/// <summary>
	/// A Command-Line Option
	/// </summary>
	public class Option
	{
		private string[] names;
		private string title;
		private string description;
		private bool required;
		private string defaultValue;
		private bool assigned;
		private string val;
		/// <summary>
		/// True if the option should be prompted for.
		/// </summary>
		protected bool prompt = true;
		private OptionEnteredHandler onOptionEntered;


		/// <summary>
		/// Delegate to handle the entered option.
		/// </summary>
		public delegate bool OptionEnteredHandler();
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="names">A comma-delimited list of option names.</param>
		/// <param name="title">A title for the option.</param>
		/// <param name="description">A description of the option.</param>
		/// <param name="required">Is this option required?</param>
		/// <param name="defaultValue">The option default value;</param>
		public Option(string names, string title, string description, bool required, string defaultValue)
		{
			this.names = names.Split(',');
			this.title = title;
			this.description = description;
			this.required = required;
			this.defaultValue = defaultValue;
			this.assigned = false;
			this.val = null;
		}

		/// <summary>
		/// The First Option Name
		/// </summary>
		public string Name
		{
			get { return names[0]; }
		}

		/// <summary>
		/// An Array of Option Names
		/// </summary>
		public string[] Names
		{
			get { return names; }
		}

		/// <summary>
		/// Get the option title
		/// </summary>
		public string Title
		{
			get { return title; }
		}

		/// <summary>
		/// The Option Description
		/// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		/// <summary>
		/// Is the Option Required?
		/// </summary>
		public bool Required
		{
			get { return required; }
			set { required = value; }
		}

		/// <summary>
		/// The Option Default Value
		/// </summary>
		public string DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		/// <summary>
		/// Has the Option been Assigned?
		/// </summary>
		public bool Assigned
		{
			get { return assigned; }
			set { assigned = value; }
		}

		/// <summary>
		/// The Option Value
		/// </summary>
		public string Value
		{
			get { return Assigned ? val : defaultValue; }
				
			set
			{
				// save the value
				val = value;

				// mark the option assigned
				assigned = true;
				if (onOptionEntered != null)
					onOptionEntered();
			}
		}

		/// <summary>
		/// Set the value without calling delegate.
		/// </summary>
		internal string InternalValue
		{
			set { val = value; assigned = true; }
		}

		/// <summary>
		/// Should we prompt for this option.
		/// </summary>
		public bool Prompt
		{
			get { return prompt; }
			set { prompt = value; }
		}

		/// <summary>
		/// Sets the handler for the option.
		/// </summary>
		public OptionEnteredHandler OnOptionEntered
		{
			set { onOptionEntered = value; }
		}

		/// <summary>
		/// Get the Option Value from the Environment
		/// </summary>
		/// <param name="variable">The Environment Variable</param>
		public void FromEnvironment(string variable)
		{
			string temp = Environment.GetEnvironmentVariable(variable);

			//DEBUG Code : Print any Env Variables parsed.
		        //Console.Error.WriteLine ("Env Variable : {0} Value : {1}",variable, temp);

			if ((temp != null) && (temp.Length > 0))
			{
				// only assign if found
				Value = temp;
			}
		}

		/// <summary>
		/// Create a String Representation of the Option.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0} {1} \"{2}\"", Name, Assigned ? "=" : "~", Value);	
		}
	}

	/// <summary>
	/// Unknown Option Exception
	/// </summary>
	public class UnknownOptionException : ArgumentException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public UnknownOptionException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Bad Argument Exception
	/// </summary>
	public class BadArgumentException : ArgumentException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public BadArgumentException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Required Option Not Assigned Exception
	/// </summary>
	public class RequiredOptionNotAssignedException : ArgumentException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public RequiredOptionNotAssignedException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// A Boolean Command-Line Option
	/// </summary>
	public class BoolOption : Option
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="names">A comma-delimited list of option names.</param>
		/// <param name="title">A title for the option.</param>
		/// <param name="description">A description of the option.</param>
		/// <param name="required">Is this option required?</param>
		/// <param name="defaultValue">The option default value;</param>
		public BoolOption(string names, string title, string description, bool required, bool defaultValue) :
			base (names, title, description, required, defaultValue.ToString())
		{
		}

		/// <summary>
		/// The Option Default Value
		/// </summary>
		public new bool DefaultValue
		{
			get { return Boolean.Parse(base.DefaultValue); }
			set { base.DefaultValue = value.ToString(); }
		}


		/// <summary>
		/// The Option Value
		/// </summary>
		public new bool Value
		{
			get
			{
				string result = base.Value;
				if (!Assigned)
				{
					// use the default value if the option was not assigned\
					result = base.DefaultValue;
				}
				return Boolean.Parse(result);
			}
			set 
			{		
				// save the value
				base.Value = value.ToString();
			}
		}
	}

	/// <summary>
	/// A Boolean Command-Line Option
	/// </summary>
	public class NoPromptOption : Option
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="names">A comma-delimited list of option names.</param>
		/// <param name="title">A title for the option.</param>
		/// <param name="description">A description of the option.</param>
		/// <param name="required">Is this option required?</param>
		/// <param name="defaultValue">The option default value;</param>
		public NoPromptOption(string names, string title, string description, bool required, string defaultValue) :
			base (names, title, description, required, defaultValue)
		{
			prompt = false;
		}
	}
}
