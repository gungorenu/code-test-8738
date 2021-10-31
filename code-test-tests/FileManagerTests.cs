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
        public void SaveFileTest_InvalidFilePath_ThrowsException()
        {
            // arrange
            IFileManager fileMgr = new FileManager();

            // act
            // assert
            string filePath = Path.Combine(_testPath, "rand?\\/!!\",.com");
            Assert.Throws<IOException>(() => fileMgr.Save(filePath, new byte[10]), "Invalid file path, expected an error");
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
            Assert.AreEqual("my fancy 10 message", data);
        }


    }
}
