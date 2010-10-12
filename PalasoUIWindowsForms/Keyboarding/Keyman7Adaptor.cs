using System;
using System.Collections.Generic;
#if !MONO
using Keyman7Interop;
#endif
using Palaso.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{
    // ABOUT Keyman7Interop
    // I created this by running:
    // E:\Program Files\Common Files\Tavultesoft\Keyman Engine 7.0>tlbimp kmcomapi.dll /out:"c:\palasolib\lib\Keyman7Interop.dll
    // Notice that the com dll is in the "common files".
    // The separation into 2 classes and all the try/catch blocks are there because without them,
    // when keyman7 is not installed, when the code is jit'ed, the app will crash.

    internal class Keyman7Adaptor 
    {
		public static List<KeyboardDescriptor> KeyboardDescriptors
        {
            get
            {
				if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
					try
					{
						return InnerKeyman7Wrapper.KeyboardDescriptors;
					}
					catch (Exception) {}
                }
				return new List<KeyboardDescriptor>();
            }
        }

        public static bool EngineAvailable
        {
            get
            {
				if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
					try
					{
						return InnerKeyman7Wrapper.EngineAvailable;
					}
					catch (Exception) {}
                }
                return false;
            }
        }

        public static void ActivateKeyboard(string name)
        {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

            try
            {
                InnerKeyman7Wrapper.ActivateKeyboard(name);
            }
			catch(Palaso.Reporting.ErrorReport.ProblemNotificationSentToUserException)
            {
				throw; // needed for tests to know that a message box would have been shown
            }
			catch (Exception)
            {
            }
        }

		public static void ActivateKeyboard(KeyboardDescriptor keyboard)
		{
			if (keyboard.KeyboardingEngine != Engines.Keyman7) return;
			ActivateKeyboard(keyboard.KeyboardName);
		}

        public static void Deactivate()
        {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

            try
            {
                InnerKeyman7Wrapper.Deactivate();
            }
			catch (Exception)
            {
            }
        }

        public static bool HasKeyboardNamed(string name)
        {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return false;
			}

            try
            {
                return InnerKeyman7Wrapper.HasKeyboardNamed(name);
            }
			catch (Exception)
            {
                return false;
            }
        }

		public static bool HasKeyboard(KeyboardDescriptor keyboard)
		{
			if (keyboard.KeyboardingEngine != Engines.Keyman7) return false;
			return HasKeyboardNamed(keyboard.KeyboardName);
		}

        public static string GetActiveKeyboard()
        {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return null;
			}

            try
            {
                return InnerKeyman7Wrapper.GetActiveKeyboard();
            }
			catch (Exception)
            {
                return null;
            }
        }

		public static KeyboardDescriptor GetActiveKeyboardDescriptor()
		{
			KeyboardDescriptor activeKeyboard = null;
			string activeKeyboardName = GetActiveKeyboard();
			if (!String.IsNullOrEmpty(activeKeyboardName))
			{
				return new KeyboardDescriptor(activeKeyboardName, Engines.Keyman6, activeKeyboardName);
			}
			return activeKeyboard;
		}
    }

    internal class InnerKeyman7Wrapper
    {
		public static List<KeyboardDescriptor> KeyboardDescriptors
        {
            get
            {
				List<KeyboardDescriptor> keyboards =
					new List<KeyboardDescriptor>();

#if !MONO
                Keyman7Interop.TavultesoftKeymanClass keyman = new Keyman7Interop.TavultesoftKeymanClass();
                foreach (Keyman7Interop.IKeymanKeyboard keyboard in keyman.Keyboards)
                {
					KeyboardDescriptor descriptor = new KeyboardDescriptor(keyboard.Name, Engines.Keyman7, keyboard.Name);
                    keyboards.Add(descriptor);
                }
#endif
                return keyboards;
            }
        }

        public static bool EngineAvailable
        {
			get
			{
#if MONO
				return false;
#else
				//if we were able to load the libarry, we assume it is installed
				return null != new Keyman7Interop.TavultesoftKeymanClass();
#endif
			}
        }

        public static void ActivateKeyboard(string name)
        {
#if !MONO
            TavultesoftKeymanClass keyman = new Keyman7Interop.TavultesoftKeymanClass();
            int oneBasedIndex = keyman.Keyboards.IndexOf(name);

            if(oneBasedIndex < 1)
            {
				 Palaso.Reporting.ErrorReport.NotifyUserOfProblem("The keyboard '{0}' could not be activated using Keyman 7.", name);
                return;
            }
            string s = keyman.Keyboards[oneBasedIndex].Name;
            keyman.Control.ActiveKeyboard = keyman.Keyboards[oneBasedIndex];
#endif
        }

        public static void Deactivate()
        {
#if !MONO
            TavultesoftKeymanClass keyman = new Keyman7Interop.TavultesoftKeymanClass();
            keyman.Control.ActiveKeyboard = null;
#endif
        }

        public static bool HasKeyboardNamed(string name)
        {
#if MONO
			return false;
#else
            TavultesoftKeymanClass keyman = new Keyman7Interop.TavultesoftKeymanClass();
			return(keyman.Keyboards.IndexOf(name) > 0 /*remember, keyman indices are 1-based*/);
#endif
        }

        public static string GetActiveKeyboard()
        {
#if !MONO
            TavultesoftKeymanClass keyman = new Keyman7Interop.TavultesoftKeymanClass();
            if (keyman.Control.ActiveKeyboard != null)
            {
                return keyman.Control.ActiveKeyboard.Name;
            }
#endif
            return null;
        }
    }
}