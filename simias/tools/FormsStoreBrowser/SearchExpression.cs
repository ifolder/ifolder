using System;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for Search.
	/// </summary>
	public class SearchExpression
	{
		#region Class Members
		private string propertyName;
		private string propertyType;
		private string propertyValue;
		private string operation;
		private DisplayNode node;
		#endregion

		#region Constructor
		public SearchExpression( DisplayNode target )
		{
			node = target;
		}

		public SearchExpression( DisplayNode target, string propertyName, string propertyType, string propertyValue, string operation ) :
			this ( target )
		{
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.propertyValue = propertyValue;
			this.operation = operation;
		}
		#endregion

		#region Properties
		public DisplayNode Target
		{
			get
			{
				return node;
			}
		}

		public string PropertyName
		{
			get
			{
				return propertyName;
			}
			set
			{
				propertyName = value;
			}
		}

		public string PropertyType
		{
			get
			{
				return propertyType;
			}
			set
			{
				propertyType = value;
			}
		}

		public string PropertyValue
		{
			get
			{
				return propertyValue;
			}
			set
			{
				propertyValue = value;
			}
		}

		public string Operation
		{
			get
			{
				return operation;
			}
			set
			{
				operation = value;
			}
		}
		#endregion
	}
}
