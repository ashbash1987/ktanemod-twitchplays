using System;
using UnityEngine;

public static class ComponentSolverFactory
{
    public static ComponentSolver CreateSolver(BombCommander bombCommander, MonoBehaviour bombComponent, ComponentTypeEnum componentType, IRCConnection ircConnection, CoroutineCanceller canceller)
    {
        switch (componentType)
        {
            case ComponentTypeEnum.Wires:
                return new WireSetComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Keypad:
                return new KeypadComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.BigButton:
                return new ButtonComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Memory:
                return new MemoryComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Simon:
                return new SimonComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Venn:
                return new VennWireComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Morse:
                return new MorseCodeComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.WireSequence:
                return new WireSequenceComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Password:
                return new PasswordComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.Maze:
                return new InvisibleWallsComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.WhosOnFirst:
                return new WhosOnFirstComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.NeedyVentGas:
                return new NeedyVentComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.NeedyCapacitor:
                return new NeedyDischargeComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

            case ComponentTypeEnum.NeedyKnob:
                return new NeedyKnobComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
                
            case ComponentTypeEnum.Mod:
                KMBombModule module = bombComponent.GetComponent<KMBombModule>();
                switch (module.ModuleDisplayName)
                {
                    case "Forget Me Not": return new ForgetMeNotComponentSolver (bombCommander, bombComponent, ircConnection, canceller);
                    case "Square Button": return new SquareButtonComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
                    case "Round Keypad":  return new RoundKeypadComponentSolver (bombCommander, bombComponent, ircConnection, canceller);
                    case "Safety Safe":   return new SafetySafeComponentSolver  (bombCommander, bombComponent, ircConnection, canceller);
                    case "Simon States":  return new SimonStatesComponentSolver (bombCommander, bombComponent, ircConnection, canceller);
                    default:
                        throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
                }
                
            case ComponentTypeEnum.NeedyMod:
                KMNeedyModule needy = bombComponent.GetComponent<KMNeedyModule>();
                switch (needy.ModuleDisplayName)
                {
                    default:
                        throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
                }

            default:
                throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }
    }
}

