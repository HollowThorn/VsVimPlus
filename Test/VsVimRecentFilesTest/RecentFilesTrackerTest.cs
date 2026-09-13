using Microsoft.VisualStudio.Shell;
using Moq;
using Xunit;

namespace Vim.VisualStudio.RecentFiles.UnitTest
{
    public abstract class RecentFilesTrackerTest
    {
        private readonly IRecentFilesService _recentFilesService;

        protected RecentFilesTrackerTest()
        {
            var serviceProvider = new Mock<SVsServiceProvider>(MockBehavior.Strict);
            _recentFilesService = new RecentFilesTracker(serviceProvider.Object);
        }

        public sealed class GetRecentFilesTest : RecentFilesTrackerTest
        {
            [Fact]
            public void EmptyByDefault()
            {
                Assert.Empty(_recentFilesService.GetRecentFiles());
            }

            [Fact]
            public void MostRecentlyActivatedFileIsFirst()
            {
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                _recentFilesService.OnFileActivated(@"c:\b.txt");
                _recentFilesService.OnFileActivated(@"c:\c.txt");
                Assert.Equal(
                    new[] { @"c:\c.txt", @"c:\b.txt", @"c:\a.txt" },
                    _recentFilesService.GetRecentFiles());
            }

            [Fact]
            public void ReactivatingAFileMovesItToTheFront()
            {
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                _recentFilesService.OnFileActivated(@"c:\b.txt");
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                Assert.Equal(
                    new[] { @"c:\a.txt", @"c:\b.txt" },
                    _recentFilesService.GetRecentFiles());
            }

            [Fact]
            public void ReactivatingAFileDoesNotDuplicateIt()
            {
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                Assert.Single(_recentFilesService.GetRecentFiles());
            }

            [Fact]
            public void ComparisonIsCaseInsensitive()
            {
                _recentFilesService.OnFileActivated(@"c:\a.txt");
                _recentFilesService.OnFileActivated(@"C:\A.TXT");
                Assert.Single(_recentFilesService.GetRecentFiles());
                Assert.Equal(@"C:\A.TXT", _recentFilesService.GetRecentFiles()[0]);
            }

            [Fact]
            public void IsCappedAtMaxCount()
            {
                for (var i = 0; i < RecentFilesTracker.MaxCount + 10; i++)
                {
                    _recentFilesService.OnFileActivated($@"c:\{i}.txt");
                }

                var recentFiles = _recentFilesService.GetRecentFiles();
                Assert.Equal(RecentFilesTracker.MaxCount, recentFiles.Count);

                // The most recently activated files are the ones that should survive
                Assert.Equal($@"c:\{RecentFilesTracker.MaxCount + 9}.txt", recentFiles[0]);
            }

            [Fact]
            public void NullOrEmptyPathIsIgnored()
            {
                _recentFilesService.OnFileActivated(null);
                _recentFilesService.OnFileActivated(string.Empty);
                Assert.Empty(_recentFilesService.GetRecentFiles());
            }
        }
    }
}
