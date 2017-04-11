using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ComponentSolverFactory
{
    private delegate ComponentSolver ModComponentSolverDelegate(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller);
    private static readonly Dictionary<string, ModComponentSolverDelegate> ModComponentSolverCreators;

    private enum ModCommandType
    {
        Simple,
        Coroutine
    }

    static ComponentSolverFactory()
    {
        ModComponentSolverCreators = new Dictionary<string, ModComponentSolverDelegate>();
        ModComponentSolverCreators["MemoryV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new ForgetMeNotComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["KeypadV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new RoundKeypadComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["ButtonV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SquareButtonComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["SimonV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SimonStatesComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["PasswordV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SafetySafeComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["NeedyVentV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new NeedyQuizComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["TwoBits"] = (bombCommander, bombComponent, ircConnection, canceller) => new TwoBitsComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
		ModComponentSolverCreators["NeedyKnobV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new NeedyRotaryPhoneComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

        //Perky Modules  (Silly Slots is maintained by Timwi, and as such its handler lives there.)
        ModComponentSolverCreators["CrazyTalk"] = (bombCommander, bombComponent, ircConnection, canceller) => new CrazyTalkComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["ForeignExchangeRates"] = (bombCommander, bombComponent, ircConnection, canceller) => new ForeignExchangeRatesComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

    }

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
                KMBombModule solvableModule = bombComponent.GetComponent<KMBombModule>();
                return CreateModComponentSolver(bombCommander, bombComponent, ircConnection, canceller, solvableModule.ModuleType);                

            case ComponentTypeEnum.NeedyMod:
                KMNeedyModule needyModule = bombComponent.GetComponent<KMNeedyModule>();
                return CreateModComponentSolver(bombCommander, bombComponent, ircConnection, canceller, needyModule.ModuleType);

            default:
                throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays'.", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }
    }

    private static ComponentSolver CreateModComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller, string moduleType)
    {
        if (ModComponentSolverCreators.ContainsKey(moduleType))
        {
            return ModComponentSolverCreators[moduleType](bombCommander, bombComponent, ircConnection, canceller);
        }

        Debug.LogFormat("Attempting to find a valid process command method to respond with on component {0}...", moduleType);

        ModComponentSolverDelegate modComponentSolverCreator = GenerateModComponentSolverCreator(bombComponent);
        if (modComponentSolverCreator == null)
        {
            throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays' - Could not generate a valid componentsolver for the mod component!", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }

        ModComponentSolverCreators[moduleType] = modComponentSolverCreator;

        return modComponentSolverCreator(bombCommander, bombComponent, ircConnection, canceller);
    }

    private static ModComponentSolverDelegate GenerateModComponentSolverCreator(MonoBehaviour bombComponent)
    {
        ModCommandType commandType = ModCommandType.Simple;
        Type commandComponentType = null;
        MethodInfo method = FindProcessCommandMethod(bombComponent, out commandType, out commandComponentType);
        if (method != null)
        {
            switch (commandType)
            {
                case ModCommandType.Simple:
                    return delegate (BombCommander _bombCommander, MonoBehaviour _bombComponent, IRCConnection _ircConnection, CoroutineCanceller _canceller)
                    {
                        Component commandComponent = _bombComponent.GetComponentInChildren(commandComponentType);
                        return new SimpleModComponentSolver(_bombCommander, _bombComponent, _ircConnection, _canceller, method, commandComponent);
                    };
                case ModCommandType.Coroutine:
                    return delegate (BombCommander _bombCommander, MonoBehaviour _bombComponent, IRCConnection _ircConnection, CoroutineCanceller _canceller)
                    {
                        Component commandComponent = _bombComponent.GetComponentInChildren(commandComponentType);
                        return new CoroutineModComponentSolver(_bombCommander, _bombComponent, _ircConnection, _canceller, method, commandComponent);
                    };

                default:
                    break;
            }
        }

        return null;
    }

    private static MethodInfo FindProcessCommandMethod(MonoBehaviour bombComponent, out ModCommandType commandType, out Type commandComponentType)
    {
        Component[] allComponents = bombComponent.GetComponentsInChildren<Component>(true);
        foreach (Component component in allComponents)
        {
            Type type = component.GetType();
            MethodInfo candidateMethod = type.GetMethod("ProcessTwitchCommand", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (candidateMethod == null)
            {
                continue;
            }

            if (ValidateMethodCommandMethod(type, candidateMethod, out commandType))
            {
                commandComponentType = type;
                return candidateMethod;
            }
        }

        commandType = ModCommandType.Simple;
        commandComponentType = null;
        return null;
    }

    private static bool ValidateMethodCommandMethod(Type type, MethodInfo candidateMethod, out ModCommandType commandType)
    {
        commandType = ModCommandType.Simple;

        ParameterInfo[] parameters = candidateMethod.GetParameters();
        if (parameters == null || parameters.Length == 0)
        {
            Debug.LogFormat("Found a potential candidate ProcessCommand method in {0}, but the parameter list does not match the expected parameter list (too few parameters).", type.FullName);
            return false;
        }

        if (parameters.Length > 1)
        {
            Debug.LogFormat("Found a potential candidate ProcessCommand method in {0}, but the parameter list does not match the expected parameter list (too many parameters).", type.FullName);
            return false;
        }

        if (parameters[0].ParameterType != typeof(string))
        {
            Debug.LogFormat("Found a potential candidate ProcessCommand method in {0}, but the parameter list does not match the expected parameter list (expected a single string parameter, got a single {1} parameter).", type.FullName, parameters[0].ParameterType.FullName);
            return false;
        }

        if (candidateMethod.ReturnType == typeof(KMSelectable[]))
        {
            Debug.LogFormat("Found a valid candidate ProcessCommand method in {0} (using easy/simple API).", type.FullName);
            commandType = ModCommandType.Simple;
            return true;
        }

        if (candidateMethod.ReturnType == typeof(IEnumerator))
        {
            Debug.LogFormat("Found a valid candidate ProcessCommand method in {0} (using advanced/coroutine API).", type.FullName);
            commandType = ModCommandType.Coroutine;
            return true;
        }

        return false;
    }
}

