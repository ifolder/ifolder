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
		public Win32Window(IntPtr window)
		{
			this.window = window;
		}

		/// <summary>
		/// Get a long value for this window. See GetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public int GetWindowLong(int index)
		{
			return GetWindowLong(window, index);
		}

		/// <summary>
		/// Set a long value for this window. See SetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetWindowLong(int index, int value)
		{
			return SetWindowLong(window, index, value);
		}

		/// <summary>
		/// Turn this window into a tool window, so it doesn't show up in the Alt-tab list...
		/// </summary>
		/// 
		const int GWL_EXSTYLE = -20;
		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_EX_APPWINDOW = 0x00040000;

		public void MakeToolWindow()
		{
			int windowStyle = GetWindowLong(GWL_EXSTYLE);
			SetWindowLong(GWL_EXSTYLE, windowStyle | WS_EX_TOOLWINDOW);
		}

		public void MakeNormalWindow()
		{
			int windowStyle = GetWindowLong(GWL_EXSTYLE);
			SetWindowLong(GWL_EXSTYLE, windowStyle & ~WS_EX_TOOLWINDOW);
		}

		[DllImport("user32.dll")]
		static extern int SetWindowLong(
			IntPtr window,
			int index,
			int value);

		[DllImport("user32.dll")]
		static extern int GetWindowLong(
			IntPtr window,
			int index);
	}
}
