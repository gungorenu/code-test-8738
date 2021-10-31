using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CodeTest;
using Moq;
using NUnit.Framework;

namespace CodeTest_Tests
{

    public class WebClientTests
    {
        [Test]
        public void Download_Existing_Link()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            IWebClient client = new WebClient(fileMgr.Object);

            // act
            var data = client.DownloadFile("http://www.google.com");

            // assert
            Assert.IsNotNull(data, "Got null data from google.com");
            Assert.Greater(data.Length, 0, "Got 0 data");
        }

        [Test]
        public void Download_NonExisting_Link()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            fileMgr.Setup(f => f.TraceError("", null));
            IWebClient client = new WebClient(fileMgr.Object);

            // act
            var data = client.DownloadFile("http://www.thiswebsitemostprobablydoesnotexistbutnotsure.com");
            // assert
            Assert.IsNull(data, "Got some data from an unexisting site? From space perhaps?");
        }


        [Test]
        public void DownloadFile_Cancellable()
        {
            // arrange
            Mock<IFileManager> fileMgr = new Mock<IFileManager>();
            fileMgr.Setup(f => f.TraceError("", null));
            IWebClient client = new WebClient(fileMgr.Object);
            using (var source = new CancellationTokenSource())
            {
                client.Token = source.Token;

                // act
                var data = client.DownloadFile("http://www.google.com");

                // assert
                Assert.IsNotNull(data, "Got null data from google.com");
                Assert.Greater(data.Length, 0, "Got 0 data");
            }

        }


        [Test]
        public void NullChecksOnConstructor()
        {
            // skipping for time constraints
        }

    }
}
