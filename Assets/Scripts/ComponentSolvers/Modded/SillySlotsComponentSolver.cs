using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SillySlotsComponentSolver : ComponentSolver
{
    public SillySlotsComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _keepButton = (MonoBehaviour)_keepButtonField.GetValue(bombComponent.GetComponent(_componentSolverType));
        _pullLever = (MonoBehaviour)_pullLeverField.GetValue(bombComponent.GetComponent(_componentSolverType));
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {       
        if (inputCommand.Equals("keep", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "keep";

            DoInteractionStart(_keepButton);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_keepButton);
        }
        else if (inputCommand.Equals("pull", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "pull";

            DoInteractionStart(_pullLever);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_pullLever);
        }
    }

    static SillySlotsComponentSolver()
    {
        _componentSolverType = ReflectionHelper.FindType("SillySlots");
        _keepButtonField = _componentSolverType.GetField("Keep", BindingFlags.Public | BindingFlags.Instance);
        _pullLeverField = _componentSolverType.GetField("Lever", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _componentSolverType = null;
    private static FieldInfo _keepButtonField = null;
    private static FieldInfo _pullLeverField = null;

    private MonoBehaviour _keepButton = null;
    private MonoBehaviour _pullLever = null;
}
