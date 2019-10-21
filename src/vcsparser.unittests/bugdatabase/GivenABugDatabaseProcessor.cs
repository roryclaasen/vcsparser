using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenABugDatabaseProcessor
    {
        private Mock<IBugDatabaseProvider> bugDatabaseProviderMock;
        private Mock<IBugDatabaseRegistry> bugDatabaseRegistryMock;
        private Mock<IWebRequest> webRequest;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IJsonListParser<WorkItem>> workItemParser;
        private Mock<ILogger> loggerMock;

        private BugDatabaseProcessor bugDatabaseProcessor;
        private Mock<IChangesetProcessor> changesetProcessorMock;

        private string someDatabaseKey;
        private IEnumerable<string> someDatabaseArgs;

        public GivenABugDatabaseProcessor()
        {
            this.someDatabaseKey = "Some Database Key";
            this.someDatabaseArgs = new List<string>() { "--some", "dll", "-arguments" };

            this.bugDatabaseProviderMock = new Mock<IBugDatabaseProvider>();
            this.bugDatabaseProviderMock.Setup(b => b.ProcessArgs(someDatabaseArgs)).Returns(0);
            this.bugDatabaseProviderMock.Setup(b => b.Process()).Returns(new Dictionary<DateTime, Dictionary<string, WorkItem>>()
            {
                    {
                        new DateTime(2019, 04, 11),
                        new Dictionary<string, WorkItem>()
                        {
                            {
                                "SomeChangeSetId",
                                new WorkItem
                                {
                                    ChangesetId = "SomeChangeSetId",
                                    ClosedDate =  new DateTime(2019, 04, 11),
                                    WorkItemId = "1"
                                }
                            }
                        }
                    }
            });

            this.webRequest = new Mock<IWebRequest>();

            this.bugDatabaseRegistryMock = new Mock<IBugDatabaseRegistry>();
            this.bugDatabaseRegistryMock
                .Setup(b => b.Retrive(someDatabaseKey, someDatabaseArgs, webRequest.Object))
                .Returns(bugDatabaseProviderMock.Object);


            this.fileSystemMock = new Mock<IFileSystem>();

            this.workItemParser = new Mock<IJsonListParser<WorkItem>>();
            this.workItemParser.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<WorkItem>()
            {
                new WorkItem
                {
                    ChangesetId = "SomeChangeSetId",
                    ClosedDate =  new DateTime(2019, 04, 11),
                    WorkItemId = "1"
                }
            });

            this.loggerMock = new Mock<ILogger>();

            this.changesetProcessorMock = new Mock<IChangesetProcessor>();
            this.changesetProcessorMock.Setup(c => c.WorkItemCache).Returns(new Dictionary<string, List<WorkItem>>());

            this.bugDatabaseProcessor = new BugDatabaseProcessor(this.bugDatabaseRegistryMock.Object, this.webRequest.Object, this.fileSystemMock.Object, this.workItemParser.Object, this.loggerMock.Object);
        }

        [Fact]
        public void WhenProcessBugDatabaseShouldProcess()
        {
            this.bugDatabaseProcessor.ProcessBugDatabase(someDatabaseKey, someDatabaseArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheChangesetProviderNullShouldExit()
        {
            this.bugDatabaseProcessor.ProcessCache("", null);

            this.changesetProcessorMock.Verify(c => c.WorkItemCache, Times.Never);
            Assert.Empty(this.changesetProcessorMock.Object.WorkItemCache);
        }

        [Fact]
        public void WhenProcessCacheCacheOutputShouldExit()
        {
            this.bugDatabaseProcessor.ProcessCache(null, this.changesetProcessorMock.Object);

            this.changesetProcessorMock.Verify(c => c.WorkItemCache, Times.Never);
            Assert.Empty(this.changesetProcessorMock.Object.WorkItemCache);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundShouldProcessEachFile()
        {
            var fileMock = new Mock<IFile>();
            fileMock
                .SetupSequence(f => f.FileName)
                .Returns("SomeFile.json").Returns("SomeFile.json")
                .Returns("SomeOtherFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object, fileMock.Object });

            this.workItemParser.SetupSequence(w => w.ParseFile(It.IsAny<string>())).Returns(new List<WorkItem>()
            {
                new WorkItem
                {
                    ChangesetId = "SomeChangeSetId1",
                    ClosedDate =  new DateTime(2019, 04, 11),
                    WorkItemId = "1"
                }
            }).Returns(new List<WorkItem>()
            {
                new WorkItem
                {
                    ChangesetId = "SomeChangeSetId2",
                    ClosedDate =  new DateTime(2019, 04, 11),
                    WorkItemId = "2"
                }
            });

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.loggerMock.Verify(l => l.LogToConsole($"Processing SomeFile.json"), Times.Once);
            this.loggerMock.Verify(l => l.LogToConsole($"Processing SomeOtherFile.json"), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheNoFilesFoundShouldNotProcessFiles()
        {
            this.fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<IFile>());

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.loggerMock.Verify(l => l.LogToConsole(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundShouldAddToChangesetProcessor()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.changesetProcessorMock.Verify(c => c.WorkItemCache, Times.Exactly(3));
            var list = Assert.Single(this.changesetProcessorMock.Object.WorkItemCache).Value;
            Assert.Single(list);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundSameChangesetShouldAddToChangesetProcessor()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            this.workItemParser.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<WorkItem>()
            {
                new WorkItem
                {
                    ChangesetId = "SameChangeSetId",
                    ClosedDate =  new DateTime(2019, 04, 11),
                    WorkItemId = "1"
                },
                new WorkItem
                {
                    ChangesetId = "SameChangeSetId",
                    ClosedDate =  new DateTime(2019, 04, 11),
                    WorkItemId = "2"
                }
            });

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            var list = Assert.Single(this.changesetProcessorMock.Object.WorkItemCache).Value;
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundNoChangesetsShouldAddToChangesetProcessor()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            this.workItemParser.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<WorkItem>());

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.changesetProcessorMock.Verify(c => c.WorkItemCache, Times.Never);
            Assert.Empty(this.changesetProcessorMock.Object.WorkItemCache);
        }
    }
}
