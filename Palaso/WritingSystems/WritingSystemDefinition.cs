using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This class stores the information used to define various writing system properties.
	/// </summary>
	public class WritingSystemDefinition
	{
		public enum SortRulesType
		{
			/// <summary>
			/// Default Unicode ordering rules (actually CustomICU without any rules)
			/// </summary>
			[Description("Default Ordering")]
			DefaultOrdering,
			/// <summary>
			/// Custom Simple (Shoebox/Toolbox) style rules
			/// </summary>
			[Description("Custom Simple (Shoebox style) rules")]
			CustomSimple,
			/// <summary>
			/// Custom ICU rules
			/// </summary>
			[Description("Custom ICU rules")]
			CustomICU,
			/// <summary>
			/// Use the sort rules from another language. When this is set, the SortRules are interpreted as a cultureId for the language to sort like.
			/// </summary>
			[Description("Same as another language")]
			OtherLanguage
		}

		//This is the version of our writingsystemDefinition implementation and is mostly used for migration purposes.
		//This should not be confused with the version of the locale data contained in this writing system.
		//That information is stored in the "VersionNumber" property.
		static public int LatestWritingSystemDefinitionVersion
		{
			get { return 1; }
		}

		private RFC5646Tag _rfcTag;

		private string _languageName;

		private string _abbreviation;
		private bool _isLegacyEncoded;

		private string _versionNumber;
		private string _versionDescription;

		private DateTime _dateModified;

		private string _defaultFontName;
		private float _defaultFontSize;
		private string _keyboard;

		private SortRulesType _sortUsing;
		private string _sortRules;
		private string _spellCheckingId;

		private string _nativeName;
		private bool _rightToLeftScript;
		private ICollator _collator;

		/// <summary>
		/// singleton
		/// </summary>
		private static List<Iso15924Script> _scriptOptions = new List<Iso15924Script>();
	   /// <summary>
		/// singleton
		/// </summary>
		private static List<Iso639LanguageCode> _languageCodes;

		/// <summary>
		/// For overriding the other identifier fields, to specify a custom RFC5646
		/// </summary>
		//private string _customLanguageTag;

		public WritingSystemDefinition()
		{
			_sortUsing = SortRulesType.DefaultOrdering;
			_isLegacyEncoded = false;
			_rfcTag = new RFC5646Tag("qaa",String.Empty,String.Empty,String.Empty,String.Empty);
		   // _defaultFontSize = 10; //arbitrary
		}

		public WritingSystemDefinition(string iso)
			: this()
		{
			_rfcTag.Language = iso;
			_abbreviation = _rfcTag.Script = _languageName = _rfcTag.Variant = _rfcTag.Region = _nativeName = string.Empty;
		}

		public WritingSystemDefinition(string iso, string script, string region, string variant, string abbreviation, bool rightToLeftScript)
			: this()
		{
			ISO639 = String.IsNullOrEmpty(iso)?"qaa":iso;
			Script = script;
			Region = region;
			Variant = variant;
			_abbreviation = abbreviation;
			_rightToLeftScript = rightToLeftScript;
		}

		private string GetRfc5646PrivateUseTag(string variant)
		{
			string[] variantAndPrivateUseTags = GetRfc5646VariantAndPrivateUseTagsFromVariant(variant);
			if (variantAndPrivateUseTags.Length > 1)
			{
				return variantAndPrivateUseTags[1];
			}
			return String.Empty;
		}

		private string[] GetRfc5646VariantAndPrivateUseTagsFromVariant(string variant)
		{
			string[] partsOfVariant = variant.Split(new[] { "-x-" }, StringSplitOptions.None);
			return partsOfVariant;
		}

		private string GetRfc5646Variant(string variant)
		{
			string[] variantAndExtensions = GetRfc5646VariantAndPrivateUseTagsFromVariant(variant);
			return variantAndExtensions[0];
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="ws">The ws.</param>
		public WritingSystemDefinition(WritingSystemDefinition ws)
			: this(ws._rfcTag.Language, ws._rfcTag.Script, ws._rfcTag.Region, ws._rfcTag.Variant, ws._abbreviation, ws._rightToLeftScript)
		{
			_defaultFontName = ws._defaultFontName;
			_defaultFontSize = ws._defaultFontSize;
			_keyboard = ws._keyboard;
			_versionNumber = ws._versionNumber;
			_versionDescription = ws._versionDescription;
			_nativeName = ws._nativeName;
			_sortUsing = ws._sortUsing;
			_sortRules = ws._sortRules;
			_spellCheckingId = ws._spellCheckingId;
			_dateModified = ws._dateModified;
			_isLegacyEncoded = ws._isLegacyEncoded;
			_rfcTag = new RFC5646Tag(ws._rfcTag);
			_languageName = ws._languageName;
		}

		/// <summary>
		/// Provides a list of ISO639 language codes.  Uses ISO639 639-1 and 639-3 where ISO639 639-1 is not available.
		/// </summary>
		public static IList<Iso639LanguageCode> ValidIso639LanguageCodes
		{
			get
			{
				return RFC5646Tag.ValidIso639LanguageCodes;
			}
		}

		//This is the version of the locale data contained in this writing system.
		//This should not be confused with the version of our writingsystemDefinition implementation which is mostly used for migration purposes.
		//That information is stored in the "LatestWritingSystemDefinitionVersion" property.
		virtual public string VersionNumber
		{
			get { return _versionNumber; }
			set { UpdateString(ref _versionNumber, value); }
		}

		virtual public string VersionDescription
		{
			get { return _versionDescription; }
			set { UpdateString(ref _versionDescription, value); }
		}

		virtual public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}


		/// <summary>
		/// Note: this treats the etic and emic extensions as if they were variants, which we can get
		/// away with for now, but maybe not if this class grows to be extension aware.
		/// Ideally, these should be suffixes rather than private use
		/// </summary>
		[Obsolete("The setter on this property is being deprecated. Please use the new SetIpaStatus method instead.")]
		virtual public IpaStatusChoices IpaStatus
		{
			get
			{
				if (Rfc5646TagIsPhonemicConform)
				{
					return IpaStatusChoices.IpaPhonemic;
				}
				if (Rfc5646TagIsPhoneticConform)
				{
					return IpaStatusChoices.IpaPhonetic;
				}
				if (VariantSubTagIsIpaConform)
				{
					return IpaStatusChoices.Ipa;
				}
				return IpaStatusChoices.NotIpa;
			}

			set
			{
				SetIpaStatus(value);
			}
		}

		[Obsolete("The setter on this property is being deprecated. Please use the new SetIsVoice method instead.")]
		virtual public bool IsVoice
		{
			get
			{
				bool rfcTagindicatesVoiceWritingSystem = ScriptSubTagIsAudioConform && VariantSubTagIsAudioConform;
				if (rfcTagindicatesVoiceWritingSystem) { return true; }
				return false;
			}
			set
			{
				SetIsVoice(value);
			}
		}

		private bool VariantSubTagIsAudioConform
		{
			get
			{
				return _rfcTag.PrivateUseContainsPart(WellKnownSubTags.Audio.PrivateUseSubtag);
			}
		}

		private bool ScriptSubTagIsAudioConform
		{
			get { return _rfcTag.Script.Equals(WellKnownSubTags.Audio.Script,StringComparison.OrdinalIgnoreCase); }
		}

		/// <summary>
		/// Todo: this could/should become an ordered list of variant tags
		/// </summary>
		virtual public string Variant
		{
			get
			{
				bool privateUseIsPopulatedAndVariantIsNot = String.IsNullOrEmpty(_rfcTag.Variant) && !String.IsNullOrEmpty(_rfcTag.PrivateUse);
				bool variantIsPopulatedAndPrivateUseIsNot = !String.IsNullOrEmpty(_rfcTag.Variant) && String.IsNullOrEmpty(_rfcTag.PrivateUse);
				bool variantAndPrivateUseAreBothPopulated = !String.IsNullOrEmpty(_rfcTag.Variant) && !String.IsNullOrEmpty(_rfcTag.PrivateUse);
				string variantToReturn = "";
				if(variantIsPopulatedAndPrivateUseIsNot)
				{
					variantToReturn = _rfcTag.Variant;
				}
				else if(privateUseIsPopulatedAndVariantIsNot)
				{
					variantToReturn = _rfcTag.PrivateUse;
				}
				else if(variantAndPrivateUseAreBothPopulated)
				{
					variantToReturn = _rfcTag.Variant + "-" + _rfcTag.PrivateUse;
				}
				return variantToReturn;
			}
			set
			{
				if (value == null || value == Variant) { return; }
				bool variantEndsInXorXDash = value.EndsWith("-x") || value.EndsWith("-x-");
				bool variantDoesNotContainPrivateUseSubtags = !value.Contains("x-");
				bool variantIsRfc5646ConformPrivateUseSubTag = value.StartsWith("x-");
				if(variantEndsInXorXDash)
				{
					throw new ArgumentException("The variant may not end in '-x' or '-x-'");
				}
				if (variantDoesNotContainPrivateUseSubtags)
				{
					_rfcTag.Variant = value;
					_rfcTag.PrivateUse = "";
				}
				else if (variantIsRfc5646ConformPrivateUseSubTag)
				{
					_rfcTag.Variant = "";
					_rfcTag.PrivateUse = value;
				}
				else
				{
					string variantAccordingToRfc5646 = GetRfc5646Variant(value);
					string privateUseTagAccordingToRfc5646 = GetRfc5646PrivateUseTag(value);
					_rfcTag.Variant = variantAccordingToRfc5646;
					_rfcTag.PrivateUse = privateUseTagAccordingToRfc5646;
				}
				Modified = true;
				CheckIfRfcTagIsValid();
			}
		}

		private void CheckIfRfcTagIsValid()
		{
			bool variantIsAudioConformButScriptIsNot = VariantSubTagIsAudioConform && !ScriptSubTagIsAudioConform;
			if(variantIsAudioConformButScriptIsNot)
			{
				throw new ArgumentException("The script subtag must be set to " + WellKnownSubTags.Audio.Script + " when the variant tag indicates an audio writing system.");
			}
			bool variantContainsVoiceMarkerAsWellAsSomeFormOfIpaMarker = VariantSubTagIsAudioConform &&
															   VariantSubtagIndicatesSomeFormOfIpa;
			if(variantContainsVoiceMarkerAsWellAsSomeFormOfIpaMarker)
			{
				throw new ArgumentException("A writing system may not be marked as audio and ipa at the same time.");
			}
		}

		private bool VariantSubtagIndicatesSomeFormOfIpa
		{
			get { return VariantSubTagIsIpaConform || Rfc5646TagIsPhonemicConform || Rfc5646TagIsPhoneticConform; }
		}

		private bool VariantSubTagIsIpaConform
		{
			get
			{
				return _rfcTag.VariantContainsPart(WellKnownSubTags.Ipa.VariantSubtag);
			}
		}

		private bool Rfc5646TagIsPhoneticConform
		{
			get
			{
				return  _rfcTag.VariantContainsPart(WellKnownSubTags.Ipa.VariantSubtag) &&
					_rfcTag.PrivateUseContainsPart(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag);
			}
		}

		private bool Rfc5646TagIsPhonemicConform
		{
			get
			{
				return _rfcTag.VariantContainsPart(WellKnownSubTags.Ipa.VariantSubtag) &&
					_rfcTag.PrivateUseContainsPart(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag);
			}
		}

		public void SetIsVoice(bool isVoice)
		{
			if (IsVoice == isVoice) { return; }
			if (isVoice)
			{
				IpaStatus = IpaStatusChoices.NotIpa;
				Keyboard = string.Empty;
				Script = WellKnownSubTags.Audio.Script;
				_rfcTag.AddToPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
			}
			else
			{
				_rfcTag.Script = String.Empty;
				_rfcTag.RemoveFromPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
			}
			Modified = true;
			CheckIfRfcTagIsValid();
		}

		public void SetIpaStatus(IpaStatusChoices ipaStatus)
		{
			if(IpaStatus == ipaStatus)
			{
				return;
			}
			_rfcTag.RemoveFromPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
			/* "There are some variant subtags that have no prefix field,
			 * eg. fonipa (International IpaPhonetic Alphabet). Such variants
			 * should appear after any other variant subtags with prefix information."
			 */
			_rfcTag.RemoveFromPrivateUse("x-etic");
			_rfcTag.RemoveFromPrivateUse("x-emic");
			_rfcTag.RemoveFromVariant("fonipa");

			switch (ipaStatus)
			{
				default:
					break;
				case IpaStatusChoices.Ipa:
					_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
					break;
				case IpaStatusChoices.IpaPhonemic:
					_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
					_rfcTag.AddToPrivateUse(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag);
					break;
				case IpaStatusChoices.IpaPhonetic:
					_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
					_rfcTag.AddToPrivateUse(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag);
					break;
			}
			Modified = true;
		}

		public void SetAllRfc5646LanguageTagComponents(string language, string script, string region, string variant)
		{
			ISO639 = language;
			Script = script;
			Region = region;
			Variant = variant;
			CheckIfRfcTagIsValid();
		}

		virtual public string Region
		{
			get
			{
				return _rfcTag.Region;
			}
			set
			{
				if (value == Region) { return; }
				_rfcTag.Region = value;
				Modified = true;
			}
		}

		/// <summary>
		/// The ISO639-639 code which is also the Ethnologue code.
		/// </summary>
		[Obsolete("Please use ISO639")]
		virtual public string ISO
		{
			get { return ISO639; }
			set { ISO639 = value; }
		}

		/// <summary>
		/// The ISO-639 code which is also the Ethnologue code.
		/// </summary>
		virtual public string ISO639
		{
			get
			{
				return _rfcTag.Language;
			}
			set
			{
				if (value == ISO639) { return; }
				_rfcTag.Language = value;
				Modified = true;
			}
		}

		virtual public string Abbreviation
		{
			get
			{
				return _abbreviation;
			}
			set
			{
				UpdateString(ref _abbreviation, value);
			}
		}

		virtual public string Script
		{
			get
			{
				return _rfcTag.Script;
			}
			set
			{
				if (value == Script)
				{
					return;
				}
				_rfcTag.Script = value;
				Modified = true;
				CheckIfRfcTagIsValid();
			}
		}

		virtual public string LanguageName
		{
			get
			{
				bool customLanguageNameIsSet = !String.IsNullOrEmpty(_languageName);
				if (!customLanguageNameIsSet)
				{
					foreach (Iso639LanguageCode code in ValidIso639LanguageCodes)
					{
						if(code.Code.Equals(ISO639))
						{
							return code.Name;
						}
					}
				}
				else if (customLanguageNameIsSet)
				{
					return _languageName;
				}
				return "Unknown Language";
			}
			set
			{
				UpdateString(ref _languageName, value);
			}
		}


		protected void UpdateString(ref string field, string value)
		{
			if (field == value)
				return;

			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
			{
				return;
			}
			Modified = true;
			field = value;
		}

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so they can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		virtual public string StoreID { get; set; }

		virtual public string DisplayLabel
		{
			get
			{
				//jh (Oct 2010) made it start with RFC5646 because all ws's in a lang start with the
				//same abbreviation, making imppossible to see (in SOLID for example) which you chose.
				bool languageIsUnknown = RFC5646.Equals("qaa", StringComparison.OrdinalIgnoreCase);
				if (!String.IsNullOrEmpty(RFC5646) && !languageIsUnknown)
				{
					return RFC5646;
				}
				if (languageIsUnknown)
				{
					if (!String.IsNullOrEmpty(_abbreviation))
					{
						return _abbreviation;
					}
					if (!String.IsNullOrEmpty(_languageName))
					{
						string n = _languageName;
						return n.Substring(0, n.Length > 4 ? 4 : n.Length);
					}
				}
				return "???";
			}
		}

		virtual public string ListLabel
		{
			get
			{
				string n = string.Empty;
				if (!String.IsNullOrEmpty(_languageName))
				{
					n=_languageName;
				}
				else
				{
					n = DisplayLabel;
				}
				string details = "";
				if(IpaStatus != IpaStatusChoices.NotIpa)
				{
					switch (IpaStatus)
					{
						case IpaStatusChoices.Ipa:
							details += "IPA-";
							break;
						case IpaStatusChoices.IpaPhonetic:
							details += "IPA-etic-";
							break;
						case IpaStatusChoices.IpaPhonemic:
							details += "IPA-emic-";
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				else if (!String.IsNullOrEmpty(_rfcTag.Script))
				{
					details += _rfcTag.Script+"-";
				}
				if (!String.IsNullOrEmpty(_rfcTag.Region))
				{
					details += _rfcTag.Region + "-";
				}
				if (IpaStatus == IpaStatusChoices.NotIpa && !String.IsNullOrEmpty(_rfcTag.Variant))
				{
					details += _rfcTag.Variant + "-";
				}

				if (IsVoice)
				{
					details = details.Replace("Zxxx-", "");
					details += "voice";
				}
				details = details.Trim(new char[] { '-' });
				if (details.Length > 0)
					details = " ("+details + ")";
				return n+details;
			}
		}

		virtual public string RFC5646
		{
			get
			{
//                if(!string.IsNullOrEmpty(_customLanguageTag))
//                {
//                    return _customLanguageTag;
//                }
				return _rfcTag.CompleteTag;
			}
//            set
//            {
//                _customLanguageTag=value;
//            }
		}

		[Obsolete("Use RFC5646 Property.")]
		public string Id
		{
			get
			{
				return RFC5646;
			}
		}

		virtual public string VerboseDescription
		{
			get
			{
				var summary = new StringBuilder();
				summary.AppendFormat(" {0}", LanguageName);
				if (!String.IsNullOrEmpty(Region))
				{
					summary.AppendFormat(" in {0}", Region);
				}
				if (!String.IsNullOrEmpty(Script))
				{
					summary.AppendFormat(" written in {0} script", CurrentScriptOptionLabel);
				}

				summary.AppendFormat(". ({0})", RFC5646);
				return summary.ToString().Trim();
			}
		}

		private string CurrentScriptOptionLabel
		{
			get
			{
				Iso15924Script option = Iso15924Script;
				return option == null ? _rfcTag.Script : option.Label;
			}
		}

		/// <summary>
		/// If we don't have an option for the current script, returns null
		/// </summary>
		virtual public Iso15924Script Iso15924Script
		{
			get
			{
				string script = Script;
				if (String.IsNullOrEmpty(script))
				{
					script = "latn";
				}
				foreach (var option in ScriptOptions)
				{
					if (option.Code == script)
					{
						return option;
					}
				}
				return null;
			}
		}

		public static List<Iso15924Script> ScriptOptions
		{
			get
			{
				return RFC5646Tag.ValidIso15924Scripts;
			}
		}

		virtual public bool Modified { get; set; }

		virtual public bool MarkedForDeletion { get; set; }

		virtual public string DefaultFontName
		{
			get
			{
				return _defaultFontName;
			}
			set
			{
				UpdateString(ref _defaultFontName, value);
			}
		}

		virtual public float DefaultFontSize
		{
			get
			{
				return _defaultFontSize;
			}
			set
			{
				if (value == _defaultFontSize)
				{
					return;
				}
				if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
				{
					throw new ArgumentOutOfRangeException();
				}
				_defaultFontSize = value;
				Modified = true;
			}
		}

		virtual public string Keyboard
		{
			get
			{
				if(String.IsNullOrEmpty(_keyboard))
				{
					return "";
				}
				return _keyboard;
			}
			set
			{
				UpdateString(ref _keyboard, value);
			}
		}

		virtual public bool RightToLeftScript
		{
			get
			{
				return _rightToLeftScript;
			}
			set
			{
				if(value != _rightToLeftScript)
				{
					Modified = true;
					_rightToLeftScript = value;
				}
			}
		}

		/// <summary>
		/// The windows "NativeName" from the Culture class
		/// </summary>
		virtual public string NativeName
		{
			get
			{
				return _nativeName;
			}
			set
			{
				UpdateString(ref _nativeName, value);
			}
		}

		virtual public SortRulesType SortUsing
		{
			get { return _sortUsing; }
			set
			{
				if (value != _sortUsing)
				{
					_sortUsing = value;
					_collator = null;
					Modified = true;
				}
			}
		}

		virtual public string SortRules
		{
			get { return _sortRules ?? string.Empty; }
			set
			{
				_collator = null;
				UpdateString(ref _sortRules, value);
			}
		}

		virtual public string SpellCheckingId
		{
			get
			{
				if (string.IsNullOrEmpty(_spellCheckingId))
				{
					return _rfcTag.Language;
				}
				return _spellCheckingId;
			}
			set { UpdateString(ref _spellCheckingId, value); }
		}

		/// <summary>
		/// Returns an ICollator interface that can be used to sort strings based
		/// on the custom collation rules.
		/// </summary>
		virtual public ICollator Collator
		{
			get
			{
				if (_collator == null)
				{
					switch (SortUsing)
					{
						case SortRulesType.DefaultOrdering:
							_collator = new IcuRulesCollator(String.Empty); // was SystemCollator(null);
							break;
						case SortRulesType.CustomSimple:
							_collator = new SimpleRulesCollator(SortRules);
							break;
						case SortRulesType.CustomICU:
							_collator = new IcuRulesCollator(SortRules);
							break;
						case SortRulesType.OtherLanguage:
							_collator = new SystemCollator(SortRules);
							break;
					}
				}
				return _collator;
			}
		}

		virtual public bool IsLegacyEncoded
		{
			get
			{
				return _isLegacyEncoded;
			}
			set
			{
				if(value != _isLegacyEncoded)
				{
					Modified = true;
					_isLegacyEncoded = value;
				}
			}
		}


		/// <summary>
		/// Tests whether the current custom collation rules are valid.
		/// </summary>
		/// <param name="message">Used for an error message if rules do not validate.</param>
		/// <returns>True if rules are valid, false otherwise.</returns>
		virtual public bool ValidateCollationRules(out string message)
		{
			message = null;
			switch (SortUsing)
			{
				case SortRulesType.DefaultOrdering:
					return String.IsNullOrEmpty(SortRules);
				case SortRulesType.CustomICU:
					return IcuRulesCollator.ValidateSortRules(SortRules, out message);
				case SortRulesType.CustomSimple:
					return SimpleRulesCollator.ValidateSimpleRules(SortRules, out message);
				case SortRulesType.OtherLanguage:
					try
					{
						new SystemCollator(SortRules);
					}
					catch (Exception e)
					{
						message = String.Format("Error while validating sorting rules: {0}", e.Message);
						return false;
					}
					return true;
			}
			return false;
		}

		public override string ToString()
		{
			return _rfcTag.ToString();
		}

		virtual public WritingSystemDefinition Clone()
		{
			return new WritingSystemDefinition(this);
		}

		public bool IsUnicodeEncoded { get; set; } // TODO Introduce IsUnicodeEncoded to palaso wsd.

		public static WritingSystemDefinition FromRFC5646(string tag)
		{
			throw new NotImplementedException();
		}

		public void SortUsingOtherLanguage(string sortRules)
		{
			SortUsing = SortRulesType.OtherLanguage;
			SortRules = sortRules;
		}

		public void SortUsingCustomICU(string sortRules)
		{
			SortUsing = SortRulesType.CustomICU;
			SortRules = sortRules;
		}

		public void SortUsingCustomSimple(string sortRules)
		{
			SortUsing = SortRulesType.CustomSimple;
			SortRules = sortRules;
		}

	}

	public enum IpaStatusChoices
	{
		NotIpa,
		Ipa,
		IpaPhonetic,
		IpaPhonemic
	}

	public class WellKnownSubTags
	{
		public class Audio
		{
			public const string PrivateUseSubtag = "x-audio";
			public const string Script= "Zxxx";
		}

		public class Ipa
		{
			public const string VariantSubtag = "fonipa";
			public const string PhonemicPrivateUseSubtag = "x-emic";
			public const string PhoneticPrivateUseSubtag = "x-etic";
		}
	}
}