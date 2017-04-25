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

        public void AddStrike(int num)
        {
            StrikeCount += num;
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

    private bool GetEntry(string UserName, out LeaderboardEntry entry)
    {
        return _entryDictionary.TryGetValue(UserName.ToLowerInvariant(), out entry);
    }

    private LeaderboardEntry GetEntry(string userName, Color userColor)
    {
        LeaderboardEntry entry = null;
        if (!GetEntry(userName, out entry))
        {
            entry = new LeaderboardEntry();
            _entryDictionary[userName.ToLowerInvariant()] = entry;
            _entryList.Add(entry);
        }
        entry.UserName = userName;
        entry.UserColor = userColor;
        return entry;
    }

    public void AddSolve(string userName, Color userColor)
    {
        LeaderboardEntry entry = GetEntry(userName, userColor);

        entry.AddSolve();
        ResetSortFlag();
    }

    public void AddStrike(string userName, Color userColor, int numStrikes)
    {
        LeaderboardEntry entry = GetEntry(userName, userColor);

        entry.AddStrike(numStrikes);
        ResetSortFlag();
    }

    public IEnumerable<LeaderboardEntry> GetSortedEntries(int count)
    {
        CheckAndSort();
        return _entryList.Take(count);
    }

    public int GetRank(string userName, out LeaderboardEntry entry)
    {
        if (!GetEntry(userName, out entry))
        {
            return _entryList.Count + 1;
        }

        CheckAndSort();
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
