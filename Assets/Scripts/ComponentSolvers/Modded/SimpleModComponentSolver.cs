using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimpleModComponentSolver : ComponentSolver
{
    public SimpleModComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller, MethodInfo processMethod, Component commandComponent) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        ProcessMethod = processMethod;
        CommandComponent = commandComponent;
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        KMSelectable[] selectableSequence = (KMSelectable[])ProcessMethod.Invoke(CommandComponent, new object[] { inputCommand });
        if (selectableSequence == null)
        {
            yield break;
        }

        yield return "modsequence";

        int beforeInteractionStrikeCount = StrikeCount;

        foreach (KMSelectable selectable in selectableSequence)
        {
            if (Canceller.ShouldCancel)
            {
                Canceller.ResetCancel();
                yield break;
            }

            DoInteractionStart(selectable);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(selectable);

            //Escape the sequence if a part of the given sequence is wrong
            if (StrikeCount != beforeInteractionStrikeCount)
            {
                yield break;
            }
        }
    }

    private readonly MethodInfo ProcessMethod = null;
    private readonly Component CommandComponent = null;
}
