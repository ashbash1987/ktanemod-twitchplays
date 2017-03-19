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

        while (subcoroutine.MoveNext())
        {
            yield return subcoroutine.Current;
        }

        if (Solved)
        {
            responseNotifier.ProcessResponse(CommandResponse.EndComplete);
        }
        else if (previousStrikeCount != StrikeCount)
        {
            responseNotifier.ProcessResponse(CommandResponse.EndError);
        }
        else
        {
            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }

        yield return new WaitForSeconds(0.5f);

        IEnumerator defocusCoroutine = BombCommander.Defocus(FrontFace);
        while (defocusCoroutine.MoveNext())
        {
            yield return defocusCoroutine.Current;
        }

        yield return new WaitForSeconds(0.5f);
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

    #region Protected Properties
    protected bool Solved
    {
        get
        {
            return (bool)CommonReflectedTypeInfo.IsSolvedField.GetValue(BombComponent);
        }
    }

    protected int StrikeCount
    {
        get
        {
            return (int)CommonReflectedTypeInfo.NumStrikesField.GetValue(BombCommander.Bomb);
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
}
