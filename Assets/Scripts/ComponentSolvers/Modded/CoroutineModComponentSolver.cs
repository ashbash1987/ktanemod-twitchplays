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
        IEnumerator responseCoroutine = (IEnumerator)ProcessMethod.Invoke(CommandComponent, new object[] { inputCommand });
        if (responseCoroutine == null)
        {
            yield break;
        }

        yield return "modcoroutine";

        while (responseCoroutine.MoveNext())
        {
            object currentObject = responseCoroutine.Current;
            if (currentObject.GetType() == typeof(KMSelectable))
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
