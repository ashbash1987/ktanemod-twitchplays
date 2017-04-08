using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CoroutineModComponentSolver : ComponentSolver
{
    public CoroutineModComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller, MethodInfo processMethod, Component commandComponent) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        ProcessMethod = processMethod;
        CommandComponent = commandComponent;
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (ProcessMethod == null)
        {
            Debug.LogError("A declared TwitchPlays CoroutineModComponentSolver process method is <null>, yet a component solver has been created; command invokation will not continue.");
            yield break;
        }

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

        yield return "modcoroutine";

        //This looks slightly nasty, but that's because of a compiler restriction that prevents yielding from within a try block with an associated catch block.
        //An alternative would've been a try..finally (that is allowable), but then the exception details would get lost.
        //So, exception-handle the .MoveNext() call separately, then continue on.
        while (true)
        {
            try
            {
                if (!responseCoroutine.MoveNext())
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", ProcessMethod.DeclaringType.FullName, ProcessMethod.Name);
                Debug.LogException(ex);
                break;
            }

            object currentObject = responseCoroutine.Current;
            if (currentObject is KMSelectable)
            {
                KMSelectable selectable = (KMSelectable)currentObject;
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
            yield return currentObject;
        }        
    }

    private readonly MethodInfo ProcessMethod = null;
    private readonly Component CommandComponent = null;
    private readonly List<KMSelectable> HeldSelectables = new List<KMSelectable>();
}
