using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogUploader : MonoBehaviour
{
    public string log { get; private set; }
    public IRCConnection ircConnection = null;

    [HideInInspector]
    public string analysisUrl = null;

    [HideInInspector]
    public bool postOnComplete = false;

    [HideInInspector]
    public string LOGPREFIX;

    private string output;

    public void OnEnable()
    {
        LOGPREFIX = "[" + GetType().Name + "] ";
        Application.logMessageReceived += HandleLog;
    }

    public void Clear()
    {
        log = "";
    }

    public string Flush()
    {
        string result = log;
        log = "";
        return result;
    }

    public void Post(bool postToChat = true)
    {
        analysisUrl = null;
        postOnComplete = false;
        StartCoroutine( DoPost("https://hastebin.com/documents", log, postToChat) );
    }

    private IEnumerator DoPost(string url, string data, bool postToChat)
    {
        // This first line is necessary as the Log Analyser uses it as an identifier
        data = "Initialize engine version: Twitch Plays\n" + data;

        Debug.Log(LOGPREFIX + "Posting new log to Hastebin");

        WWW www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(data));

        yield return www;

        if (www.error == null)
        {
            // example result
            // {"key":"oxekofidik"}

            string key = www.text;
            key = key.Substring(0, key.Length - 2);
            key = key.Substring(key.LastIndexOf("\"") + 1);
            string rawUrl = "https://hastebin.com/raw/" + key;

            Debug.Log(LOGPREFIX + "Paste now available at " + rawUrl);

            // original url: https://ktane.timwi.de/More/Logfile%20Analyzer.html
            analysisUrl = "http://bombch.us/Chw_#url=" + rawUrl;

            if (postOnComplete)
            {
                PostToChat();
            }
        }
        else
        {
            Debug.Log(LOGPREFIX + "Error: " + www.error);
        }

        yield break;
    }

    public bool PostToChat(string format = "Analysis for this bomb: {0}", string emote = "copyThis")
    {
        if (analysisUrl == null)
        {
            Debug.Log(LOGPREFIX + "No analysis URL available, can't post to chat");
            return false;
        }
        Debug.Log(LOGPREFIX + "Posting analysis URL to chat");
        emote = " " + emote + " ";
        ircConnection.SendMessage(string.Format(emote + format, analysisUrl));
        return true;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        log += message + "\n";
    }

}
