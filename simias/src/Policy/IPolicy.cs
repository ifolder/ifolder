using System;
using System.Collections;
using System.Text;

using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Collection Store policy factory interface.
	/// </summary>
	public interface IPolicyFactory
	{
		#region Interface Methods
		/// <summary>
		/// Saves the specified system Policy object that will be applied system-wide for the specified domain.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// </summary>
		/// <param name="policy">SystemPolicy object to be saved.</param>
		/// <param name="domainID">Identifier of domain to associate system policy with.</param>
		void CommitPolicy( SystemPolicy policy, string domainID );

		/// <summary>
		/// Saves the specified system or user Policy object and associates it with the specified user.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// The caller must have read-write rights to commit a user policy.
		/// </summary>
		/// <param name="policy">Policy object to be saved.</param>
		/// <param name="member">Member object to associate this policy with.</param>
		void CommitPolicy( IPolicy policy, Member member );

		/// <summary>
		/// Creates a system policy that can be applied to the entire Simias system or a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a system policy object.</returns>
		SystemPolicy CreateSystemPolicy( string strongName, string shortDescription );

		/// <summary>
		/// Creates a user policy that can be applied to a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a user policy object.</returns>
		UserPolicy CreateUserPolicy( string strongName, string shortDescription );

		/// <summary>
		/// Deletes the specified system or user policy.
		/// </summary>
		/// <param name="policy">Policy object to delete.</param>
		void DeletePolicy( IPolicy policy );

		/// <summary>
		/// Gets the specified system policy for the specified domain.
		/// </summary>
		/// <param name="strongName">Strong name of the system policy.</param>
		/// <param name="domainID">Identifier for the domain to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated SystemPolicy object if successful. A null is returned if the specifed
		/// system policy does not exist.</returns>
		SystemPolicy GetPolicy( string strongName, string domainID );

		/// <summary>
		/// Gets the specified system or user policy that is associated with the specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy.</param>
		/// <param name="member">Member object to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated IPolicy object if successful. A null is returned if the specifed
		/// policy does not exist.</returns>
		IPolicy GetPolicy( string strongName, Member member );

		/// <summary>
		/// Gets a list of all the system and user Policy objects for the specified user.
		/// </summary>
		/// <param name="member">Member object to get policies for.</param>
		/// <returns>A reference to an ICSList object that contains all of the IPolicy objects that apply to the
		/// specified user.</returns>
		ICSList GetPolicyList( Member member );

		/// <summary>
		/// Gets a list of all the SystemPolicy objects for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier for a domain.</param>
		/// <returns>A reference to an ICSList object that contains all of the SystemPolicy objects for the
		/// specified domain.</returns>
		ICSList GetPolicyList( string domainID );
		#endregion
	}


	/// <summary>
	/// Policy object definition.
	/// </summary>
	public interface IPolicy
	{
		#region Properties
		/// <summary>
		/// Returns the list of TimeCondition objects for this policy.
		/// </summary>
		ICSList TimeConditions
		{
			get;
		}

		/// <summary>
		/// Returns the list of Rule objects for this policy.
		/// </summary>
		ICSList Rules
		{
			get;
		}
		#endregion

		#region Interface Methods
		/// <summary>
		/// Adds a time condition to the rule to indicate when the rule is effective.
		/// </summary>
		void AddTimeCondition( TimeCondition time );

		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against the input in the policy.</param>
		void AddRule( Rule rule );

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		Policy.RuleResult Apply( object input );

		/// <summary>
		/// Removes the time condition for the policy.
		/// </summary>
		void DeleteTimeCondition( TimeCondition time );

		/// <summary>
		/// Removes the specified rule from the policy.
		/// </summary>
		/// <param name="rule">Rule that is used to match against the input in the policy.</param>
		void DeleteRule( Rule rule );
		#endregion
	}


	/// <summary>
	/// Object used to encapsulate rule into a more friendly form.
	/// </summary>
	[ Serializable ]
	public class Rule
	{
		#region Class Members
		/// <summary>
		/// Object that is used to match against the input object when the Apply method is called.
		/// The type of object must be one of the Simias.Syntax types.
		/// </summary>
		private object operand;

		/// <summary>
		/// Operation to perform between input and operand.
		/// </summary>
		private Policy.RuleOperation operation;

		/// <summary>
		/// Result to return when operation is true.
		/// </summary>
		private Policy.RuleResult result;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the object that is used to match against the input object when the
		/// Apply method is called.
		/// </summary>
		public object Operand
		{
			get { return operand; }
			set { operand = value; }
		}

		/// <summary>
		/// Gets or sets the match type operation to perform between the input and operand.
		/// </summary>
		public Policy.RuleOperation Operation
		{
			get { return operation; }
			set { operation = value; }
		}

		/// <summary>
		/// Gets or set the result to return when operation is true.
		/// </summary>
		public Policy.RuleResult Result
		{
			get { return result; }
			set { result = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes the object with the specified parameters.
		/// </summary>
		/// <param name="operand">Object that is used to match against the input object when the Apply method is called.
		/// The type of object must be one of the Simias.Syntax types.</param>
		/// <param name="operation">Operation to perform between input and operand.</param>
		/// <param name="result">Result to return when operation is true.</param>
		public Rule( object operand, Policy.RuleOperation operation, Policy.RuleResult result )
		{
			// Make sure that the object is of the proper type.
			string[] names = Enum.GetNames( typeof( Syntax ) );
			foreach( string s in names )
			{
				if ( String.Compare( operand.GetType().BaseType.Name, s, true ) == 0 )
				{
					this.operand = operand;
					this.operation = operation;
					this.result = result;
					return;
				}
			}

			throw new SimiasException( "Invalid operand type." );
		}
		#endregion
	}

	[ Serializable ]
	public class TimeCondition
	{
		#region Class Members
		/// <summary>
		/// Size of the bit table used to keep track of the time condition. 
		/// 7 Days in a week - 24 hours in a day.
		/// </summary>
		private const int tableSize = 7 * 24;

		/// <summary>
		/// Table used to represent the time in a week.
		/// </summary>
		private BitArray table;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public TimeCondition()
		{
			table = new BitArray( tableSize );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time array.
		/// </summary>
		/// <param name="timeArray">Array of bytes that represent time values in a 7 day week, 24 hour period.</param>
		public TimeCondition( byte[] timeArray )
		{
			table = new BitArray( timeArray );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time string.
		/// </summary>
		/// <param name="timeArray">String that represent time values in a 7 day week, 24 hour period.</param>
		public TimeCondition( string timeString ) :
			this( new ASCIIEncoding().GetBytes( timeString ) )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the table value stored at the specified time.
		/// </summary>
		/// <param name="day">Day of week to get value for.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ) to get value for.</param>
		private bool GetTableValue( DayOfWeek day, int hour )
		{
			if ( ( hour < 0 ) || ( hour > 23 ) )
			{
				throw new SimiasException( "The hour parameter is out of range.", new ArgumentOutOfRangeException( "hour", hour, "Must be between 0 and 23." ) );
			}

			return table[ ( ( int )day * 24 ) + hour ];
		}

		/// <summary>
		/// Set or clears the specified day of week and the hour.
		/// </summary>
		/// <param name="day">Day of week to set or clear.</param>
		/// <param name="hour">Hour of day to set or clear ( 0 - 23 ).</param>
		/// <param name="value">Value to set in table.</param>
		private void SetTableValue( DayOfWeek day, int hour, bool value )
		{
			if ( ( hour < 0 ) || ( hour > 23 ) )
			{
				throw new SimiasException( "The hour parameter is out of range.", new ArgumentOutOfRangeException( "hour", hour, "Must be between 0 and 23." ) );
			}

			table.Set( ( ( int )day * 24 ) + hour, value );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Clears all selected times.
		/// </summary>
		public void Clear()
		{
			table.SetAll( false );
		}

		/// <summary>
		/// Clears the specified selected time.
		/// </summary>
		/// <param name="day">Day of week to clear.</param>
		/// <param name="hour">Hour of day to clear ( 0 - 23 ).</param>
		public void Clear( DayOfWeek day, int hour )
		{
			SetTableValue( day, hour, false );
		}

		/// <summary>
		/// Clears the specified selected range of time.
		/// </summary>
		/// <param name="startDay">Day of week to start clear.</param>
		/// <param name="endDay">Day of week to end clear.</param>
		/// <param name="startHour">Hour of day to start clear.</param>
		/// <param name="endHour">Hour of day to end clear.</param>
		public void Clear( DayOfWeek startDay, DayOfWeek endDay, int startHour, int endHour )
		{
			for ( DayOfWeek day = startDay; day <= endDay; ++day )
			{
				for ( int hour = startHour; hour < endHour; ++hour )
				{
					Clear( day, hour );
				}
			}
		}

		/// <summary>
		/// Indicates whether the specified time is clear.
		/// </summary>
		/// <param name="day">Day of week.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ).</param>
		/// <returns>True if the time is clear, otherwise false is returned.</returns>
		public bool IsClear( DayOfWeek day, int hour )
		{
			return !GetTableValue( day, hour );
		}

		/// <summary>
		/// Indicates whether the specified time is selected.
		/// </summary>
		/// <param name="day">Day of week.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ).</param>
		/// <returns>True if the time is selected, otherwise false is returned.</returns>
		public bool IsSet( DayOfWeek day, int hour )
		{
			return GetTableValue( day, hour );
		}

		/// <summary>
		/// Sets all time values to selected.
		/// </summary>
		public void Set()
		{
			table.SetAll( true );
		}

		/// <summary>
		/// Sets the specified time as selected.
		/// </summary>
		/// <param name="day">Day of week to select.</param>
		/// <param name="hour">Hour of day to select ( 0 - 23 ).</param>
		public void Set( DayOfWeek day, int hour )
		{
			SetTableValue( day, hour, true );
		}

		/// <summary>
		/// Sets the specified range of time as selected.
		/// </summary>
		/// <param name="startDay">Day of week to start selection.</param>
		/// <param name="endDay">Day of week to end selection.</param>
		/// <param name="startHour">Hour of day to start selection.</param>
		/// <param name="endHour">Hour of day to end selection.</param>
		public void Set( DayOfWeek startDay, DayOfWeek endDay, int startHour, int endHour )
		{
			for ( DayOfWeek day = startDay; day <= endDay; ++day )
			{
				for ( int hour = startHour; hour < endHour; ++hour )
				{
					Set( day, hour );
				}
			}
		}

		/// <summary>
		/// Converts the object to a byte array that represents time values in a 7 day week, 24 hour period.
		/// </summary>
		/// <returns>A byte array that represents time values in a 7 day week, 24 hour period.</returns>
		public byte[] ToByteArray()
		{
			byte[] byteArray = new byte[ tableSize / 8 ];
			table.CopyTo( byteArray, 0 );
			return byteArray;
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string.
		/// </summary>
		/// <returns>The value of this instance.</returns>
		public override string ToString()
		{
			ASCIIEncoding encode = new ASCIIEncoding();
			return encode.GetString( ToByteArray() );
		}
		#endregion
	}


	/// <summary>
	/// Contains global definitions for Policy functionality.
	/// </summary>
	public class Policy
	{
		#region Class Members
		/// <summary>
		/// Result of applying input value to policy rule.
		/// </summary>
		public enum RuleResult
		{
			/// <summary>
			/// If rule operation comparision is true, return allowed.
			/// </summary>
			Allow,

			/// <summary>
			/// If rule operation comparison is true, return denied.
			/// </summary>
			Deny
		};

		/// <summary>
		/// Operation type to perform on policy rules.
		/// </summary>
		public enum RuleOperation
		{
			/// <summary>
			/// Used to compare if two values are equal.
			/// </summary>
			Equal,

			/// <summary>
			/// Used to compare if two values are not equal.
			/// </summary>
			Not_Equal,

			/// <summary>
			/// Used to compare if a value is greater than another value.
			/// </summary>
			Greater,

			/// <summary>
			/// Used to compare if a value is less than another value.
			/// </summary>
			Less,

			/// <summary>
			/// Used to compare if a value is greater than or equal to another value.
			/// </summary>
			Greater_Equal,

			/// <summary>
			/// Used to compare if a value is less than or equal to another value.
			/// </summary>
			Less_Equal,
		};
		#endregion
	}
}
