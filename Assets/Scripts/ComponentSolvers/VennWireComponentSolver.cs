using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class VennWireComponentSolver : ComponentSolver
{
    public VennWireComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent) :
        base(bomb, bombComponent)
    {
        _wires = (Array)_activeWiresProperty.GetValue(bombComponent, null);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        int beforeButtonStrikeCount = StrikeCount;

        string[] sequence = inputCommand.Split(' ');

        foreach (string wireIndexString in sequence)
        {
            int wireIndex = 0;
            if (!int.TryParse(wireIndexString, out wireIndex))
            {
                continue;
            }

            wireIndex--;

            if (wireIndex >= 0 && wireIndex < _wires.Length)
            {
                yield return wireIndexString;

                MonoBehaviour wire = (MonoBehaviour)_wires.GetValue(wireIndex);

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

    static VennWireComponentSolver()
    {
        _vennWireComponentType = ReflectionHelper.FindType("Assets.Scripts.Components.VennWire.VennWireComponent");
        _activeWiresProperty = _vennWireComponentType.GetProperty("ActiveWires", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _vennWireComponentType = null;
    private static PropertyInfo _activeWiresProperty = null;

    private Array _wires = null;
}
