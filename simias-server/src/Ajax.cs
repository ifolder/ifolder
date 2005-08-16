using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Ajax
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MethodAttribute : Attribute
	{
	}

	[Flags]
	public enum Debug
	{
		None         = 0,
		RequestText  = 1,
		ResponseText = 2,
		Errors       = 4,
		All          = 7
	}

	public class Manager
	{
		public static void Register(Control control)
		{
			Register(control, control.GetType().FullName);
		}

		public static void Register(Control control, string prefix)
		{
			Register(control, prefix, Debug.None);
		}

		public static void Register(Control control, Debug debug)
		{
			Register(control, control.GetType().FullName, debug);
		}

		public static void Register(Control control, string prefix, Debug debug)
		{
			string pageScript = @"
<script>
function Ajax_GetXMLHttpRequest() {
 	var x = null;
	if (typeof XMLHttpRequest != ""undefined"") {
		x = new XMLHttpRequest();
	} else {
		try {
			x = new ActiveXObject(""Msxml2.XMLHTTP"");
		} catch (e) {
			try {
				x = new ActiveXObject(""Microsoft.XMLHTTP"");
			} catch (e) {
			}
		}
	}
	return x;
}

function Ajax_CallBack(target, args, clientCallBack, debugRequestText, debugResponseText, debugErrors) {
	var x = Ajax_GetXMLHttpRequest();
	var url = document.location.href.substring(0, document.location.href.length - document.location.hash.length);
	x.open(""POST"", url, clientCallBack ? true : false);
	x.setRequestHeader(""Content-Type"", ""application/x-www-form-urlencoded"");
	if (clientCallBack) {
		x.onreadystatechange = function() {
			if (x.readyState != 4) {
				return;
			}
			if (debugResponseText) {
				alert(x.responseText);
			}
			var result = eval(""("" + x.responseText + "")"");
			if (debugErrors && result.error) {
				alert(""error: "" + result.error);
			}
			clientCallBack(result);
		}
	}
	var encodedData = ""Ajax_CallBackTarget="" + target;
	if (args) {
		for (var i in args) {
			encodedData += ""&Ajax_CallBackArgument"" + i + ""="" + encodeURIComponent(args[i]);
		}
	}
	if (document.forms.length > 0) {
		var form = document.forms[0];
		for (var i = 0; i < form.length; ++i) {
			var element = form.elements[i];
			if (element.name) {
				var elementValue = null;
				if (element.nodeName == ""INPUT"") {
					var inputType = element.getAttribute(""TYPE"").toUpperCase();
					if (inputType == ""TEXT"" || inputType == ""PASSWORD"" || inputType == ""HIDDEN"") {
						elementValue = element.value;
					} else if (inputType == ""CHECKBOX"" || inputType == ""RADIO"") {
						if (element.checked) {
							elementValue = element.value;
						}
					}
				}
				if (elementValue) {
					encodedData += ""&"" + element.name + ""="" + encodeURIComponent(elementValue);
				}
			}
		}
	}
	if (debugRequestText) {
		alert(encodedData);
	}
	x.send(encodedData);
	var result = null;
	if (!clientCallBack) {
		if (debugResponseText) {
			alert(x.responseText);
		}
		result = eval(""("" + x.responseText + "")"");
		if (debugErrors && result.error) {
			alert(""error: "" + result.error);
		}
	}
	delete x;
	return result;
}
</script>";
			control.Page.RegisterClientScriptBlock(typeof(Ajax.Manager).FullName, pageScript);

			Type type = control.GetType();

			StringBuilder controlScript = new StringBuilder();
			controlScript.Append("\n<script>\n");

			string[] prefixParts = prefix.Split('.', '+');
			controlScript.AppendFormat("var {0} = {{\n", prefixParts[0]);

			for (int i = 1; i < prefixParts.Length; ++i)
			{
				controlScript.AppendFormat("\"{0}\": {{\n", prefixParts[i]);
			}

			bool firstMethod = true;
			foreach (MethodInfo methodInfo in type.GetMethods())
			{
				object[] attributes = methodInfo.GetCustomAttributes(typeof(Ajax.MethodAttribute), true);
				if (attributes != null && attributes.Length > 0)
				{
					if (!firstMethod)
					{
						controlScript.AppendFormat(",\n");
					}
					controlScript.AppendFormat("\n\"{0}\": function(", methodInfo.Name);

					foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
					{
						controlScript.Append(paramInfo.Name + ", ");
					}

					controlScript.AppendFormat("clientCallBack) {{\n\treturn Ajax_CallBack('{0}.{1}', [", type.FullName, methodInfo.Name);

					bool firstParam = true;
					foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
					{
						if (!firstParam)
						{
							controlScript.Append(", ");
						}
						controlScript.Append(paramInfo.Name);
						firstParam = false;
					}

					controlScript.AppendFormat("], clientCallBack, {0}, {1}, {2});\n}}",
						(debug & Debug.RequestText) == Debug.RequestText ? "true" : "false",
						(debug & Debug.ResponseText) == Debug.ResponseText ? "true" : "false",
						(debug & Debug.Errors) == Debug.Errors ? "true" : "false");

					firstMethod = false;
				}
			}

			if (firstMethod)
			{
				throw new ApplicationException(string.Format("{0} does not contain any methods with the Ajax.Method attribute.", type.FullName));
			}

			controlScript.Append("\n\n");

			for (int i = 0; i < prefixParts.Length; ++i)
			{
				controlScript.Append("}");
			}

			controlScript.Append(";\n</script>");
			control.Page.RegisterClientScriptBlock("Ajax." + type.FullName, controlScript.ToString());

			control.PreRender += new EventHandler(OnPreRender);
		}

		public static string CallBackTarget
		{
			get
			{
				return HttpContext.Current.Request.Form["Ajax_CallBackTarget"];
			}
		}

		public static bool IsCallBack
		{
			get
			{
				return CallBackTarget != null;
			}
		}

		static void OnPreRender(object s, EventArgs e)
		{
			HttpRequest req = HttpContext.Current.Request;
			HttpResponse resp = HttpContext.Current.Response;

			string target = CallBackTarget;
			if (target != null)
			{
				Control control = s as Control;
				if (control != null)
				{
					int lastDot = target.LastIndexOf('.');
					if (lastDot != -1)
					{
						string typeName = target.Substring(0, lastDot);
						Type type = control.GetType();
						if (type.FullName == typeName)
						{
							string methodName = target.Substring(lastDot + 1);
							MethodInfo methodInfo = type.GetMethod(methodName);
							if (methodInfo != null)
							{
								object val = null;
								string error = null;
								try
								{
									object[] parameters = new object[methodInfo.GetParameters().Length];
									int i = 0;
									foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
									{
										object param = null;
										string paramValue = req.Form["Ajax_CallBackArgument" + i];
										if (paramValue != null)
										{
											param = Convert.ChangeType(paramValue, paramInfo.ParameterType);
										}
										parameters[i] = param;
										++i;
									}
									val = methodInfo.Invoke(control, parameters);
								}
								catch (Exception ex)
								{
									error = ex.Message;
								}
								resp.ContentType = "application/x-javascript";
								resp.Cache.SetCacheability(HttpCacheability.NoCache);
								StringBuilder sb = new StringBuilder();
								try
								{
									WriteResult(sb, val, error);
								}
								catch (Exception ex2)
								{
									sb = new StringBuilder();
									WriteResult(sb, null, ex2.Message);
								}
								resp.Write(sb.ToString());
								resp.End();
							}
						}
					}
				}
			}
		}

		static void WriteResult(StringBuilder sb, object val, string error)
		{
			sb.Append("{\"value\":");
			WriteValue(sb, val);
			sb.Append(",\"error\":");
			WriteValue(sb, error);
			sb.Append("}");
		}

		static void WriteValue(StringBuilder sb, object val)
		{
			if (val == null)
			{
				sb.Append("null");
			}
			else
			{
				if (val is String)
				{
					WriteString(sb, val as String);
				}
				else if (val is Double ||
					val is Single ||
					val is Int64 ||
					val is Int32 ||
					val is Int16 ||
					val is Byte)
				{
					sb.Append(val);
				}
				else if (val is DateTime)
				{
					sb.Append("new Date(\"");
					sb.Append(((DateTime)val).ToString("MMMM, d yyyy HH:mm:ss"));
					sb.Append("\")");
				}
				else if (val is DataSet)
				{
					WriteDataSet(sb, val as DataSet);
				}
				else if (val is DataTable)
				{
					WriteDataTable(sb, val as DataTable);
				}						
				else if (val is DataRow)
				{
					WriteDataRow(sb, val as DataRow);
				}
				else if (val is IEnumerable)
				{
					WriteEnumerable(sb, val as IEnumerable);
				}
				else
				{
					throw new ApplicationException(string.Format("Returning {0} objects is not supported.", val.GetType().FullName));
				}
			}
		}

		static void WriteString(StringBuilder sb, string s)
		{
			sb.Append("\"");
			foreach (char c in s)
			{
				switch (c)
				{
					case '\"':
						sb.Append("\\\"");
						break;
					case '\\':
						sb.Append("\\\\");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						int i = (int)c;
						if (i < 32 || i > 127)
						{
							sb.AppendFormat("\\u{0:X04}", i);
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			sb.Append("\"");
		}

		static void WriteDataSet(StringBuilder sb, DataSet ds)
		{
			sb.Append("{\"Tables\":{");
			bool firstTable = true;
			foreach (DataTable table in ds.Tables)
			{
				if (!firstTable)
				{
					sb.Append(",");
				}
				sb.AppendFormat("\"{0}\":", table.TableName);
				WriteDataTable(sb, table);
				firstTable = false;
			}
			sb.Append("}}");
		}

		static void WriteDataTable(StringBuilder sb, DataTable table)
		{
			sb.Append("{\"Rows\":[");
			bool firstRow = true;
			foreach (DataRow row in table.Rows)
			{
				if (!firstRow)
				{
					sb.Append(",");
				}
				WriteDataRow(sb, row);
				firstRow = false;
			}
			sb.Append("]}");
		}

		static void WriteDataRow(StringBuilder sb, DataRow row)
		{
			sb.Append("{");
			bool firstColumn = true;
			foreach (DataColumn column in row.Table.Columns)
			{
				if (!firstColumn)
				{
					sb.Append(",");
				}
				sb.AppendFormat("\"{0}\":", column.ColumnName);
				WriteValue(sb, row[column]);
				firstColumn = false;
			}
			sb.Append("}");
		}

		static void WriteEnumerable(StringBuilder sb, IEnumerable e)
		{
			sb.Append("[");
			bool firstVal = true;
			foreach (object val in e)
			{
				if (!firstVal)
				{
					sb.Append(",");
				}
				WriteValue(sb, val);
				firstVal = false;
			}
			sb.Append("]");
		}
	}
}
