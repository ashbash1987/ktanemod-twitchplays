using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimonStatesComponentSolver : ComponentSolver
{
    public SimonStatesComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _blue = (MonoBehaviour) _bluebuttonField.GetValue(bombComponent.GetComponent(_componentType));
        _green = (MonoBehaviour) _greenbuttonField.GetValue(bombComponent.GetComponent(_componentType));
        _red = (MonoBehaviour) _redbuttonField.GetValue(bombComponent.GetComponent(_componentType));
        _yellow = (MonoBehaviour) _yellowbuttonField.GetValue(bombComponent.GetComponent(_componentType));
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
                button = _red;
            }
            else if (buttonString.Equals("b", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("blue", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _blue;
            }
            else if (buttonString.Equals("g", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("green", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _green;
            }
            else if (buttonString.Equals("y", StringComparison.InvariantCultureIgnoreCase) || buttonString.Equals("yellow", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _yellow;
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

    static SimonStatesComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("AdvancedSimon");
        _redbuttonField = _componentType.GetField("ButtonRed", BindingFlags.NonPublic | BindingFlags.Instance);
        _greenbuttonField = _componentType.GetField("ButtonGreen", BindingFlags.NonPublic | BindingFlags.Instance);
        _yellowbuttonField = _componentType.GetField("ButtonYellow", BindingFlags.NonPublic | BindingFlags.Instance);
        _bluebuttonField = _componentType.GetField("ButtonBlue", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _redbuttonField, _greenbuttonField, _yellowbuttonField, _bluebuttonField = null;

    private MonoBehaviour _red = null;
    private MonoBehaviour _green = null;
    private MonoBehaviour _yellow = null;
    private MonoBehaviour _blue = null;

}
