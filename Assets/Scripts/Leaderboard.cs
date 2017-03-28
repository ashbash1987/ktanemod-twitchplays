using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leaderboard
{
    public class LeaderboardEntry
    {
        public void AddSolve()
        {
            SolveCount++;
        }

        public void AddStrike()
        {
            StrikeCount++;
        }

        public string UserName
        {
            get;
            set;
        }

        public Color UserColor
        {
            get;
            set;
        }

        public int SolveCount
        {
            get;
            private set;
        }

        public int StrikeCount
        {
            get;
            private set;
        }

        public float Percent
        {
            get
            {
                if (SolveCount == 0 && StrikeCount == 0)
                {
                    return float.NaN;
                }

                return (SolveCount * 100.0f) / (SolveCount + StrikeCount);
            }
        }

        public float Score
        {
            get
            {
                if (SolveCount == 0 && StrikeCount == 0)
                {
                    return 0;
                }

                return SolveCount + (SolveCount / (SolveCount + StrikeCount));
            }
        }
    }

    public void AddSolve(string userName, Color userColor)
    {
        LeaderboardEntry entry = null;

        if (!_entries.ContainsKey(userName))
        {
            entry = new LeaderboardEntry() { UserName = userName, UserColor = userColor };
            _entries[userName] = entry;
        }
        else
        {
            entry = _entries[userName];
        }

        entry.AddSolve();
    }

    public void AddStrike(string userName, Color userColor)
    {
        LeaderboardEntry entry = null;

        if (!_entries.ContainsKey(userName))
        {
            entry = new LeaderboardEntry() { UserName = userName, UserColor = userColor };
            _entries[userName] = entry;
        }
        else
        {
            entry = _entries[userName];
        }

        entry.AddStrike();
    }

    public IEnumerable<LeaderboardEntry> GetSortedEntries(int count)
    {
        return _entries.Values.OrderByDescending((x) => x.Score).Take(count);
    }

    public void GetTotalSolveStrikeCounts(out int solveCount, out int strikeCount)
    {
        solveCount = 0;
        strikeCount = 0;

        foreach (LeaderboardEntry entry in _entries.Values)
        {
            solveCount += entry.SolveCount;
            strikeCount += entry.StrikeCount;
        }
    }

    private Dictionary<string, LeaderboardEntry> _entries = new Dictionary<string, LeaderboardEntry>();
}

