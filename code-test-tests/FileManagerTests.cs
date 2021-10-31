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
            else
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

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
            string filePath = Path.Combine(_testPath, "rand?\\/!!\",.com");

            // assert
            Assert.Throws<Exception>(() => fileMgr.Save(filePath, new byte[10]), "Invalid file path, expected an error");
        }

    }
}
