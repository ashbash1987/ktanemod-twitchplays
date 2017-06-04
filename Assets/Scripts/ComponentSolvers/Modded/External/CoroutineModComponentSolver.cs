using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CoroutineModComponentSolver : ComponentSolver
{
    public CoroutineModComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller, MethodInfo processMethod, Component commandComponent, string manual, string help, FieldInfo cancelfield, Type canceltype) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        ProcessMethod = processMethod;
        CommandComponent = commandComponent;
        helpMessage = help;
        manualCode = manual;
        TryCancelField = cancelfield;
        TryCancelComponentSolverType = canceltype;
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (ProcessMethod == null)
        {
            Debug.LogError("A declared TwitchPlays CoroutineModComponentSolver process method is <null>, yet a component solver has been created; command invokation will not continue.");
            yield break;
        }

        int beforeStrikeCount = StrikeCount;
        IEnumerator responseCoroutine = null;
        try
        {
            responseCoroutine = (IEnumerator)ProcessMethod.Invoke(CommandComponent, new object[] { inputCommand });
            if (responseCoroutine == null)
            {
                yield break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", ProcessMethod.DeclaringType.FullName, ProcessMethod.Name);
            Debug.LogException(ex);
            yield break;
        }

        bool moveNext = false;
        while (beforeStrikeCount == StrikeCount && !Solved)
        {
            try
            {
                if (!responseCoroutine.MoveNext())
                    yield break;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat(
                    "An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.",
                    ProcessMethod.DeclaringType.FullName, ProcessMethod.Name);
                Debug.LogException(ex);
                yield break;
            }
            if (!moveNext)
            {
                moveNext = true;
                yield return "CoroutineModComponentSolver";
            }

            object currentObject = responseCoroutine.Current;
            if (currentObject is KMSelectable)
            {
                KMSelectable selectable = (KMSelectable) currentObject;
                if (HeldSelectables.Contains(selectable))
                {
                    DoInteractionEnd(selectable);
                    HeldSelectables.Remove(selectable);
                }
                else
                {
                    DoInteractionStart(selectable);
                    HeldSelectables.Add(selectable);
                }
            }
            if (currentObject is KMSelectable[])
            {
                KMSelectable[] selectables = (KMSelectable[]) currentObject;
                foreach (KMSelectable selectable in selectables)
                {
                    DoInteractionStart(selectable);
                    yield return new WaitForSeconds(0.1f);
                    DoInteractionEnd(selectable);
                    if (beforeStrikeCount != StrikeCount || Canceller.ShouldCancel || Solved)
                        break;
                }
            }
            if (currentObject is string)
            {
                string str = (string) currentObject;
                if (str.Equals("cancelled", StringComparison.InvariantCultureIgnoreCase))
                {
                    Canceller.ResetCancel();
                    TryCancel = false;
                }
            }
            yield return currentObject;

            if (Canceller.ShouldCancel)
                TryCancel = true;
        } 
    }

    private readonly MethodInfo ProcessMethod = null;
    private readonly Component CommandComponent = null;
    private readonly HashSet<KMSelectable> HeldSelectables = new HashSet<KMSelectable>();
}
