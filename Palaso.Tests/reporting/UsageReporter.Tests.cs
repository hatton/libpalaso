﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Palaso.Reporting;

namespace Palaso.Tests.reporting
{
	[TestFixture]
	public class UsageReporterTests
	{
		private class EnvironmentForTest : IDisposable
		{

			public EnvironmentForTest()
			{
				UsageReporter.AppNameToUseInDialogs = "PalasoUnitTest";
				UsageReporter.AppNameToUseInReporting = "PalasoUnitTest";
			}

			public void Dispose()
			{
			}
		}

//        [Test, Ignore("Run by hand")]
//        public void UsageReporterSmokeTest()
//        {
//            UsageReporter.AppNameToUseInDialogs = "PalasoUnitTest";
//            UsageReporter.AppNameToUseInReporting = "PalasoUnitTest";
//			UsageReporter.RecordLaunch();
//        }

//        [Test, Ignore("Run by hand")]
//        public void HttpPost_WithValidArgs_Ok()
//        {
//            using (var e = new EnvironmentForTest())
//            {
//                var parameters = new Dictionary<string, string>();
//                parameters.Add("app", UsageReporter.AppNameToUseInReporting);
//                parameters.Add("version", "test-0.0.0.0");
//                parameters.Add("launches", "1");
//                parameters.Add("user", "testuser");
//
//                string result = UsageReporter.HttpPost("http://www.wesay.org/usage/post.php", parameters);
//                Assert.AreEqual("OK", result);
//            }
//        }
	}
}
