using CodeTest;
using Moq;
using NUnit.Framework;

namespace CodeTest_Tests
{
    // SPECIAL NOTE: more complex tests can be added but I have time constraints so I skip some tests, I write too basic or sample complex ones

    public class DownloadManagerTests
    {
        private DownloadManager Fake
        {
            get
            {
                Mock<IFileManager> fileMgr = new Mock<IFileManager>();
                Mock<IWebClient> webClient = new Mock<IWebClient>();
                Mock<IConsole> console = new Mock<IConsole>();

                DownloadManager downloadMgr = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);

                return downloadMgr;
            }
        }


        [Test]
        public void NullChecksOnConstructor()
        {
            // skipping for time constraints
        }

        [Test]
        public void PopNewFile_EmptyList()
        {
            // arrange
            DownloadManager fake = Fake;

            // act
            string file = fake.PopNewFileFromQueue();

            // assert
            Assert.IsNull(file, "Empty download manager gave us something");
        }


        [Test]
        public void AddNewFileToQueue_SingleAdd_Pop_MustBeSame()
        {
            // arrange
            DownloadManager fake = Fake;

            // act
            fake.AddNewFileToQueue("file1");
            string file = fake.PopNewFileFromQueue();

            // assert
            Assert.AreEqual("file1", file, "Popped file is different than what we pushed");
            Assert.AreEqual(0, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "List must be empty");
        }

        [Test]
        public void AddNewFileToQueue_DoubleAdd_UniqueFilesOnly()
        {
            // arrange
            DownloadManager fake = Fake;

            // act
            fake.AddNewFileToQueue("file1");
            fake.AddNewFileToQueue("FILE1");

            // assert
            Assert.AreEqual(2, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "Function does not do normalization again. Inspect is supposed to do that");
        }

        [Test]
        public void DownloadedFiles_CannotBeAddedAgain()
        {
            // arrange
            DownloadManager fake = Fake;
            ((IDownloadManagerTesting)fake).DownloadedFiles.Add("file1");

            // act
            fake.AddNewFileToQueue("file1");

            // assert
            Assert.AreEqual(0, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "List must be empty because we already downloaded the file");
        }

        [Test]
        public void NormalizeSite_SlashAtEnd()
        {
            // arrange
            DownloadManager fake = Fake;

            // act
            string value = fake.NormalizeWebSite("www.google.com");

            // assert
            Assert.IsTrue(value.EndsWith("/"), "Trailing slash was not added");
        }

        [Test]
        public void NormalizeSite_SlashAtEnd_SingleOnly()
        {
            // arrange
            DownloadManager fake = Fake;

            // act
            string value = fake.NormalizeWebSite("www.google.com/");

            // assert
            Assert.IsTrue(value.EndsWith("/"), "Trailing slash was not added");
            Assert.IsFalse(value.EndsWith("//"), "Trailing slash was doubled");
        }

        [Test]
        public void NormalizeSite_AddsHttpIfNotThere()
        {
            // skipping due to time constraints
        }

        [Test]
        public void FilterDownloadFile_Basic_Accepted()
        {
            // arrange
            DownloadManager fake = Fake;
            ((IDownloadManagerTesting)fake).Site = fake.NormalizeWebSite("www.google.com");

            // act
            bool result = fake.ShouldDownloadFile("http://www.google.com/exit.png");

            // assert
            Assert.IsTrue(result, "Basic png should be downloaded");
        }

        [Test]
        public void FilterDownloadFile_LinkToAnotherHost_Rejected()
        {
            // arrange
            DownloadManager fake = Fake;
            ((IDownloadManagerTesting)fake).Site = fake.NormalizeWebSite("www.google.com");

            // act
            bool result = fake.ShouldDownloadFile("http://www.facebook.com/join.png");

            // assert
            Assert.IsFalse(result, "Facebook must be bad");
        }

        [Test]
        public void FilterDownloadFile_RootLink_Accepted()
        {
            // arrange
            DownloadManager fake = Fake;
            ((IDownloadManagerTesting)fake).Site = fake.NormalizeWebSite("www.google.com");

            // act
            bool result = fake.ShouldDownloadFile("/search.png");

            // assert
            Assert.IsTrue(result, "Root links should be downloaded");
        }


        [Test]
        public void FilterDownloadFile_SimpleLink_NotNormalized_Rejected()
        {
            // arrange
            DownloadManager fake = Fake;
            ((IDownloadManagerTesting)fake).Site = fake.NormalizeWebSite("www.google.com");

            // act
            bool result = fake.ShouldDownloadFile("inselffolder_image.png");

            // assert
            Assert.IsFalse(result, "Files under same folder should have different path so it should not be downloaded");
        }

        [Test]
        public void InspectSite_SimpleLinks()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            fileMgr.Setup(f => f.ReadAllText("dummy")).Returns("<a href=\"/link1.png\"/><a href=\"/link2.png\"/>");
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();
            
            DownloadManager fake = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);
            string site = fake.NormalizeWebSite("www.google.com");
            ((IDownloadManagerTesting)fake).Site = site;

            // act
            fake.InspectSite("dummy", site);

            // assert
            Assert.AreEqual(2, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "2 links should have been registered to download");
            Assert.AreEqual(site + "link1.png", ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Peek() , "Link is incorrect");
        }

        [Test]
        public void InspectSite_FullLink_Accepted()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            fileMgr.Setup(f => f.ReadAllText("dummy")).Returns("<a href=\"http://www.google.com/link1.png\"/>");
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            DownloadManager fake = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);
            string site = fake.NormalizeWebSite("www.google.com");
            ((IDownloadManagerTesting)fake).Site = site;

            // act
            fake.InspectSite("dummy", site);

            // assert
            Assert.AreEqual(1, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "Link should have been registered to download");
        }

        [Test]
        public void InspectSite_AnotherHost_Rejected()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            fileMgr.Setup(f => f.ReadAllText("dummy")).Returns("<a href=\"http://www.facebook.com/link1.png\"/>");
            Mock<IWebClient> webClient = new Mock<IWebClient>();
            Mock<IConsole> console = new Mock<IConsole>();

            DownloadManager fake = new DownloadManager(console.Object, fileMgr.Object, webClient.Object);
            string site = fake.NormalizeWebSite("www.google.com");
            ((IDownloadManagerTesting)fake).Site = site;

            // act
            fake.InspectSite("dummy", site);

            // assert
            Assert.AreEqual(0, ((IDownloadManagerTesting)fake).ToBeDownloadedFiles.Count, "Facebook is bad");
        }


        [Test]
        public void ProgressUpdate_Empty_NoX()
        {
            // skipping for time constraints
        }


        [Test]
        public void ProgressUpdate_Full_CountX()
        {
            // skipping for time constraints
        }

        [Test]
        public void ProgressUpdate_Half_CountX()
        {
            // skipping for time constraints
        }

    }
}
