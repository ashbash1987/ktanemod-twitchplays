using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ComponentSolverFactory
{
    private delegate ComponentSolver ModComponentSolverDelegate(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller);
    private static readonly Dictionary<string, ModComponentSolverDelegate> ModComponentSolverCreators;
    private static readonly Dictionary<string, string> ModComponentSolverHelpMessages;
    private static readonly Dictionary<string, string> ModComponentSolverManualCodes;

    private enum ModCommandType
    {
        Simple,
        Coroutine
    }

    static ComponentSolverFactory()
    {
        ModComponentSolverCreators = new Dictionary<string, ModComponentSolverDelegate>();
        ModComponentSolverHelpMessages = new Dictionary<string, string>();
        ModComponentSolverManualCodes = new Dictionary<string, string>();

        //AT_Bash Modules
        ModComponentSolverCreators["MotionSense"] = (bombCommander, bombComponent, ircConnection, canceller) => new MotionSenseComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

       //Hexi Modules
        ModComponentSolverCreators["MemoryV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new ForgetMeNotComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["KeypadV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new RoundKeypadComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["ButtonV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SquareButtonComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["SimonV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SimonStatesComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["PasswordV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new SafetySafeComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["NeedyVentV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new NeedyQuizComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
		ModComponentSolverCreators["NeedyKnobV2"] = (bombCommander, bombComponent, ircConnection, canceller) => new NeedyRotaryPhoneComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

        //Perky Modules  (Silly Slots is maintained by Timwi, and as such its handler lives there.)
        ModComponentSolverCreators["CrazyTalk"] = (bombCommander, bombComponent, ircConnection, canceller) => new CrazyTalkComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["CryptModule"] = (bombCommander, bombComponent, ircConnection, canceller) => new CryptographyComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["ForeignExchangeRates"] = (bombCommander, bombComponent, ircConnection, canceller) => new ForeignExchangeRatesComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["Listening"] = (bombCommander, bombComponent, ircConnection, canceller) => new ListeningComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["OrientationCube"] = (bombCommander, bombComponent, ircConnection, canceller) => new OrientationCubeComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["Probing"] = (bombCommander, bombComponent, ircConnection, canceller) => new ProbingComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["TurnTheKey"] = (bombCommander, bombComponent, ircConnection, canceller) => new TurnTheKeyComponentSolver(bombCommander, bombComponent, ircConnection, canceller);
        ModComponentSolverCreators["TurnTheKeyAdvanced"] = (bombCommander, bombComponent, ircConnection, canceller) => new TurnTheKeyAdvancedComponentSolver(bombCommander, bombComponent, ircConnection, canceller);

        //Kaneb Modules
        ModComponentSolverCreators["TwoBits"] = (bombCommander, bombComponent, ircConnection, canceller) => new TwoBitsComponentSolver(bombCommander, bombComponent, ircConnection, canceller);


        //Help Messages
        ModComponentSolverHelpMessages["AdjacentLettersModule"] = "Set the Letters with !{0} set W D J S.  (warning, this will unset ALL letters not specified.)  Submit your answer with !{0} submit.";
        ModComponentSolverHelpMessages["spwizAdventureGame"] = "Cycle the stats with !{0} cycle stats.  Cycle the Weapons/Items with !{0} cycle items. Use weapons/Items with !{0} use potion. (spell out the item name completely. not case sensitive)";
        ModComponentSolverHelpMessages["spwizAstrology"] = "Press good on 3 with !{0} press good on 3.  Press bad on 2 with !{0} press bad on 2. No Omen is !{0} press no";
        ModComponentSolverHelpMessages["BattleshipModule"] = "Scan the safe spots with !{0} scan A2 B3 E5. Mark the spots as water with !{0} miss A1 A3 B4.  Mark the spots as ships with !{0} hit E3 E4. Fill in the rows with !{0} row 3 4. Fill in columns with !{0} col B D";
        ModComponentSolverHelpMessages["BitmapsModule"] = "Submit the correct answer with !{0} press 2.";
        ModComponentSolverHelpMessages["BlindAlleyModule"] = "Hit the correct spots with !{0} press bl mm tm tl.  (Locations are tl, tm, ml, mm, mr, bl, bm, br)";
        ModComponentSolverHelpMessages["BrokenButtonsModule"] = "Press the button by name with !{0} press \"this\".  Press the button in column 2 row 3 with !{0} press 2 3. Press the right submit button with !{0} submit right.";
        ModComponentSolverHelpMessages["CaesarCipherModule"] = "Press the correct cipher text with !{0} press K B Q I S.";
        ModComponentSolverHelpMessages["CheapCheckoutModule"] = "Cycle the items with !{0} items. Get customers to pay the correct amount with !{0} submit.  Return the proper change with !{0} submit 3.24.";
        ModComponentSolverHelpMessages["ChessModule"] = "Cycle the positions with !{0} cycle.  Submit the safe spot with !{0} press C2.";
        ModComponentSolverHelpMessages["colormath"] = "Set the correct number with !{0} set a,k,m,y.  Submit your set answer with !{0} submit. colors are Red, Orange, Yellow, Green, Blue, Purple, Magenta, White, grAy, blackK. (note what letter is capitalized in each color.)";
        ModComponentSolverHelpMessages["ColoredSquaresModule"] = "Press the desired squares with !{0} red, !{0} green, !{0} blue, !{0} yellow, !{0} magenta, !{0} row, or !{0} col.";
        ModComponentSolverHelpMessages["ColourFlash"] = "Submit the correct response with !{0} press yes 3, or !{0} press no 5.";
        ModComponentSolverHelpMessages["CoordinatesModule"] = "Cycle the options with !{0} cycle.  Submit your answer with !{0} submit <3,2>.  Partial answers are acceptable. To do chinese numbers, its !{0} submit chinese 12.";
        //ModComponentSolverHelpMessages["CreationModule"] = "";
        ModComponentSolverHelpMessages["DoubleOhModule"] = "Cycle the buttons with !{0} cycle. (Cycle presses each button 3 times, in the order of vert1, horiz1, horiz2, vert2, submit.)  Submit your answer with !{0} press vert1 horiz1 horiz2 vert2 submit.";
        ModComponentSolverHelpMessages["FollowTheLeaderModule"] = "Cut the wires in the order specified with !{0} cut 12 10 8 7 6 5 3 1. (note that order was the Lit CLR rule.)";
        ModComponentSolverHelpMessages["FriendshipModule"] = "Submit the desired friendship element with !{0} submit Fairness Conscientiousness Kindness Authenticity.";
        ModComponentSolverHelpMessages["HexamazeModule"] = "Move towards the exit with !{0} move 12 10 6 6 6 2, or with !{0} move N NW S S S NE.  (clockface or cardinal)";
        ModComponentSolverHelpMessages["Laundry"] = "Set all of the options with !{0} set all 30C,2 dot,110C,Wet Cleaning.  Set just washing with !{0} set wash 40C.  Submit with !{0} insert coin. Prey for that 4 in 2 + lit bob. Kappa";
        ModComponentSolverHelpMessages["LightCycleModule"] = "Submit your answer with !{0} B R W M G Y. (note, this module WILL try to input any answer you put into it. Don't do !{0} claim or !{0} mine here.)";
        ModComponentSolverHelpMessages["Logic"] = "Logic is answered with !{0} submit F T.";
        ModComponentSolverHelpMessages["ModuleAgainstHumanity"] = "Reset the module with !{0} press reset.  Move the black card +2 with !{0} move black 2.  Move the white card -3 with !{0} move white -3. Submit with !{0} press submit.";
        ModComponentSolverHelpMessages["MysticSquareModule"] = "Move the numbers around with !{0} press 1 3 2 1 3 4 6 8.  (Note, this module will NOT stop moving numbers around on a strike. Make sure that knight gets uncovered asap.)";
        ModComponentSolverHelpMessages["neutralization"] = "Turn the filter on/off with !{0} filter. Move to the next base with !{0} base next.  Move to previous base with !{0} base prev. Set drop count with !{0} set conc 48. Submit with !{0} titrate.";
        //ModComponentSolverHelpMessages["OnlyConnectModule"] = "";
        ModComponentSolverHelpMessages["PianoKeys"] = "Submit your answer with !{0} press Bb Bb Bb Bb Gb Ab Bb Ab Bb.";
        ModComponentSolverHelpMessages["RockPaperScissorsLizardSpockModule"] = "Submit your answer with !{0} press scissors lizard.";
        ModComponentSolverHelpMessages["Semaphore"] = "Move to the next flag with !{0} move right or !{0} press right. Move to previous flag with !{0} move left or !{0} press left.  Submit with !{0} press ok.";
        ModComponentSolverHelpMessages["SillySlots"] = "Keep the slots with !{0} keep.  Pull the slots with !{0} pull.";
        ModComponentSolverHelpMessages["SimonScreamsModule"] = "Press the correct colors for each round with !{0} press B O Y.";
        //ModComponentSolverHelpMessages["SkewedSlotsModule"] = "";
        //ModComponentSolverHelpMessages["SouvenirModule"] = "";
        ModComponentSolverHelpMessages["TextField"] = "Press the button in Row 2 column 3 and Row 3 Column 4 with !{0} press 3,2 4,3.";
        ModComponentSolverHelpMessages["TheBulbModule"] = "Press O with !{0} press O.  Press I with !{0} press I. Unscrew the bulb with !{0} unscrew.  Screw in the bulb with !{0} screw.";
        //ModComponentSolverHelpMessages["TheGamepadModule"] = "";
        //ModComponentSolverHelpMessages["ThirdBase"] = "";
        //ModComponentSolverHelpMessages["TicTacToeModule"] = "";
        ModComponentSolverHelpMessages["webDesign"] = "Accept the design with !{0} acc.  Consider the design with !{0} con.  Reject the design with !{0} reject.";
        ModComponentSolverHelpMessages["WirePlacementModule"] = "Cut the correct wires with !{0} cut A2 B4 D3.";
        //ModComponentSolverHelpMessages["WordSearchModule"] = "";
        //ModComponentSolverHelpMessages["http"] = "";
        //ModComponentSolverHelpMessages["LightsOut"] = "";

        //Manual Codes
        ModComponentSolverManualCodes["ColourFlash"] = "Color Flash";

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

        ModComponentSolverDelegate modComponentSolverCreator = GenerateModComponentSolverCreator(bombComponent, moduleType);
        if (modComponentSolverCreator == null)
        {
            throw new NotSupportedException(string.Format("Currently {0} is not supported by 'Twitch Plays' - Could not generate a valid componentsolver for the mod component!", (string)CommonReflectedTypeInfo.ModuleDisplayNameField.Invoke(bombComponent, null)));
        }

        ModComponentSolverCreators[moduleType] = modComponentSolverCreator;

        return modComponentSolverCreator(bombCommander, bombComponent, ircConnection, canceller);
    }

    private static ModComponentSolverDelegate GenerateModComponentSolverCreator(MonoBehaviour bombComponent, string moduleType)
    {
        ModCommandType commandType = ModCommandType.Simple;
        Type commandComponentType = null;
        MethodInfo method = FindProcessCommandMethod(bombComponent, out commandType, out commandComponentType);
        string help = FindHelpMessage(bombComponent);
        string manual = FindManualCode(bombComponent);

        if (help == null && ModComponentSolverHelpMessages.ContainsKey(moduleType))
            help = ModComponentSolverHelpMessages[moduleType];

        if (manual == null && ModComponentSolverManualCodes.ContainsKey(moduleType))
            manual = ModComponentSolverManualCodes[moduleType];

        if (method != null)
        {
            switch (commandType)
            {
                case ModCommandType.Simple:
                    return delegate (BombCommander _bombCommander, MonoBehaviour _bombComponent, IRCConnection _ircConnection, CoroutineCanceller _canceller)
                    {
                        Component commandComponent = _bombComponent.GetComponentInChildren(commandComponentType);
                        return new SimpleModComponentSolver(_bombCommander, _bombComponent, _ircConnection, _canceller, method, commandComponent, manual, help);
                    };
                case ModCommandType.Coroutine:
                    FieldInfo cancelfield;
                    Type canceltype;
                    FindCancelBool(bombComponent, out cancelfield, out canceltype);
                    return delegate (BombCommander _bombCommander, MonoBehaviour _bombComponent, IRCConnection _ircConnection, CoroutineCanceller _canceller)
                    {
                        Component commandComponent = _bombComponent.GetComponentInChildren(commandComponentType);
                        return new CoroutineModComponentSolver(_bombCommander, _bombComponent, _ircConnection, _canceller, method, commandComponent, manual, help, cancelfield, canceltype);
                    };

                default:
                    break;
            }
        }

        return null;
    }

    private static string FindManualCode(MonoBehaviour bombComponent)
    {
        Component[] allComponents = bombComponent.GetComponentsInChildren<Component>(true);
        foreach (Component component in allComponents)
        {
            Type type = component.GetType();
            FieldInfo candidateString = type.GetField("TwitchManualCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (candidateString == null)
            {
                continue;
            }
            if (candidateString.GetValue(bombComponent.GetComponent(type)) is string)
                return (string)candidateString.GetValue(bombComponent.GetComponent(type));
        }
        return null;
    }

    private static string FindHelpMessage(MonoBehaviour bombComponent)
    {
        Component[] allComponents = bombComponent.GetComponentsInChildren<Component>(true);
        foreach (Component component in allComponents)
        {
            Type type = component.GetType();
            FieldInfo candidateString = type.GetField("TwitchHelpMessage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (candidateString == null)
            {
                continue;
            }
            if (candidateString.GetValue(bombComponent.GetComponent(type)) is string)
                return (string)candidateString.GetValue(bombComponent.GetComponent(type));
        }
        return null;
    }

    private static bool FindCancelBool(MonoBehaviour bombComponent, out FieldInfo CancelField, out Type CancelType)
    {
        Component[] allComponents = bombComponent.GetComponentsInChildren<Component>(true);
        foreach (Component component in allComponents)
        {
            Type type = component.GetType();
            FieldInfo candidateBoolField = type.GetField("TwitchShouldCancelCommand", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (candidateBoolField == null)
            {
                continue;
            }
            if (candidateBoolField.GetValue(bombComponent.GetComponent(type)) is bool)
            {
                CancelField = candidateBoolField;
                CancelType = type;
                return true;
            }
        }
        CancelField = null;
        CancelType = null;
        return false;
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

