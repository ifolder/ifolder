using System;
using System.IO;
using System.Reflection;

using Simias.Policy;

namespace Simias
{
	/// <summary>
	/// Summary description for Manager.
	/// </summary>
	public class Manager
	{
		#region Class Members
		/// <summary>
		/// Strings used to access the configuration file.
		/// </summary>
		static private string PolicyCfgSection = "Policy";
		static private string PolicyCfgAssembly = "Assembly";
		static private string PolicyCfgType = "Type";
		#endregion

		#region Factory Method
		/// <summary>
		/// Factory method for getting an IPolicy interface.
		/// </summary>
		/// <returns>A IPolicy interface object if successful. Otherwise a null is returned.</returns>
		static public IPolicyFactory GetPolicyFactory()
		{
			IPolicyFactory iPolicy = null;

			// Check in the configuration file to see if an assembly has been specified to handle the policy.
			Configuration config = Configuration.GetConfiguration();
			if ( config.Exists( PolicyCfgSection, PolicyCfgAssembly ) )
			{
				// Get the assembly name.
				string assemblyName = config.Get( PolicyCfgSection, PolicyCfgAssembly, String.Empty );
				string objectType = config.Get( PolicyCfgSection, PolicyCfgType, String.Empty );

				// Load the assembly and find our provider.
				Assembly assembly = AppDomain.CurrentDomain.Load( Path.GetFileNameWithoutExtension( assemblyName ) );
				iPolicy = assembly.CreateInstance( objectType ) as IPolicyFactory;
			}
			else
			{
				// Just use the default policy implementation.
				iPolicy = new WorkGroupPolicyFactory();
			}
		
			return iPolicy;
		}
		#endregion
	}
}
