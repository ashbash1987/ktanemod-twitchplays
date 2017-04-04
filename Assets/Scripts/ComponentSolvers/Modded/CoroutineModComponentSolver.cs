using System;
using System.Collections;
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
            yield return responseCoroutine.Current;
        }
    }

    private readonly MethodInfo ProcessMethod = null;
    private readonly Component CommandComponent = null;
}
