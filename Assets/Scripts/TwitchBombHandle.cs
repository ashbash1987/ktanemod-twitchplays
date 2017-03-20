using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TwitchBombHandle : MonoBehaviour
{
    #region Public Fields
    public TwitchMessage messagePrefab = null;

    public CanvasGroup canvasGroup = null;
    public CanvasGroup highlightGroup = null;
    public Text idText = null;
    public ScrollRect messageScroll = null;
    public GameObject messageScrollContents = null;

    [HideInInspector]
    public IRCConnection ircConnection = null;

    [HideInInspector]
    public BombCommander bombCommander = null;

    [HideInInspector]
    public CoroutineQueue coroutineQueue = null;

    [HideInInspector]
    public CoroutineCanceller coroutineCanceller = null;
    #endregion

    #region Private Fields
    private string _code = null;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _code = "bomb";
    }

    private void Start()
    {
        if (ircConnection != null)
        {
            ircConnection.OnMessageReceived.AddListener(OnMessageReceived);
        }

        idText.text = string.Format("!{0}", _code);

        canvasGroup.alpha = 1.0f;
        highlightGroup.alpha = 0.0f;
    }

    private void OnDestroy()
    {
        if (ircConnection != null)
        {
            ircConnection.OnMessageReceived.RemoveListener(OnMessageReceived);
        }
    }
    #endregion

    #region Private Methods
    private void OnMessageReceived(string userNickName, string userColor, string text)
    {
        Match match = Regex.Match(text, string.Format("^!{0} (.+)", _code), RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return;
        }

        string internalCommand = match.Groups[1].Value;

        TwitchMessage message = (TwitchMessage)Instantiate(messagePrefab, messageScrollContents.transform, false);
        if (string.IsNullOrEmpty(userColor))
        {
            message.SetMessage(string.Format("<b>{0}</b>: {1}", userNickName, internalCommand));
        }
        else
        {
            message.SetMessage(string.Format("<b><color={2}>{0}</color></b>: {1}", userNickName, internalCommand, userColor));
        }

        coroutineQueue.AddToQueue(RespondToCommandCoroutine(userNickName, internalCommand, message));
    }

    private IEnumerator RespondToCommandCoroutine(string userNickName, string internalCommand, ICommandResponseNotifier message, float fadeDuration = 0.1f)
    {
        float time = Time.time;
        while (Time.time - time < fadeDuration)
        {
            float lerp = (Time.time - time) / fadeDuration;
            highlightGroup.alpha = Mathf.Lerp(0.0f, 1.0f, lerp);
            yield return null;
        }
        highlightGroup.alpha = 1.0f;

        IEnumerator commandResponseCoroutine = bombCommander.RespondToCommand(userNickName, internalCommand, message);
        while (commandResponseCoroutine.MoveNext())
        {
            yield return commandResponseCoroutine.Current;
        }

        time = Time.time;
        while (Time.time - time < fadeDuration)
        {
            float lerp = (Time.time - time) / fadeDuration;
            highlightGroup.alpha = Mathf.Lerp(1.0f, 0.0f, lerp);
            yield return null;
        }
        highlightGroup.alpha = 0.0f;
    }
    #endregion    
}
