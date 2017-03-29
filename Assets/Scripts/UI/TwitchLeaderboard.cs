using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwitchLeaderboard : MonoBehaviour
{
    [Header("Prefabs")]
    public TwitchLeaderboardRow[] specialRows = null;
    public TwitchLeaderboardRow normalRow = null;

    [Header("Text Elements")]
    public Text totalBombCountText = null;
    public Text totalSolveCountText = null;
    public Text totalStrikeCountText = null;
    public Text totalSuccessText = null;

    [Header("Hierachy Management")]
    public RectTransform tableTransform = null;

    [Header("Values")]
    public Leaderboard leaderboard = null;
    public int bombCount = 0;
    public int maximumRowCount = 15;

    private List<TwitchLeaderboardRow> _instancedRows = new List<TwitchLeaderboardRow>();

    private void Start()
    {
        if (leaderboard == null)
        {
            return;
        }

        float delay = 0.6f;
        int index = 0;

        foreach (Leaderboard.LeaderboardEntry entry in leaderboard.GetSortedEntries(maximumRowCount))
        {
            TwitchLeaderboardRow row = Instantiate<TwitchLeaderboardRow>(index < specialRows.Length ? specialRows[index] : normalRow);
            row.position = index + 1;
            row.leaderboardEntry = entry;
            row.delay = delay;
            row.transform.SetParent(tableTransform, false);

            _instancedRows.Add(row);

            delay += 0.1f;
            index++;
        }

        int solveCount = 0;
        int strikeCount = 0;
        leaderboard.GetTotalSolveStrikeCounts(out solveCount, out strikeCount);
        float totalPercentage = (solveCount * 100.0f) / (solveCount + strikeCount);

        totalBombCountText.text = bombCount.ToString();
        totalSolveCountText.text = solveCount.ToString();
        totalStrikeCountText.text = strikeCount.ToString();
        totalSuccessText.text = Mathf.RoundToInt(totalPercentage).ToString();
    }

    private void OnDisable()
    {
        foreach(TwitchLeaderboardRow row in _instancedRows)
        {
            DestroyObject(row);
        }

        _instancedRows.Clear();
    }
}
