﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;
using System.Diagnostics;
using System.Text;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class handles switching of keyboards on Wasta versions >= 14 (aka
	/// Mint 17/Cinnamon).  Wasta 12 worked the same as Precise (Ubuntu 12.04), and default
	/// Wasta 14 without IBus appears to also work the same as Precise with XKB keyboards only.
	/// Starting with Wasta 14, if IBus is used for keyboard inputs, things are joined together,
	/// but not the same as the combined keyboard processing in Trusty (Ubuntu 14.04).
	/// It also works for other desktop environments that use combined ibus keyboards, e.g.
	/// XFCE.
	/// </summary>
	[CLSCompliant(false)]
	public class CombinedIbusKeyboardSwitchingAdaptor: IbusKeyboardSwitchingAdaptor
	{
		// These should not change while the program is running, and they're expensive to obtain.
		// So we've made them static.
		internal static string DefaultLayout { get; set; }
		internal static string DefaultVariant { get; set; }
		internal static string DefaultOption { get; set; }
		internal static string[] LatinLayouts { get; set; }
		internal static bool UseXmodmap { get; set; }
		private static readonly string[] knownXModMapFiles = {".xmodmap", ".xmodmaprc", ".Xmodmap", ".Xmodmaprc"};

		private IKeyboardDefinition _defaultKeyboard;

		public CombinedIbusKeyboardSwitchingAdaptor(IIbusCommunicator ibusCommunicator): base(ibusCommunicator)
		{
		}

		/// <summary>
		/// Set the XKB layout to use henceforward using the setxkbmap program.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetXkbLayout(string layout, string variant, string option)
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = "/usr/bin/setxkbmap";
			var bldr = new StringBuilder();
			bldr.AppendFormat ("-layout {0}", layout);
			if (!String.IsNullOrEmpty(variant))
				bldr.AppendFormat (" -variant {0}", variant);
			if (!String.IsNullOrEmpty(option))
				bldr.AppendFormat (" -option {0}", option);
			//Console.WriteLine ("DEBUG: about to call \"{0} {1}\"", startInfo.FileName, bldr.ToString ());
			startInfo.Arguments = bldr.ToString();
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			using (var process = Process.Start(startInfo))
			{
				process.WaitForExit();
				process.Close();
			}

			if (UseXmodmap)
				SetXModMap();
		}

		/// <summary>
		/// Add the user's modifications to whatever keyboard mapping is active.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		private static void SetXModMap()
		{
			string homedir = Environment.GetEnvironmentVariable("HOME");
			foreach (string file in knownXModMapFiles)
			{
				string filepath = System.IO.Path.Combine(homedir, file);
				if (!System.IO.File.Exists(filepath))
					continue;
				var startInfo = new ProcessStartInfo();
				startInfo.FileName = "xmodmap";
				startInfo.Arguments = filepath;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				using (var process = Process.Start(startInfo))
				{
					process.WaitForExit();
					process.Close();
				}
				break;
			}
		}

		internal override bool IBusKeyboardAlreadySet(IbusKeyboardDescription keyboard)
		{
			if (keyboard == null || keyboard.IBusKeyboardEngine == null)
			{
				UnsetKeyboard();
				return true;
			}

			// Even when GlobalCachedInputContext.Keyboard is already set to keyboard we still
			// want to set the keyboard again
			return false;
		}

		/// <summary>
		/// Set the XKB layout from the IBus keyboard.
		/// </summary>
		/// <remarks>
		/// This mimics the behavior of the ibus panel applet code.
		/// </remarks>
		protected override void SelectKeyboard(IKeyboardDefinition keyboard)
		{
			var ibusKeyboard = keyboard as IbusKeyboardDescription;

			var parentLayout = ibusKeyboard.ParentLayout;
			if (parentLayout == "en")
				parentLayout = "us";	// layout is a country code, not a language code!
			var variant = ibusKeyboard.IBusKeyboardEngine.LayoutVariant;
			var option = ibusKeyboard.IBusKeyboardEngine.LayoutOption;
			Debug.Assert(parentLayout != null);

			bool need_us_layout = false;
			foreach (string latinLayout in LatinLayouts)
			{
				if (parentLayout == latinLayout && variant != "eng")
				{
					need_us_layout = true;
					break;
				}
				if (!String.IsNullOrEmpty(variant) && String.Format("{0}({1})", parentLayout, variant) == latinLayout)
				{
					need_us_layout = true;
					break;
				}
			}

			if (String.IsNullOrEmpty(parentLayout) || parentLayout == "default")
			{
				parentLayout = DefaultLayout;
				variant = DefaultVariant;
			}
			if (String.IsNullOrEmpty(variant) || variant == "default")
				variant = null;
			if (String.IsNullOrEmpty(option) || option == "default")
			{
				option = DefaultOption;
			}
			else if (!string.IsNullOrEmpty(DefaultOption))
			{
				if (DefaultOption.Split(',').Contains(option, StringComparison.InvariantCulture))
					option = DefaultOption;
				else
					option = String.Format("{0},{1}", DefaultOption, option);
			}

			if (need_us_layout)
			{
				parentLayout = parentLayout + ",us";
				// If we have a variant, we need to indicate an empty variant to
				// match the "us" layout.
				if (!String.IsNullOrEmpty(variant))
					variant = variant + ",";
			}

			SetXkbLayout(parentLayout, variant, option);

			if (!ibusKeyboard.Name.StartsWith("xkb:", StringComparison.InvariantCulture))
			{
				// Set the IBus keyboard
				var context = GlobalCachedInputContext.InputContext;
				context.SetEngine(ibusKeyboard.IBusKeyboardEngine.LongName);
			}
		}

		/// <summary>
		/// On Cinnamon everything is ibus, but we still need a system type, so we impersonate that
		/// </summary>
		public override KeyboardType Type
		{
			get { return KeyboardType.System; }
		}

		/// <summary>
		/// This adaptor doesn't make use of XkbKeyboardDefinition objects, so we have to
		/// figure out the default keyboard here, searching the available keyboards for the
		/// best match to _defaultLayout.  An explicit xkb: keyboard is preferred, but we
		/// settle for another match (or anything at all) if we need to.
		/// </summary>
		public override IKeyboardDefinition DefaultKeyboard
		{
			get
			{
				if (_defaultKeyboard == null)
				{
					var desired = String.Format("xkb:{0}:", DefaultLayout);
					if (!String.IsNullOrEmpty (DefaultVariant))
						desired = String.Format ("xkb:{0}\\{1}:", DefaultLayout, DefaultVariant);
					var pattern = String.Format("[^A-Za-z]{0}[^A-Za-z]|^{0}[^A-Za-z]|.*[^A-Za-z]{0}$",
						DefaultLayout);
					var regex = new System.Text.RegularExpressions.Regex(pattern);
					IKeyboardDefinition first = null;
					foreach (var kbd in Keyboard.Controller.AllAvailableKeyboards)
					{
						if (first == null)
							first = kbd;	// last-ditch value if all else fails
						if (kbd.Layout.StartsWith(desired, StringComparison.InvariantCulture)
							|| kbd.Layout == DefaultLayout)
						{
							_defaultKeyboard = kbd;
							break;
						}
						if (regex.IsMatch(kbd.Layout))
						{
							_defaultKeyboard = kbd;
						}
					}
					if (_defaultKeyboard == null)
						_defaultKeyboard = first;
				}
				return _defaultKeyboard;
			}
		}

		/// <summary>
		/// Implementation is not required because the default implementation of KeyboardController
		/// is sufficient.
		/// </summary>
		public override IKeyboardDefinition ActiveKeyboard
		{
			get { return null; }
		}
	}
}
#endif
