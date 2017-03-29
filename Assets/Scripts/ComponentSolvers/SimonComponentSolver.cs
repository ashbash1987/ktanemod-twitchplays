using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimonComponentSolver : ComponentSolver
{
    public SimonComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _buttons = (Array)_buttonsField.GetValue(bombComponent);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (!inputCommand.StartsWith("press ", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }
        inputCommand = inputCommand.Substring(6);

        int beforeButtonStrikeCount = StrikeCount;

        string[] sequence = inputCommand.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string buttonString in sequence)
        {
            MonoBehaviour button = null;

            if (buttonString.Equals("r", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("red", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons.GetValue(0);
            }
            else if (buttonString.Equals("b", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("blue", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons.GetValue(1);
            }
            else if (buttonString.Equals("g", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("green", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons.GetValue(2);
            }
            else if (buttonString.Equals("y", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("yellow", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons.GetValue(3);
            }

            if (button != null)
            {
                yield return buttonString;

                if (Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    yield break;
                }

                DoInteractionStart(button);
                yield return new WaitForSeconds(0.1f);
                DoInteractionEnd(button);

                //Escape the sequence if a part of the given sequence is wrong
                if (StrikeCount != beforeButtonStrikeCount)
                {
                    break;
                }
            }
        }
    }

    static SimonComponentSolver()
    {
        _simonComponentType = ReflectionHelper.FindType("SimonComponent");
        _buttonsField = _simonComponentType.GetField("buttons", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _simonComponentType = null;
    private static FieldInfo _buttonsField = null;

    private Array _buttons = null;
}
