using Xunit;

namespace Vim.VisualStudio.RecentFiles.UnitTest
{
    public abstract class FuzzyMatcherTest
    {
        public sealed class ScoreTest : FuzzyMatcherTest
        {
            [Fact]
            public void EmptyQueryMatchesAnything()
            {
                Assert.Equal(0, FuzzyMatcher.Score(@"c:\foo\bar.txt", string.Empty));
            }

            [Fact]
            public void EmptyCandidateNeverMatchesANonEmptyQuery()
            {
                Assert.Equal(-1, FuzzyMatcher.Score(string.Empty, "a"));
            }

            [Fact]
            public void QueryCharactersMustAppearInOrder()
            {
                Assert.Equal(-1, FuzzyMatcher.Score("abc", "ba"));
            }

            [Fact]
            public void OutOfOrderSubsequenceDoesNotMatch()
            {
                Assert.Equal(-1, FuzzyMatcher.Score(@"c:\bar\foo.txt", "foobar"));
            }

            [Fact]
            public void NonContiguousSubsequenceStillMatches()
            {
                Assert.True(FuzzyMatcher.Score(@"c:\foo\bar.txt", "fbt") >= 0);
            }

            [Fact]
            public void MatchIsCaseInsensitive()
            {
                Assert.True(FuzzyMatcher.Score(@"c:\Foo\Bar.txt", "foobar") >= 0);
            }

            [Fact]
            public void ContiguousMatchScoresHigherThanScattered()
            {
                var contiguous = FuzzyMatcher.Score(@"c:\src\program.cs", "program");
                var scattered = FuzzyMatcher.Score(@"c:\src\pxroxgxrxaxm.cs", "program");
                Assert.True(contiguous > scattered);
            }

            [Fact]
            public void ShorterCandidateScoresHigherThanLongerForSameQuery()
            {
                var shorter = FuzzyMatcher.Score(@"c:\a\program.cs", "program");
                var longer = FuzzyMatcher.Score(@"c:\a\b\c\d\e\program.cs", "program");
                Assert.True(shorter > longer);
            }

            /// <summary>
            /// Regression test: a match occurring deep in a long path (i.e. in the filename,
            /// after a long directory prefix) must still score as a match (>= 0). The position
            /// tie-break used to be large enough to push these negative, which made callers that
            /// filter on "score >= 0" silently drop real matches - looking like a prefix-only
            /// matcher instead of a real fuzzy one.
            /// </summary>
            [Fact]
            public void MatchDeepInALongPathIsStillFound()
            {
                var longPath = @"c:\users\example\documents\code\some-project\src\deeply\nested\namespace\folder\program.cs";
                Assert.True(FuzzyMatcher.Score(longPath, "prog") >= 0);
                Assert.True(FuzzyMatcher.Score(longPath, "pcs") >= 0);
            }
        }
    }
}
