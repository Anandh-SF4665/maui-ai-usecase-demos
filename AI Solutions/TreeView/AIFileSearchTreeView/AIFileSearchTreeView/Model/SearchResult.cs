using System;
using System.Collections.Generic;

namespace AIFileSearchTreeView
{
    /// <summary>
    /// Represents the result of an AI-powered semantic search
    /// </summary>
    public class SearchResult
    {
        public List<MatchedNode> Matches { get; set; } = new List<MatchedNode>();
        public int TotalMatches => Matches.Count;
        public DateTime SearchExecutedAt { get; set; }
        public double ExecutionTimeMs { get; set; }
        public string SearchQuery { get; set; }

        public class MatchedNode
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public double ConfidenceScore { get; set; } = 1.0;
            public string[] MatchReasons { get; set; } = Array.Empty<string>();
            public List<string> ParentsToExpand { get; set; } = new List<string>();
        }
    }
}
