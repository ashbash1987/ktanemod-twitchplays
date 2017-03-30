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

        public float SolveRate
        {
            get
            {
                if (StrikeCount == 0)
                {
                    return SolveCount;
                }

                return ((float)SolveCount) / StrikeCount;
            }
        }
    }

    public void AddSolve(string userName, Color userColor)
    {
        LeaderboardEntry entry = null;

        if (!_entryDictionary.ContainsKey(userName))
        {
            entry = new LeaderboardEntry() { UserName = userName, UserColor = userColor };
            _entryDictionary[userName] = entry;
            _entryList.Add(entry);
        }
        else
        {
            entry = _entryDictionary[userName];
        }

        entry.AddSolve();

        ResetSortFlag();
    }

    public void AddStrike(string userName, Color userColor)
    {
        LeaderboardEntry entry = null;

        if (!_entryDictionary.ContainsKey(userName))
        {
            entry = new LeaderboardEntry() { UserName = userName, UserColor = userColor };
            _entryDictionary[userName] = entry;
            _entryList.Add(entry);
        }
        else
        {
            entry = _entryDictionary[userName];
        }

        entry.AddStrike();

        ResetSortFlag();
    }

    public IEnumerable<LeaderboardEntry> GetSortedEntries(int count)
    {
        CheckAndSort();
        return _entryList.Take(count);
    }

    public int GetRank(string userName, out LeaderboardEntry entry)
    {
        if (!_entryDictionary.ContainsKey(userName))
        {
            entry = null;
            return _entryList.Count + 1;
        }

        CheckAndSort();
        entry = _entryDictionary[userName];
        return _entryList.IndexOf(entry) + 1;
    }

    public void GetTotalSolveStrikeCounts(out int solveCount, out int strikeCount)
    {
        solveCount = 0;
        strikeCount = 0;

        foreach (LeaderboardEntry entry in _entryList)
        {
            solveCount += entry.SolveCount;
            strikeCount += entry.StrikeCount;
        }
    }

    private void ResetSortFlag()
    {
        _sorted = false;
    }

    private void CheckAndSort()
    {
        if (!_sorted)
        {
            _entryList.Sort(CompareScores);
            _sorted = true;
        }
    }

    private static int CompareScores(LeaderboardEntry lhs, LeaderboardEntry rhs)
    {
        if (lhs.SolveCount != rhs.SolveCount)
        {
            //Intentially reversed comparison to sort from highest to lowest
            return rhs.SolveCount.CompareTo(lhs.SolveCount);
        }

        //Intentially reversed comparison to sort from highest to lowest
        return rhs.SolveRate.CompareTo(lhs.SolveRate);
    }

    private Dictionary<string, LeaderboardEntry> _entryDictionary = new Dictionary<string, LeaderboardEntry>();
    private List<LeaderboardEntry> _entryList = new List<LeaderboardEntry>();
    private bool _sorted = true;
}
