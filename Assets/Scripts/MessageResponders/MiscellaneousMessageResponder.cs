using System;
using UnityEngine;

public class MiscellaneousMessageResponder : MessageResponder
{
    public Leaderboard leaderboard = null;

    protected override void OnMessageReceived(string userNickName, string userColorCode, string text)
    {
        if (text.Equals("!cancel", StringComparison.InvariantCultureIgnoreCase))
        {
            _coroutineCanceller.SetCancel();
            return;
        }
        else if (text.Equals("!stop", StringComparison.InvariantCultureIgnoreCase))
        {
            _coroutineCanceller.SetCancel();
            _coroutineQueue.CancelFutureSubcoroutines();
            return;
        }
        else if (text.Equals("!manual", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Go to http://www.bombmanual.com to get the vanilla manual for KTaNE.");
            return;
        }
        else if (text.Equals("!help", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Go to http://www.twitchplaysktane.me/Manual to get the command reference for TP:KTaNE.");
            return;
        }
        else if (text.Equals("!rank", StringComparison.InvariantCultureIgnoreCase))
        {
            Leaderboard.LeaderboardEntry entry = null;
            int rank = leaderboard.GetRank(userNickName, out entry);
            if (entry != null)
            {
                _ircConnection.SendMessage(string.Format("{0} is #{1} with {2} solves and {3} strikes (success rate of {4:0.00})", userNickName, rank, entry.SolveCount, entry.StrikeCount, entry.SolveRate));
            }
            else
            {
                _ircConnection.SendMessage(string.Format("{0}, you don't have any solves or strikes yet!", userNickName));
            }
        }
    }
}
