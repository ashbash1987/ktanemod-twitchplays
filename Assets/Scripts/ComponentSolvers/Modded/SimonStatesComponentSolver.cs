using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimonStatesComponentSolver : ComponentSolver
{
    public SimonStatesComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _tl = (MonoBehaviour) _tlField.GetValue(bombComponent.GetComponent(_componentType));
        _tr = (MonoBehaviour) _trField.GetValue(bombComponent.GetComponent(_componentType));
        _bl = (MonoBehaviour) _blField.GetValue(bombComponent.GetComponent(_componentType));
        _br = (MonoBehaviour) _brField.GetValue(bombComponent.GetComponent(_componentType));
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

            if (buttonString.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _tl;
            }
            else if (buttonString.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _tr;
            }
            else if (buttonString.Equals("3", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _bl;
            }
            else if (buttonString.Equals("4", StringComparison.InvariantCultureIgnoreCase))
            {
                button = _br;
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
        _tlField = _componentType.GetField("ButtonTL", BindingFlags.Public | BindingFlags.Instance);
        _trField = _componentType.GetField("ButtonTR", BindingFlags.Public | BindingFlags.Instance);
        _blField = _componentType.GetField("ButtonBL", BindingFlags.Public | BindingFlags.Instance);
        _brField = _componentType.GetField("ButtonBR", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _tlField, _trField, _blField, _brField = null;

    private MonoBehaviour _tl, _tr, _bl, _br = null;

}
