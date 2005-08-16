using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// Summary description for ServerAdminForm.
	/// </summary>
	public class RegistrationForm : System.Web.UI.Page
	{
		[Ajax.Method]
		public string RegisterUser( string firstName, string lastName, string userName, string password )
		{
			Member cMember = null;
			string status = "Successful";
			Store store = Store.GetStore();

			try
			{
				//
				// Verify the Simias Server domain exists if it
				// doesn't go ahead and create it.
				//
			
				Simias.Server.Domain ssDomain = new Simias.Server.Domain( false );
				Simias.Storage.Domain sDomain = ssDomain.GetSimiasServerDomain( false );
				if ( sDomain == null )
				{
					status = "Server domain does not exist";
					return status;
				}

				cMember = sDomain.GetMemberByName( userName );
				if ( cMember != null )
				{
					status = "User already exists!";
					return status;
				}

				// Add the new user to the domain
				cMember = 
					new Member(
							userName,
							Guid.NewGuid().ToString(), 
							Access.Rights.ReadOnly,
							firstName,
							lastName );

				// Note! need to first perform a known
				// hash on the password before storing it.
				//

				// Set the admin password
				Property pwd = 
					new Property( 
					"SS:PWD", 
					password );
				pwd.LocalProperty = true;
				cMember.Properties.ModifyProperty( pwd );

				sDomain.Commit( cMember );
			}
			catch(Exception e1)
			{
				status = "Internal exception!";
			}			
			
			return status;
		}
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			Ajax.Manager.Register( this, "My.Page", Ajax.Debug.None );
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//

			/*
			this.FirstEdit.Text = "";
			this.LastEdit.Text = "";
			this.UserEdit.Text = "";
			this.PasswordEdit.Text = "";
			this.PwdVerifyEdit.Text = "";
			this.ErrorLabel.Text = "";
			this.RegisterButton.Enabled = true;
			*/

			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		/*
		private void RegisterButton_Click(object sender, System.EventArgs e)
		{
			if ( this.FirstEdit.Text == null || this.FirstEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a first name";
				return;
			}

			if ( this.LastEdit.Text == null || this.LastEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a last name";
				return;
			}

			if ( this.PasswordEdit.Text == null || this.PasswordEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a password";
				return;
			}

			this.ErrorLabel.Text = "Registration successful";
			//this.RegisterButton.Enabled = false;
		}

		private void PasswordEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void FirstEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void LastEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void UserEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}

		private void PwdVerifyEdit_TextInit(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}

		private void PwdVerifyEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}
		*/
	}
}
