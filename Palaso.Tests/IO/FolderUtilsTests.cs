﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class FolderUtilsTests
	{
		private string _srcFolder;
		private string _dstFolder;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			Reporting.ErrorReport.IsOkToInteractWithUser = false;
			_srcFolder = Path.Combine(Path.GetTempPath(), "~!source");
			_dstFolder = Path.Combine(Path.GetTempPath(), "~!destination");
			Directory.CreateDirectory(_srcFolder);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TestTearDown()
		{
			try
			{
				if (Directory.Exists(_srcFolder))
					Directory.Delete(_srcFolder, true);
			}
			catch { }

			try
			{
				if (Directory.Exists(_dstFolder))
					Directory.Delete(_dstFolder, true);
			}
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceDoesNotExist_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(FolderUtils.CopyFolderContents("sblah", "dblah"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_DestinationFolderDoesNotExist_CreatesItAndReturnsTrue()
		{
			Assert.IsFalse(Directory.Exists(_dstFolder));
			Assert.IsTrue(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(_dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsFilesNoSubFolders_CopiesThemAndSucceeds()
		{
			File.CreateText(Path.Combine(_srcFolder, "file1.txt")).Close();
			File.CreateText(Path.Combine(_srcFolder, "file2.txt")).Close();
			Assert.IsTrue(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file1.txt")));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file2.txt")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsLockedFile_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_CopyFails_DestinationFolderNotLeftBehind()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));
				Assert.IsFalse(Directory.Exists(_dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsEmptySubFolders_CopiesThem()
		{
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder1"));
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder2"));
			Assert.IsTrue(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder1")));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder2")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsSubFolderWithFiles_CopiesThem()
		{
			var subfolder = Path.Combine(_srcFolder, "subfolder");
			Directory.CreateDirectory(subfolder);
			File.CreateText(Path.Combine(subfolder, "file1.txt")).Close();
			Assert.IsTrue(FolderUtils.CopyFolderContents(_srcFolder, _dstFolder));

			subfolder = Path.Combine(_dstFolder, "subfolder");
			Assert.IsTrue(File.Exists(Path.Combine(subfolder, "file1.txt")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolderToTempFolder_SourceFolderExists_MakesCopyInTempFolder()
		{
			Assert.IsTrue(FolderUtils.CopyFolderToTempFolder(_srcFolder));
			var foldername = Path.GetFileName(_srcFolder);
			Assert.IsTrue(Directory.Exists(Path.Combine(Path.GetTempPath(), foldername)));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_DestinationFolderDoesNotExist_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(FolderUtils.CopyFolder(_srcFolder, _dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceFolderExists_MakesCopyOfFolderWithSameName()
		{
			Directory.CreateDirectory(_dstFolder);
			Assert.IsTrue(FolderUtils.CopyFolder(_srcFolder, _dstFolder));
			var foldername = Path.GetFileName(_srcFolder);
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, foldername)));
		}
	}
}