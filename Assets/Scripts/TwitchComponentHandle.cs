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
    public Text headerText = null;
    public Text idText = null;
    public ScrollRect messageScroll = null;
    public GameObject messageScrollContents = null;

    public Image upArrow = null;
    public Image downArrow = null;
    public Image leftArrow = null;
    public Image rightArrow = null;

    [HideInInspector]
    public IRCConnection ircConnection = null;

    [HideInInspector]
    public MonoBehaviour bomb = null;

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
    #endregion

    #region Private Fields
    private string _code = null;
    private Regex _regex = null;
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
        _regex = new Regex(string.Format("^!{0} (.+)", _code));
    }

    private void Start()
    {
        if (ircConnection != null)
        {
            ircConnection.OnMessageReceived.AddListener(OnMessageReceived);
        }

        if (bombComponent != null)
        {
            headerText.text = (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null);
        }

        idText.text = string.Format("!{0}", _code);

        canvasGroup.alpha = 0.0f;

        switch (direction)
        {
            case Direction.Up:
                upArrow.gameObject.SetActive(true);
                break;
            case Direction.Down:
                downArrow.gameObject.SetActive(true);
                break;
            case Direction.Left:
                leftArrow.gameObject.SetActive(true);
                break;
            case Direction.Right:
                rightArrow.gameObject.SetActive(true);
                break;

            default:
                break;
        }

        _solver = ComponentSolverFactory.CreateSolver(bomb, bombComponent, componentType, ircConnection);
    }

    private void OnDestroy()
    {
        if (ircConnection != null)
        {
            ircConnection.OnMessageReceived.RemoveListener(OnMessageReceived);
        }
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

    #region Public Methods
    public void OnMessageReceived(string userNickName, string userColor, string text)
    {
        Match match = _regex.Match(text);
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

        if (_solver != null)
        {
            coroutineQueue.AddToQueue(_solver.RespondToCommand(userNickName, internalCommand));
        }
    }
    #endregion
}
