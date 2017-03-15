using System;
using UnityEngine;

public static class ComponentSolverFactory
{
    public static ComponentSolver CreateSolver(MonoBehaviour bomb, MonoBehaviour bombComponent, ComponentTypeEnum componentType, IRCConnection ircConnection)
    {
        switch (componentType)
        {
            case ComponentTypeEnum.Wires:
                return new WireSetComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Keypad:
                return new KeypadComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.BigButton:
                return new ButtonComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Memory:
                return new MemoryComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Simon:
                return new SimonComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Venn:
                return new VennWireComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Morse:
                return new MorseCodeComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.WireSequence:
                return new WireSequenceComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Password:
                return new PasswordComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.Maze:
                return new InvisibleWallsComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.WhosOnFirst:
                return new WhosOnFirstComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.NeedyVentGas:
                return new NeedyVentComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.NeedyCapacitor:
                return new NeedyDischargeComponentSolver(bomb, bombComponent, ircConnection);

            case ComponentTypeEnum.NeedyKnob:
                return new NeedyKnobComponentSolver(bomb, bombComponent, ircConnection);

            default:
                throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }
    }
}

