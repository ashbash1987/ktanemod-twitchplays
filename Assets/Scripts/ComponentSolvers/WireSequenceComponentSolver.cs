using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class WireSequenceComponentSolver : ComponentSolver
{
    public WireSequenceComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent, IRCConnection ircConnection) :
        base(bomb, bombComponent, ircConnection)
    {
        _wireSequence = (IList)_wireSequenceField.GetValue(bombComponent);
        _upButton = (MonoBehaviour)_upButtonField.GetValue(bombComponent);
        _downButton = (MonoBehaviour)_downButtonField.GetValue(bombComponent);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (inputCommand.Equals("up", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "up";

            DoInteractionStart(_upButton);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_upButton);
        }
        else if (inputCommand.Equals("down", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "down";

            DoInteractionStart(_downButton);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_downButton);
        }
        else
        {
            if (!inputCommand.StartsWith("cut ", StringComparison.InvariantCultureIgnoreCase))
            {
                yield break;
            }
            inputCommand = inputCommand.Substring(4);

            int beforeButtonStrikeCount = StrikeCount;

            string[] sequence = inputCommand.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string wireIndexString in sequence)
            {
                int wireIndex = 0;
                if (!int.TryParse(wireIndexString, out wireIndex))
                {
                    continue;
                }

                wireIndex--;

                if (CanInteractWithWire(wireIndex))
                {
                    yield return wireIndexString;

                    MonoBehaviour wire = GetWire(wireIndex);
                    DoInteractionStart(wire);
                    yield return new WaitForSeconds(0.1f);
                    DoInteractionEnd(wire);

                    //Escape the sequence if a part of the given sequence is wrong
                    if (StrikeCount != beforeButtonStrikeCount)
                    {
                        break;
                    }
                }
            }
        }
    }

    private bool CanInteractWithWire(int wireIndex)
    {
        int wirePageIndex = wireIndex / 3;
        return wirePageIndex == (int)_currentPageField.GetValue(BombComponent);
    }

    private MonoBehaviour GetWire(int wireIndex)
    {
        return (MonoBehaviour)_wireField.GetValue(_wireSequence[wireIndex]);
    }

    static WireSequenceComponentSolver()
    {
        _wireSequenceComponentType = ReflectionHelper.FindType("WireSequenceComponent");
        _wireSequenceField = _wireSequenceComponentType.GetField("wireSequence", BindingFlags.NonPublic | BindingFlags.Instance);
        _currentPageField = _wireSequenceComponentType.GetField("currentPage", BindingFlags.NonPublic | BindingFlags.Instance);
        _upButtonField = _wireSequenceComponentType.GetField("UpButton", BindingFlags.Public | BindingFlags.Instance);
        _downButtonField = _wireSequenceComponentType.GetField("DownButton", BindingFlags.Public | BindingFlags.Instance);

        _wireConfigurationType = ReflectionHelper.FindType("WireSequenceComponent+WireConfiguration");
        _wireField = _wireConfigurationType.GetField("Wire", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _wireSequenceComponentType = null;
    private static Type _wireConfigurationType = null;
    private static FieldInfo _wireSequenceField = null;
    private static FieldInfo _currentPageField = null;
    private static FieldInfo _upButtonField = null;
    private static FieldInfo _downButtonField = null;
    private static FieldInfo _wireField = null;

    private IList _wireSequence = null;
    private MonoBehaviour _upButton = null;
    private MonoBehaviour _downButton = null;
}
