using System;
using UnityEngine;

public static class ComponentSolverFactory
{
    public static ComponentSolver CreateSolver(MonoBehaviour bomb, MonoBehaviour bombComponent, ComponentTypeEnum componentType)
    {
        switch (componentType)
        {
            case ComponentTypeEnum.Wires:
                return new WireSetComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Keypad:
                return new KeypadComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.BigButton:
                return new ButtonComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Memory:
                return new MemoryComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Simon:
                return new SimonComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Venn:
                return new VennWireComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Morse:
                return new MorseCodeComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.WireSequence:
                return new WireSequenceComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Password:
                return new PasswordComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.Maze:
                return new InvisibleWallsComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.WhosOnFirst:
                return new WhosOnFirstComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.NeedyVentGas:
                return new NeedyVentComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.NeedyCapacitor:
                return new NeedyDischargeComponentSolver(bomb, bombComponent);

            case ComponentTypeEnum.NeedyKnob:
                return new NeedyKnobComponentSolver(bomb, bombComponent);

            default:
                throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }
    }
}

