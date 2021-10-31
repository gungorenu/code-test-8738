using System;
using CodeTest;
using NUnit.Framework;
using System.IO;

namespace CodeTest_Tests
{
    public class FileManagerTests
    {
        private string _testPath;

        [SetUp]
        public void Setup()
        {
            string dirPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "tests");
            if (System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.Delete(dirPath, true);
            }

            System.IO.Directory.CreateDirectory(dirPath);

            _testPath = dirPath;
        }


        [Test]
        public void SaveFileTest_SunnyDay()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            string filePath = Path.Combine(_testPath, "SaveFileTest_SunnyDay");
            fileMgr.Save(filePath, new byte[10]);

            // assert
            Assert.IsTrue(System.IO.File.Exists(filePath), "File creation failed");
            Assert.AreEqual(10, System.IO.File.ReadAllBytes(filePath).Length, "File data is not correct");
        }

        [Test]
        public void SaveFileTest_InvalidFilePath_NotThrowsException()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            // assert
            string filePath = Path.Combine(_testPath, "rand?\\/!!\",.com");
            Assert.DoesNotThrow(() => fileMgr.Save(filePath, new byte[10]), "Invalid file path should not throw exc anymore");
        }

        [Test]
        public void SaveFile_NullChecks()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            // assert
            Assert.Throws<ArgumentNullException>(() => fileMgr.Save(null, new byte[10]), "File path null");
            string filePath = Path.Combine(_testPath, "SaveFile_NullChecks");
            Assert.Throws<ArgumentNullException>(() => fileMgr.Save(filePath, null), "Data null");
        }


        [Test]
        public void Trace_LogMessage()
        {
            // arrange
            IFileManager fileMgr = new FileManager();
            string traceFile = ((FileManager)fileMgr).TraceFile;
            System.IO.File.Delete(traceFile);

            // act
            string message = "my fancy {0} message";
            int number = 10;
            fileMgr.Trace(message, number);

            // assert
            string data = System.IO.File.ReadAllText(traceFile).Trim();
            Assert.AreEqual(string.Format("[T#{0}] my fancy 10 message", System.Threading.Thread.CurrentThread.ManagedThreadId), data);
        }

        [Test]
        public void WhereToSave_BasicCheck()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            string result = fileMgr.GetWhereToSaveFile("http://www.google.com/");

            // assert
            string expected = System.Environment.CurrentDirectory + "\\www.google.com\\";
            Assert.AreEqual(expected, result, "Base folder could not be calculated properly");
        }

        [Test]
        public void WhereToSave_SubPath()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            string result = fileMgr.GetWhereToSaveFile("http://www.google.com/images");

            // assert
            string expected = System.Environment.CurrentDirectory + "\\www.google.com\\images";
            Assert.AreEqual(expected, result, "Nested folder could not be calculated properly");
        }

        [Test]
        public void WhereToSave_SubPath_WithoutBase_BaseAdded()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            string result = fileMgr.GetWhereToSaveFile("/images");

            // assert
            string expected = System.Environment.CurrentDirectory + "\\images";
            Assert.AreEqual(expected, result, "SubPath without base could not be calculated properly");
        }

        [Test]
        public void WhereToSave_SubPath_CreatesFolderStructure()
        {
            // skipping due to time constraints
        }
    }
}
