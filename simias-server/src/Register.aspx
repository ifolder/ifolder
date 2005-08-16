<%@ Page language="c#" Codebehind="Register.aspx.cs" AutoEventWireup="false" Inherits="Simias.Server.RegistrationForm" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>
	<head>
		<title>Simias Server user registration</title> 
		<!-- <script type="text/javascript" src="exampleValidation.js"></script> -->
		<!-- <script type="text/javascript" src="genericValidation.js"></script> -->
		<link href="Register.css" type=text/css rel=stylesheet >
		<script type=text/javascript>
		
		// set the focus on the password text box, unless no username exists
		function setFocus()
		{
			var tb = document.getElementById("firstEdit");
			if (tb.value.length > 0)
			{
				var pwd = document.getElementById("passwordEdit");
				if (pwd.value.length > 0)
				{
					document.getElementById("passwordEdit").select();
				}
				else
				{
					document.getElementById("passwordEdit").focus();
				}
			}
			else
			{
				tb.focus();
			}
		}

		// Event fired when control is given to the password edit box
		function passwordFocus()
		{
			var pwd = document.getElementById("pwdVerifyEdit");
			if (pwd.value.length > 0)
			{
				pwd.select();
			}
			else
			{
				pwd.focus();
			}
		}
		
		// Event fired when control is given to the password edit box
		function passwordBlur()
		{
			document.getElementById("pwdVerifyEdit").deselect();
		}
		
		function clearNotify()
		{
			document.getElementById("notify").value = "";
		}
		
		function testListener()
		{
			document.getElementById("registerButton").disabled = false;		
			clearNotify();
		}
	
		function clearForm(result)
		{
			document.getElementById("registerButton").disabled = false;		
			document.getElementById( 'firstEdit' ).value = "";
			document.getElementById( 'lastEdit' ).value = "";
			document.getElementById( 'userEdit' ).value = "";
			document.getElementById( 'passwordEdit' ).value = "";
			document.getElementById( 'pwdVerifyEdit' ).value = "";
			var notifyelm = document.getElementById("notify");
			notifyelm.value = "";
			notifyelm.style.display="none";
			
			var firstElm = document.getElementById( 'firstEdit' );
			firstElm.value = "";
			firstElm.focus();
		}
			
		function passwordCheck()
		{
			var notifyelm = document.getElementById("notify");
			var pwd = document.getElementById("passwordEdit");
			if ( pwd.value.length == 0)
			{
				//pwd.select();
				notifyelm.value = "You must enter a password";
				notifyelm.style.display="inline";
				return;
			}
			
			var pwdv = document.getElementById("pwdVerifyEdit");
			if ( pwd.value != pwdv.value )
			{
				notifyelm.value = "Passwords don't match!";
				notifyelm.style.display="inline";
				return;
			}
			
			notifyelm.style.display="none";
			notifyelm.value = "";
			return;
		}
		
		function passwordVerifyCheck()
		{
			var notifyelm = document.getElementById("notify");
			var pwdv = document.getElementById("pwdVerifyEdit");
			if ( pwdv.value.length == 0)
			{
				//pwdv.select();
				notifyelm.value = "You must enter a password";
				notifyelm.style.display="inline";
				return;
			}
			
			var pwd = document.getElementById("passwordEdit");
			if ( pwd.value != pwdv.value )
			{
				notifyelm.value = "Passwords don't match!";
				notifyelm.style.display="inline";
				return;
			}
			
			notifyelm.style.display="none";
			notifyelm.value = "";
			return;
		}
			
		function onSubmit()
		{
			var notifyelm = document.getElementById("notify");
			if ( document.getElementById( 'passwordEdit' ).value == "" ||
				document.getElementById( 'pwdVerifyEdit' ).value == "" )
			{
				notifyelm.style.display="inline";
				notifyelm.value = "You must enter a password";
				setFocus();
				document.getElementById( 'passwordEdit' ).select();
				return;
			}		
			
			if ( document.getElementById( 'passwordEdit' ).value !=
				document.getElementById( 'pwdVerifyEdit' ).value )
			{
				notifyelm.style.display="inline";
				notifyelm.value = "Passwords don't match!";
				setFocus();
				return;
			}
			
			var result = My.Page.RegisterUser(
							document.getElementById( 'firstEdit' ).value,
							document.getElementById( 'lastEdit' ).value,
							document.getElementById( 'userEdit' ).value,
							document.getElementById( 'passwordEdit' ).value);
			
			if ( result.value == "Successful" )
			{
				document.getElementById("registerButton").disabled = true;
				notifyelm.style.display="inline";
				notifyelm.value = "Successful";
				window.setTimeout("clearForm()", 3000 );
			}
			else
			{
				notifyelm.style.display="inline";
				notifyelm.value = result.value;
				setFocus();
			}
		}
	
		function installListeners( e )
		{
			var element = document.getElementById( 'firstEdit' );
			addEvent( element, 'change', testListener, false );
			
			var pwdVerify = document.getElementById( 'pwdVerifyEdit' );
			addEvent( pwdVerify, 'blur', passwordVerifyCheck, false );
				
			var pwd = document.getElementById( 'passwordEdit' );
			addEvent( pwd, 'blur', passwordCheck, false );
			
			var submitElement = document.getElementById( 'registerButton' );
			addEvent( submitElement, 'click', onSubmit, false );
		}
		
		function onLoad()
		{
			document.getElementById( 'firstEdit' ).value = "";
			document.getElementById( 'lastEdit' ).value = "";
			document.getElementById( 'userEdit' ).value = "";
			document.getElementById( 'passwordEdit' ).value = "";
			document.getElementById( 'pwdVerifyEdit' ).value = "";
		
			var notifyelm = document.getElementById( 'notify' );
			notifyelm.disabled = true;
			//notifyelm.visible = false;
			notifyelm.value = "";
			notifyelm.style.display="none";

			setFocus();
		}		
			
		// Portable way of adding an event listener
		function addEvent( elm, eventType, fn, useCapture )
		{
			if ( elm.addEventListener )
			{
				elm.addEventListener( eventType, fn, useCapture );
				return true;
			}
			else if ( elm.attachEvent )
			{
				var r = elm.attachEvent( 'on' + eventType, fn );
				return r;
			}
			else
			{
				elm['on' + eventType] = fn;
			}
		}
		
		addEvent( window, 'load', installListeners, false );
		window.onload = onLoad;
		</script>
