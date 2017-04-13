using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class ComponentSolver : ICommandResponder
{
    public delegate IEnumerator RegexResponse(Match match);

    #region Constructors
    static ComponentSolver()
    {
        _selectableType = ReflectionHelper.FindType("Selectable");
        _interactMethod = _selectableType.GetMethod("HandleInteract", BindingFlags.Public | BindingFlags.Instance);
        _interactEndedMethod = _selectableType.GetMethod("OnInteractEnded", BindingFlags.Public | BindingFlags.Instance);
        _setHighlightMethod = _selectableType.GetMethod("SetHighlight", BindingFlags.Public | BindingFlags.Instance);
        _getFocusDistanceMethod = _selectableType.GetMethod("GetFocusDistance", BindingFlags.Public | BindingFlags.Instance);
    }

    public ComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller)
    {
        BombCommander = bombCommander;
        BombComponent = bombComponent;
        Selectable = (MonoBehaviour)bombComponent.GetComponent(_selectableType);
        IRCConnection = ircConnection;
        Canceller = canceller;
    }
    #endregion

    #region Interface Implementation
    public IEnumerator RespondToCommand(string userNickName, string message, ICommandResponseNotifier responseNotifier)
    {
        if (Solved)
        {
            responseNotifier.ProcessResponse(CommandResponse.NoResponse);
            yield break;
        }

        IEnumerator subcoroutine = RespondToCommandCommon(message);
        if (subcoroutine == null || !subcoroutine.MoveNext())
        {
            subcoroutine = RespondToCommandInternal(message);
            if (subcoroutine == null || !subcoroutine.MoveNext())
            {
                responseNotifier.ProcessResponse(CommandResponse.NoResponse);
                yield break;
            }
        }

        responseNotifier.ProcessResponse(CommandResponse.Start);

        IEnumerator focusCoroutine = BombCommander.Focus(Selectable, FocusDistance, FrontFace);
        while (focusCoroutine.MoveNext())
        {
            yield return focusCoroutine.Current;
        }

        yield return new WaitForSeconds(0.5f);

        int previousStrikeCount = StrikeCount;
        bool parseError = false;
        bool needQuaternionReset = false;

        while (subcoroutine.MoveNext())
        {
            object currentValue = subcoroutine.Current;
            if (currentValue is string)
            {
                string currentString = (string)currentValue;
                if (currentString.Equals("strike", StringComparison.InvariantCultureIgnoreCase))
                {
                    _delegatedStrikeUserNickName = userNickName;
                    _delegatedStrikeResponseNotifier = responseNotifier;
                    _delegatedStrikeCount = StrikeCount;
                }
                else if (currentString.Equals("solve", StringComparison.InvariantCultureIgnoreCase))
                {
                    _delegatedSolveUserNickName = userNickName;
                    _delegatedSolveResponseNotifier = responseNotifier;
                }
                else if (currentString.Equals("parseerror", StringComparison.InvariantCultureIgnoreCase))
                {
                    parseError = true;
                    break;
                }
                else if (currentString.Equals("trycancel", StringComparison.InvariantCultureIgnoreCase) && Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    break;
                }
            }
            else if (currentValue is Quaternion)
            {
                Quaternion localQuaternion = (Quaternion)currentValue;
                BombCommander.RotateByLocalQuaternion(localQuaternion);
                needQuaternionReset = true;
            }
            yield return currentValue;
        }

        if (needQuaternionReset)
        {
            BombCommander.RotateByLocalQuaternion(Quaternion.identity);
        }

        if (parseError)
        {
            responseNotifier.ProcessResponse(CommandResponse.NoResponse);
        }
        else
        {
            if (Solved && _delegatedSolveUserNickName == null)
            {
                AwardSolve(userNickName, responseNotifier);
            }
            else if (previousStrikeCount != StrikeCount && _delegatedStrikeUserNickName == null)
            {
                AwardStrikes(userNickName, responseNotifier, StrikeCount - previousStrikeCount);
            }
            else
            {
                responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
            }

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator defocusCoroutine = BombCommander.Defocus(FrontFace);
        while (defocusCoroutine.MoveNext())
        {
            yield return defocusCoroutine.Current;
        }

        yield return new WaitForSeconds(0.5f);
    }
    #endregion

    #region Public Methods
    public void Update()
    {
        if (_delegatedStrikeUserNickName != null && StrikeCount != _delegatedStrikeCount)
        {
            AwardStrikes(_delegatedStrikeUserNickName, _delegatedStrikeResponseNotifier, StrikeCount - _delegatedStrikeCount);
            _delegatedStrikeUserNickName = null;
            _delegatedStrikeResponseNotifier = null;
            _delegatedStrikeCount = int.MinValue;
        }

        if (_delegatedSolveUserNickName != null && Solved)
        {
            AwardSolve(_delegatedSolveUserNickName, _delegatedSolveResponseNotifier);
            _delegatedSolveUserNickName = null;
            _delegatedSolveResponseNotifier = null;
        }
    }
    #endregion

    #region Abstract Interface
    protected abstract IEnumerator RespondToCommandInternal(string inputCommand);
    #endregion

    #region Protected Helper Methods
    protected void DoInteractionStart(MonoBehaviour interactable)
    {
        MonoBehaviour selectable = (MonoBehaviour)interactable.GetComponent(_selectableType);
        _interactMethod.Invoke(selectable, null);
    }

    protected void DoInteractionEnd(MonoBehaviour interactable)
    {
        MonoBehaviour selectable = (MonoBehaviour)interactable.GetComponent(_selectableType);
        _interactEndedMethod.Invoke(selectable, null);
        _setHighlightMethod.Invoke(selectable, new object[] { false });
    }
    #endregion

    #region Private Methods
    private void AwardSolve(string userNickName, ICommandResponseNotifier responseNotifier)
    {
        IRCConnection.SendMessage(string.Format("VoteYea Module {0} is solved! +1 solve to {1}", Code, userNickName));
        responseNotifier.ProcessResponse(CommandResponse.EndComplete);
    }

    private void AwardStrikes(string userNickName, ICommandResponseNotifier responseNotifier, int strikeCount)
    {
        IRCConnection.SendMessage(string.Format("VoteNay Module {0} got {1} strike{2}! +{3} strike{2} to {4}", Code, strikeCount == 1 ? "a" : strikeCount.ToString(), strikeCount == 1 ? "" : "s", strikeCount, userNickName));
        responseNotifier.ProcessResponse(CommandResponse.EndError, strikeCount);
    }
    #endregion

    public string Code
    {
        get;
        set;
    }

    #region Protected Properties
    protected bool Solved
    {
        get
        {
            return (bool)CommonReflectedTypeInfo.IsSolvedField.GetValue(BombComponent);
        }
    }

    protected bool Detonated
    {
        get
        {
            return (bool)CommonReflectedTypeInfo.HasDetonatedProperty.GetValue(BombCommander.Bomb, null);
        }
    }

    protected int StrikeCount
    {
        get
        {
            //This extra check is required, since it doesn't increment the NumStrikes field on the last strike of the bomb!
            if (Detonated)
            {
                return (int)CommonReflectedTypeInfo.NumStrikesToLoseField.GetValue(BombCommander.Bomb);
            }
            else
            {
                return (int)CommonReflectedTypeInfo.NumStrikesField.GetValue(BombCommander.Bomb);
            }
        }
    }

    protected float FocusDistance
    {
        get
        {
            MonoBehaviour selectable = (MonoBehaviour)BombComponent.GetComponent(_selectableType);
            return (float)_getFocusDistanceMethod.Invoke(selectable, null);
        }
    }

    protected bool FrontFace
    {
        get
        {
            Vector3 componentUp = BombComponent.transform.up;
            Vector3 bombUp = BombCommander.Bomb.transform.up;
            float angleBetween = Vector3.Angle(componentUp, bombUp);
            return angleBetween < 90.0f;
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator RespondToCommandCommon(string inputCommand)
    {
        if (inputCommand.Equals("show", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "show";
            yield return null;
        }
    }
    #endregion

    #region Readonly Fields
    protected readonly BombCommander BombCommander = null;
    protected readonly MonoBehaviour BombComponent = null;
    protected readonly MonoBehaviour Selectable = null;
    protected readonly IRCConnection IRCConnection = null;
    public readonly CoroutineCanceller Canceller = null;
    #endregion

    #region Private Static Fields
    private static Type _selectableType = null;
    private static MethodInfo _interactMethod = null;
    private static MethodInfo _interactEndedMethod = null;
    private static MethodInfo _setHighlightMethod = null;
    private static MethodInfo _getFocusDistanceMethod = null;
    #endregion

    #region Private Fields
    private ICommandResponseNotifier _delegatedStrikeResponseNotifier = null;
    private string _delegatedStrikeUserNickName = null;
    private int _delegatedStrikeCount = int.MinValue;

    private ICommandResponseNotifier _delegatedSolveResponseNotifier = null;
    private string _delegatedSolveUserNickName = null;
    #endregion
}
