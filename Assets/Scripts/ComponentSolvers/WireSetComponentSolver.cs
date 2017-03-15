using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class WireSetComponentSolver : ComponentSolver
{
    public WireSetComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent):
        base(bomb, bombComponent)
    {
        _wires = (IList)_wiresField.GetValue(bombComponent);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        int wireIndex = 0;
        if (!int.TryParse(inputCommand, out wireIndex))
        {
            yield break;
        }

        wireIndex--;

        if (wireIndex >= 0 && wireIndex < _wires.Count)
        {
            yield return inputCommand;

            MonoBehaviour wireToCut = (MonoBehaviour)_wires[wireIndex];
            DoInteractionStart(wireToCut);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(wireToCut);
        }
    }

    static WireSetComponentSolver()
    {
        _wireSetComponentType = ReflectionHelper.FindType("WireSetComponent");
        _wiresField = _wireSetComponentType.GetField("wires", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _wireSetComponentType = null;
    private static FieldInfo _wiresField = null;

    private IList _wires = null;
}
