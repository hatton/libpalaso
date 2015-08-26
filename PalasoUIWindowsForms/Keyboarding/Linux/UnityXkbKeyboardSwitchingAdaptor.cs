﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if __MonoCS__
using System;
using X11.XKlavier;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// Class for dealing with xkb keyboards on Unity (as found in Trusty >= 13.10)
	/// </summary>
	public class UnityXkbKeyboardSwitchingAdaptor: XkbKeyboardSwitchingAdaptor
	{
		public UnityXkbKeyboardSwitchingAdaptor(IXklEngine engine): base(engine)
		{
		}

		protected override void SelectKeyboard(IKeyboardDefinition keyboard)
		{
			var xkbKeyboard = keyboard as XkbKeyboardDescription;
			if (xkbKeyboard != null)
			{
				if (xkbKeyboard.GroupIndex >= 0)
					UnityIbusKeyboardSwitchingAdaptor.SelectKeyboard((uint)xkbKeyboard.GroupIndex);
			}
		}
	}
}
#endif
