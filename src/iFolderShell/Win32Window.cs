using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Win32Util
{
	/// <summary>
	/// Encapsulates window functions that aren't in the framework.
	/// NOTE: This class is not thread-safe. 
	/// </summary>
	public class Win32Window
	{
		IntPtr window;

		/// <summary>
		/// Create a Win32Window
		/// </summary>
		/// <param name="window">The window handle</param>
		public Win32Window()
		{
		}

		public IntPtr Window
		{
			set
			{
				window = value;
			}
		}

		/// <summary>
		/// Bring a window to the top
		/// </summary>
		public void BringWindowToTop()
		{
			BringWindowToTop(window);
		}

		/// <summary>
		/// Find a window by name or class
		/// </summary>
		/// <param name="className">Name of the class, or null</param>
		/// <param name="windowName">Name of the window, or null</param>
		/// <returns></returns>
		public static Win32Window FindWindow(string className, string windowName)
		{
			IntPtr window = FindWindowWin32(className, windowName);
			Win32Window win32Window = null;
			if (window != IntPtr.Zero)
			{
				win32Window = new Win32Window();
				win32Window.Window = window;
			}

			return win32Window;
		}

		[DllImport("user32.dll")]
		static extern bool BringWindowToTop(IntPtr window);
		
		[DllImport("user32.dll", EntryPoint="FindWindow")]
		static extern IntPtr FindWindowWin32(string className, string windowName);
	}
}
