using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class InvisibleWallsComponentSolver : ComponentSolver
{
    public InvisibleWallsComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _buttons = (IList)_buttonsField.GetValue(bombComponent);
        
        helpMessage = "Move with !{0} move u d l r.";
        manualCode = "Mazes";
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {     
        if (!inputCommand.StartsWith("move ", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }

        inputCommand = inputCommand.Substring(5);

        int beforeButtonStrikeCount = StrikeCount;

        string[] sequence = inputCommand.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string buttonString in sequence)
        {
            MonoBehaviour button = null;

            if (buttonString.Equals("u", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("up", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons[0];
            }
            else if (buttonString.Equals("l", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("left", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons[1];
            }
            else if (buttonString.Equals("r", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("right", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons[2];
            }
            else if (buttonString.Equals("d", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("down", StringComparison.InvariantCultureIgnoreCase))
            {
                button = (MonoBehaviour)_buttons[3];
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

    static InvisibleWallsComponentSolver()
    {
        _invisibleWallsComponentType = ReflectionHelper.FindType("InvisibleWallsComponent");
        _buttonsField = _invisibleWallsComponentType.GetField("Buttons", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _invisibleWallsComponentType = null;
    private static FieldInfo _buttonsField = null;

    private IList _buttons = null;
}
