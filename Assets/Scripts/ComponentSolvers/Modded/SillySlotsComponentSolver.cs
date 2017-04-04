using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SillySlotsComponentSolver : ComponentSolver
{
    private Component c;

    public SillySlotsComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        c = bombComponent.GetComponent(_componentSolverType);

        _keepButton = (MonoBehaviour)_keepButtonField.GetValue(c);
        _pullLever = (MonoBehaviour)_pullLeverField.GetValue(c);
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

            if ((int) _stageField.GetValue(c) != 4) yield break;

            while ((bool)_animatingField.GetValue(c))
                yield return new WaitForSeconds(0.1f);
        }
    }

    static SillySlotsComponentSolver()
    {
        _componentSolverType = ReflectionHelper.FindType("SillySlots");
        _keepButtonField = _componentSolverType.GetField("Keep", BindingFlags.Public | BindingFlags.Instance);
        _pullLeverField = _componentSolverType.GetField("Lever", BindingFlags.Public | BindingFlags.Instance);
        _stageField = _componentSolverType.GetField("mStage", BindingFlags.NonPublic | BindingFlags.Instance);
        _animatingField = _componentSolverType.GetField("bAnimating", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static Type _componentSolverType = null;
    private static FieldInfo _keepButtonField = null;
    private static FieldInfo _pullLeverField = null;
    private static FieldInfo _stageField = null;
    private static FieldInfo _animatingField = null;

    private MonoBehaviour _keepButton = null;
    private MonoBehaviour _pullLever = null;
}
