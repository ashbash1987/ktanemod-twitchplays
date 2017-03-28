using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TwitchComponentHandle : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    #region Public Fields
    public TwitchMessage messagePrefab = null;

    public CanvasGroup canvasGroup = null;
    public CanvasGroup highlightGroup = null;
    public Text headerText = null;
    public Text idText = null;
    public ScrollRect messageScroll = null;
    public GameObject messageScrollContents = null;

    public Image upArrow = null;
    public Image downArrow = null;
    public Image leftArrow = null;
    public Image rightArrow = null;

    public Image upArrowHighlight = null;
    public Image downArrowHighlight = null;
    public Image leftArrowHighlight = null;
    public Image rightArrowHighlight = null;

    [HideInInspector]
    public IRCConnection ircConnection = null;

    [HideInInspector]
    public BombCommander bombCommander = null;

    [HideInInspector]
    public MonoBehaviour bombComponent = null;

    [HideInInspector]
    public ComponentTypeEnum componentType = ComponentTypeEnum.Empty;

    [HideInInspector]
    public Vector3 basePosition = Vector3.zero;

    [HideInInspector]
    public Vector3 idealHandlePositionOffset = Vector3.zero;

    [HideInInspector]
    public Direction direction = Direction.Up;

    [HideInInspector]
    public CoroutineQueue coroutineQueue = null;

    [HideInInspector]
    public CoroutineCanceller coroutineCanceller = null;

    [HideInInspector]
    public Leaderboard leaderboard = null;
    #endregion

    #region Private Fields
    private string _code = null;
    private ComponentSolver _solver = null;
    #endregion

    #region Private Statics
    private static int _nextID = 0;
    private static int GetNewID()
    {
        _nextID = (_nextID + 1) % 1000;
        return _nextID;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _code = GetNewID().ToString();
    }

    private void Start()
    {
        if (bombComponent != null)
        {
            headerText.text = (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null);
        }

        idText.text = string.Format("!{0}", _code);

        canvasGroup.alpha = 0.0f;
        highlightGroup.alpha = 0.0f;

        Arrow.gameObject.SetActive(true);
        HighlightArrow.gameObject.SetActive(true);

        _solver = ComponentSolverFactory.CreateSolver(bombCommander, bombComponent, componentType, ircConnection, coroutineCanceller);
    }

    private void LateUpdate()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 componentForward = transform.up;

        float angle = Vector3.Angle(cameraForward, -componentForward);
        float lerpAmount = Mathf.InverseLerp(60.0f, 20.0f, angle);
        lerpAmount = Mathf.Lerp(canvasGroup.alpha, lerpAmount, Time.deltaTime * 5.0f);
        canvasGroup.alpha = lerpAmount;
        transform.localPosition = basePosition + Vector3.Lerp(Vector3.zero, idealHandlePositionOffset, Mathf.SmoothStep(0.0f, 1.0f, lerpAmount));

        messageScroll.verticalNormalizedPosition = 0.0f;
    }
    #endregion

    #region Message Interface
    public void OnMessageReceived(string userNickName, string userColor, string text)
    {
        Match match = Regex.Match(text, string.Format("^!{0} (.+)", _code), RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return;
        }

        string internalCommand = match.Groups[1].Value;

        TwitchMessage message = (TwitchMessage)Instantiate(messagePrefab, messageScrollContents.transform, false);
        message.leaderboard = leaderboard;
        message.userName = userNickName;
        if (string.IsNullOrEmpty(userColor))
        {
            message.SetMessage(string.Format("<b>{0}</b>: {1}", userNickName, internalCommand));
            message.userColor = new Color(0.31f, 0.31f, 0.31f);
        }
        else
        {
            message.SetMessage(string.Format("<b><color={2}>{0}</color></b>: {1}", userNickName, internalCommand, userColor));
            if (!ColorUtility.TryParseHtmlString(userColor, out message.userColor))
            {
                message.userColor = new Color(0.31f, 0.31f, 0.31f);
            }
        }

        if (_solver != null)
        {
            coroutineQueue.AddToQueue(RespondToCommandCoroutine(userNickName, internalCommand, message));
        }
    }
    #endregion

    #region Private Methods
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

        IEnumerator commandResponseCoroutine = _solver.RespondToCommand(userNickName, internalCommand, message);
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

    #region Private Properties
    private Image Arrow
    {
        get
        {
            switch (direction)
            {
                case Direction.Up:
                    return upArrow;
                case Direction.Down:
                    return downArrow;
                case Direction.Left:
                    return leftArrow;
                case Direction.Right:
                    return rightArrow;

                default:
                    return null;
            }
        }
    }

    private Image HighlightArrow
    {
        get
        {
            switch (direction)
            {
                case Direction.Up:
                    return upArrowHighlight;
                case Direction.Down:
                    return downArrowHighlight;
                case Direction.Left:
                    return leftArrowHighlight;
                case Direction.Right:
                    return rightArrowHighlight;

                default:
                    return null;
            }
        }
    }
    #endregion
}
