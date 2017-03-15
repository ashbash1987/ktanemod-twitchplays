using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class NeedyDischargeComponentSolver : ComponentSolver
{
    public NeedyDischargeComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent, IRCConnection ircConnection) :
        base(bomb, bombComponent, ircConnection)
    {
        _dischargeButton = (MonoBehaviour)_dischargeButtonField.GetValue(bombComponent);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        string[] commandParts = inputCommand.Split(' ');

        if (commandParts.Length != 2)
        {
            yield break;
        }

        if (!commandParts[0].Equals("hold", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }

        int holdTime = 0;
        if (!int.TryParse(commandParts[1], out holdTime))
        {
            yield break;
        }

        yield return "hold";

        DoInteractionStart(_dischargeButton);
        yield return new WaitForSeconds(holdTime);
        DoInteractionEnd(_dischargeButton);
    }

    static NeedyDischargeComponentSolver()
    {
        _needyDischargeComponentType = ReflectionHelper.FindType("NeedyDischargeComponent");
        _dischargeButtonField = _needyDischargeComponentType.GetField("DischargeButton", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _needyDischargeComponentType = null;
    private static FieldInfo _dischargeButtonField = null;

    private MonoBehaviour _dischargeButton = null;
}