</head>
<body>
<!-- <h1>Simias Server User Registration</h1> -->
<form action="" runat=server>
<table height="100%" cellSpacing=0 cellPadding=0 width="100%" border=0>
	<tr>
    <td vAlign=middle align=center width="100%">
      <table class=registerDialog cellSpacing=0 cellPadding=0 border=0>
        <tr bgColor=#e50000>
          <td><IMG alt="Simias Server User Registration" src="images/product_title.gif" ></td>
          <td style="BORDER-LEFT: white 1px solid" vAlign=middle align=center><a id=helpButton href="help/en/login.html" >Help</a></td>
        </tr>
        <tr>
          <td colSpan=2>
            <table class=registerContent cellSpacing=0 cellPadding=8 border=0>
              <tr bgColor=#edeeec>
                <td class=loginMessageType style="PADDING-TOP: 16px" align=right><label id=messageType></label></td>
                <td class=loginMessage style="PADDING-TOP: 16px"><label id=messageText></label></td>
                <td style="PADDING-RIGHT: 2px; PADDING-LEFT: 2px; PADDING-BOTTOM: 2px; PADDING-TOP: 2px"></td>
              </tr>
              <tr bgColor=#edeeec>
                <td style="PADDING-TOP: 12px" align="left">First Name:</td>
                <td style="PADDING-TOP: 12px"><input id=firstEdit type=text></td>
                <td style="PADDING-RIGHT:2px; PADDING-LEFT: 2px; PADDING-BOTTOM: 2px; PADDING-TOP: 2px">&nbsp;</td>
              </tr>
              <tr bgColor=#edeeec>
                <td style="padding-top:12px" align="left">Last Name:</td>
                <td style="padding-top:12px"><input type="text" id="lastEdit"></td>
                <td style="PADDING-RIGHT: 2px; PADDING-LEFT: 2px; PADDING-BOTTOM: 2px; PADDING-TOP: 2px">&nbsp;</td>
              </tr>
              <tr bgColor=#edeeec>
                <td align="left">Username:</td>
                <td style="padding-top:12px"> <input type="text" id="userEdit"/></td>
                <td style="padding-right:2px; padding-left:2px; padding-bottom:2px; padding-top:2px">&nbsp;</td>
              </tr>
              <tr bgColor=#edeeec>
                <td style="padding-top:12px" align=left>Password:</td>
                <td><input id="passwordEdit" type="password"></td>
                <td style="PADDING-RIGHT: 2px; PADDING-LEFT: 2px; PADDING-BOTTOM: 2px; PADDING-TOP: 2px">&nbsp;</td></tr>
              <tr bgColor=#edeeec>
                <td align="left">Password (again):</td>
                <td><input id="pwdVerifyEdit" type="password"></td>
                <td style="PADDING-RIGHT: 2px; PADDING-LEFT: 2px; PADDING-BOTTOM: 2px; PADDING-TOP: 2px">&nbsp;</td>
              </tr>
              <tr bgColor=#edeeec>
                <td style="PADDING-TOP: 12px" align=center colSpan=1><input id=registerButton type=button value=Register></td>
                <td align="left"><input type="text" id="notify" style="display:none"></td>
				<td style="PADDING-RIGHT: 2px;PADDING-LEFT: 2px;PADDING-BOTTOM: 2px;PADDING-TOP: 2px">&nbsp;</td>
			  </tr>
			</table>
			</td>
			</tr>
	</table>
	</td>
	</tr>
</table>
			<!--
			<div class="userLabels">
				<label for="firstLabel">First Name:</label><br/>
				<span id="errorFirstName" class="errormessage"></span>
				<label for="lastLabel">Last Name:</label><br/>	
				<label for="userLabel">Username:</label><br/>
				<label for="passwordLabel">Password:</label><br/>
				<label for="pwdVerifyLabel">Password (again):</label>
			</div>
			
			<div class="userEdits">
				<input type="text" name="lastEdit" id="lastEdit"><br/>
				<input type="text" id="firstEdit" name="firstEdit"><br/>
				<input type="text" name="userEdit" id="userEdit"><br/>
				<input type="password" name="passwordEdit" id="passwordEdit"><br/>
				<input type="password" name="pwdVerifyEdit" id="pwdVerifyEdit">				
			</div>
			
			<div>
				<p><input type="button" name="submitButton" id="submitButton" value="Register"></p>
			</div>
			-->
		</form>
	</body>
</html>
