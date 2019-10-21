using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    internal class NotAnImplementationOfIBugDatabaseProvider { }

    public class GivenABugDatabaseRegistry
    {
        private Mock<IBugDatabaseProvider> bugDatabaseProviderMock;
        private Mock<IWebRequest> webRequestMock;

        private BugDatabaseRegistry bugDatabaseRegistry;

        private string databaseKey;
        private IEnumerable<string> databaseArgs;

        public GivenABugDatabaseRegistry()
        {
            this.databaseKey = "Some Database Key";
            this.databaseArgs = new List<string>() { "--some", "dll", "-args" };

            this.bugDatabaseProviderMock = new Mock<IBugDatabaseProvider>();
            this.bugDatabaseProviderMock.Setup((p) => p.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(0);
            this.bugDatabaseProviderMock.Setup(p => p.Key).Returns(databaseKey);

            this.webRequestMock = new Mock<IWebRequest>();

            this.bugDatabaseRegistry = new BugDatabaseRegistry();
        }

        [Fact]
        public void WhenCouldNotFindKeyThenReturnThrowException()
        {
            Action action = () => this.bugDatabaseRegistry.Retrive(this.databaseKey, this.databaseArgs, this.webRequestMock.Object);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal($"No Bug Database registered with key 'Some Database Key'", exception.Message);
            this.bugDatabaseProviderMock.Verify(p => p.ProcessArgs(this.databaseArgs), Times.Never);
        }

        [Fact]
        public void WhenFaildToProcessArgsThenThrowException()
        {
            this.bugDatabaseRegistry.AddBugDatabase(bugDatabaseProviderMock.Object);

            this.bugDatabaseProviderMock.Setup((p) => p.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(1);

            Action action = () => this.bugDatabaseRegistry.Retrive(this.databaseKey, this.databaseArgs, this.webRequestMock.Object);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal("Unable to parse Bug Databse arguments. Check requirements", exception.Message);
            this.bugDatabaseProviderMock.Verify(p => p.ProcessArgs(this.databaseArgs), Times.Once);
        }

        [Fact]
        public void WhenValidKeyThenReturnBugDatabaseProvider()
        {
            this.bugDatabaseRegistry.AddBugDatabase(bugDatabaseProviderMock.Object);

            var provider = this.bugDatabaseRegistry.Retrive(this.databaseKey, this.databaseArgs, this.webRequestMock.Object);

            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IBugDatabaseProvider>(provider);
            this.bugDatabaseProviderMock.Verify(p => p.ProcessArgs(this.databaseArgs), Times.Once);
        }

        [Fact]
        public void WhenValidKeyButDiffrentCaseThenReturnBugDatabaseProvider()
        {
            this.bugDatabaseRegistry.AddBugDatabase(bugDatabaseProviderMock.Object);

            var provider = this.bugDatabaseRegistry.Retrive("SoMe DaTabAsE KEY", this.databaseArgs, this.webRequestMock.Object);

            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IBugDatabaseProvider>(provider);
            this.bugDatabaseProviderMock.Verify(p => p.ProcessArgs(this.databaseArgs), Times.Once);
        }
    }
}
