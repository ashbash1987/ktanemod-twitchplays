using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SafetySafeComponentSolver : ComponentSolver
{
    private static string[] DialPosNames = {"TL", "TM", "TR", "BL", "BM", "BR"};

    public SafetySafeComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _buttons = (Array)_buttonsField.GetValue(bombComponent.GetComponent(_componentType));
        _lever = (MonoBehaviour)_leverField.GetValue(bombComponent.GetComponent(_componentType));
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (inputCommand.Equals("submit"))
        {
            yield return "submit";
            DoInteractionStart(_lever);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_lever);
            yield break;
        }
        for(int a = 0; a < DialPosNames.Length; a++)
        {
            string id = DialPosNames[a];
            if (inputCommand.StartsWith(id, StringComparison.InvariantCultureIgnoreCase))
            {
                string after = inputCommand.Substring(id.Length);
                if (after.Length == 0)
                {
                    IEnumerator coroutine = HandlePress(a);
                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current;
                    }
                }
                else
                {
                    int val = 0;
                    if(!int.TryParse(inputCommand.Substring(2), out val)) yield break;
                    if(val < 0) yield break;
                    for(int z = 0; z < val; z++)
                    {
                        IEnumerator coroutine = HandlePress(a);
                        while (coroutine.MoveNext())
                        {
                            yield return coroutine.Current;
                        }
                        if (Canceller.ShouldCancel)
                        {
                            Canceller.ResetCancel();
                            yield break;
                        }
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                yield break;
            }
        }

        string[] values = inputCommand.Split(new string[]{" "}, 99, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 6) yield break;

        for (int a = 0; a < 6; a++)
        {
            int val = 0;
            if(!int.TryParse(values[a], out val)) yield break;
            if(val < 0) yield break;
            for(int z = 0; z < val; z++)
            {
                IEnumerator coroutine = HandlePress(a);
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
                if (Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    yield break;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private IEnumerator HandlePress(int pos)
    {
        MonoBehaviour button = (MonoBehaviour)_buttons.GetValue(pos);
        DoInteractionStart(button);
        yield return new WaitForSeconds(0.1f);
        DoInteractionEnd(button);
    }

    static SafetySafeComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("AdvancedPassword");
        _buttonsField = _componentType.GetField("Dials", BindingFlags.NonPublic | BindingFlags.Instance);
        _leverField = _componentType.GetField("Lever", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _buttonsField, _leverField = null;

    private Array _buttons = null;
    private MonoBehaviour _lever = null;
}
