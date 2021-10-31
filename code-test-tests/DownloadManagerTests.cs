using CodeTest;
using Moq;
using NUnit.Framework;

namespace CodeTest_Tests
{
    // SPECIAL NOTE: more complex tests can be added but I have time constraints so I skip some tests

    public class DownloadManagerTests
    {
        [Test]
        public void NullChecksOnConstructor()
        {
            // skipping for time constraints
        }

        [Test]
        public void PopNewFile_EmptyList()
        {
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            Mock<IWebClient> webClient= new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            // arrange
            DownloadManager downloadMgr = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);

            // act
            string file = downloadMgr.PopNewFileFromQueue();

            // assert
            Assert.IsNull(file, "Empty download manager gave us something");
        }


        [Test]
        public void AddNewFileToQueue_SingleAdd_Pop_MustBeSame()
        {
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            // arrange
            DownloadManager downloadMgr = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);

            // act
            downloadMgr.AddNewFileToQueue("file1");
            string file = downloadMgr.PopNewFileFromQueue();

            // assert
            Assert.AreEqual("file1", file, "Popped file is different than what we pushed");
            Assert.AreEqual(0, ((IDownloadManagerTesting)downloadMgr).ToBeDownloadedFiles.Count, "List must be empty");
        }

        [Test]
        public void AddNewFileToQueue_DoubleAdd_UniqueFilesOnly()
        {
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            // arrange
            DownloadManager downloadMgr = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);

            // act
            downloadMgr.AddNewFileToQueue("file1");
            downloadMgr.AddNewFileToQueue("FILE1");

            // assert
            Assert.AreEqual(1, ((IDownloadManagerTesting)downloadMgr).ToBeDownloadedFiles.Count, "List must have 1 file only");
        }


        [Test]
        public void DownloadedFiles_CannotBeAddedAgain()
        {
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            // arrange
            DownloadManager downloadMgr = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);
            ((IDownloadManagerTesting)downloadMgr).DownloadedFiles.Add("file1");

            // act
            downloadMgr.AddNewFileToQueue("file1");

            // assert
            Assert.AreEqual(0, ((IDownloadManagerTesting)downloadMgr).ToBeDownloadedFiles.Count, "List must be empty because we already downloaded the file");
        }

    }
}
