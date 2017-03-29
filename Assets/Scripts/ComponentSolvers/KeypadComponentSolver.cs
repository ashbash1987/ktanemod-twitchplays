using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class KeypadComponentSolver : ComponentSolver
{
    public KeypadComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
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

        foreach (string buttonIndexString in sequence)
        {
            int buttonIndex = 0;
            if (!int.TryParse(buttonIndexString, out buttonIndex))
            {
                continue;
            }

            buttonIndex--;

            if (buttonIndex >= 0 && buttonIndex < _buttons.Length)
            {
                yield return buttonIndexString;

                if (Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    yield break;
                }

                MonoBehaviour button = (MonoBehaviour)_buttons.GetValue(buttonIndex);
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

    static KeypadComponentSolver()
    {
        _keypadComponentType = ReflectionHelper.FindType("KeypadComponent");
        _buttonsField = _keypadComponentType.GetField("buttons", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _keypadComponentType = null;
    private static FieldInfo _buttonsField = null;

    private Array _buttons = null;
}
