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
            _ircConnection.SendMessage("Go to http://www.bombmanual.com to get the vanilla manual for KTaNE. Type !69 manual for a link to a particular module's manual.");
            return;
        }
        else if (text.Equals("!help", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Go to http://bombch.us/CeEz to get the command reference for TP:KTaNE. Type !69 help to see the commands for a particular module.");
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
        else if (text.Equals("!about", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Twitch Plays: KTaNE is an alternative way of playing !ktane. Unlike the original game, you play as both defuser and expert, and defuse the bomb by sending special commands to the chat room. Try !help for more information!");
            return;
        }
        else if (text.Equals("!credits", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Twitch Plays: KTaNE was developed by Ash the Bash (twitch.tv/at_bash). The project is currently maintained by Ash, bmn (twitch.tv/gogobmn) and CaitSith2 (twitch.tv/caitsith2), with the support of many mod developers. The manual repository and logfile analyser are managed by Timwi (twitch.tv/timwiterby). Keep Talking and Nobody Explodes is by Steel Crate Games.");
            return;
        }
        else if (text.Equals("!ktane", StringComparison.InvariantCultureIgnoreCase))
        {
            _ircConnection.SendMessage("Keep Talking and Nobody Explodes is developed by Steel Crate Games. It's available for Windows PC, Mac OS X, PlayStation VR, Samsung Gear VR and Google Daydream. See http://www.keeptalkinggame.com/ for more information!");
            return;
        }
    }
}
