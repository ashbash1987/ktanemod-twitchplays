using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimpleModComponentSolver : ComponentSolver
{
    public SimpleModComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller, MethodInfo processMethod, Component commandComponent, string manual = null, string help = null) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        ProcessMethod = processMethod;
        CommandComponent = commandComponent;
        helpMessage = help;
        manualCode = manual;
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (ProcessMethod == null)
        {
            Debug.LogError("A declared TwitchPlays SimpleModComponentSolver process method is <null>, yet a component solver has been created; command invokation will not continue.");
            yield break;
        }

        KMSelectable[] selectableSequence = null;
        try
        {
            selectableSequence = (KMSelectable[])ProcessMethod.Invoke(CommandComponent, new object[] { inputCommand });
            if (selectableSequence == null)
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

        yield return "modsequence";

        int beforeInteractionStrikeCount = StrikeCount;

        for(int selectableIndex = 0; selectableIndex < selectableSequence.Length; ++selectableIndex)
        {
            if (Canceller.ShouldCancel)
            {
                Canceller.ResetCancel();
                yield break;
            }

            KMSelectable selectable = selectableSequence[selectableIndex];
            if (selectable == null)
            {
                Debug.LogErrorFormat("An empty selectable has been found at index {0} within the selectable array returned from {1}.{2}; Skipping this index, however this may have unintended sideeffects.", selectableIndex, ProcessMethod.DeclaringType.FullName, ProcessMethod.Name);
                continue;
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
