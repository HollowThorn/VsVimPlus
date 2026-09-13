namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// A small, fast, case-insensitive subsequence matcher: query "abc" matches any candidate
    /// that contains 'a', then 'b', then 'c' in order, scoring consecutive/early matches higher
    /// </summary>
    internal static class FuzzyMatcher
    {
        /// <returns>A match score (higher is better), or -1 if query isn't a subsequence of candidate</returns>
        internal static int Score(string candidate, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(candidate))
            {
                return -1;
            }

            var candidateIndex = 0;
            var score = 0;
            var consecutiveBonus = 0;

            for (var queryIndex = 0; queryIndex < query.Length; queryIndex++)
            {
                var queryChar = char.ToLowerInvariant(query[queryIndex]);
                var found = false;
                while (candidateIndex < candidate.Length)
                {
                    var candidateChar = char.ToLowerInvariant(candidate[candidateIndex]);
                    candidateIndex++;
                    if (candidateChar == queryChar)
                    {
                        found = true;
                        consecutiveBonus += 2;
                        // Scaled up by 100 so the position penalty below (bounded by candidate
                        // length) can never overwhelm it and flip a real match negative - that
                        // was the bug: matches deep in a long path (i.e. in the filename, past a
                        // long directory prefix) were scoring below zero and getting filtered
                        // out as "no match" by callers checking score >= 0, which made this look
                        // like a prefix-only matcher instead of a real fuzzy one.
                        score += (1 + consecutiveBonus) * 100;
                        break;
                    }

                    consecutiveBonus = 0;
                }

                if (!found)
                {
                    return -1;
                }
            }

            // Prefer tighter, earlier matches - a tie-break only, deliberately small relative to
            // the per-character score above
            score -= candidateIndex / 4;
            return score;
        }
    }
}
