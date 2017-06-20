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
            _ircConnection.SendMessage("Go to http://www.bombmanual.com to get the vanilla manual for KTaNE. A full set of manuals is at https://ktane.timwi.de/");
            return;
        }
        else if (text.Equals("!help", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Go to http://bombch.us/CeEz to get the command reference for TP:KTaNE.");
            return;
        }
        else if (text.Equals("!rank", StringComparison.InvariantCultureIgnoreCase))
        {
            Leaderboard.LeaderboardEntry entry = null;
            int rank = leaderboard.GetRank(userNickName, out entry);
            if (entry != null)
            {
                string txtSolver = "";
                string txtSolo = ".";
                if (entry.TotalSoloClears > 0)
                {
                    TimeSpan recordTimeSpan = TimeSpan.FromSeconds(entry.RecordSoloTime);
                    txtSolver = "solver ";
                    txtSolo = string.Format(", and #{0} solo with a best time of {1}:{2:00.0}", entry.SoloRank, (int)recordTimeSpan.TotalMinutes, recordTimeSpan.Seconds);
                }
                _ircConnection.SendMessage(string.Format("SeemsGood {0} is #{1} {4}with {2} solves and {3} strikes{5}", userNickName, rank, entry.SolveCount, entry.StrikeCount, txtSolver, txtSolo));
            }
            else
            {
                _ircConnection.SendMessage(string.Format("FailFish {0}, do you even play this game?", userNickName));
            }
            return;
        }
        else if ( (text.Equals("!log", StringComparison.InvariantCultureIgnoreCase)) ||
            (text.Equals("!analysis", StringComparison.InvariantCultureIgnoreCase)) )
        {
            TwitchPlaysService.logUploader.PostToChat("Analysis for the previous bomb: {0}");
            return;
        }
        }
        else if (text.Equals("!mouse", StringComparison.InvariantCultureIgnoreCase))
        {
            InputInterceptor.EnableInput();
        }
    }
}
