﻿//ListeningComponentSolver
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ListeningComponentSolver : ComponentSolver
{
    public ListeningComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _buttons = new MonoBehaviour[4];
        _play = (MonoBehaviour)_playField.GetValue(bombComponent.GetComponent(_componentType));
        _buttons[0] = (MonoBehaviour)_dollarField.GetValue(bombComponent.GetComponent(_componentType));
        _buttons[1] = (MonoBehaviour)_poundField.GetValue(bombComponent.GetComponent(_componentType));
        _buttons[2] = (MonoBehaviour)_starField.GetValue(bombComponent.GetComponent(_componentType));
        _buttons[3] = (MonoBehaviour)_ampersandField.GetValue(bombComponent.GetComponent(_componentType));
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        MonoBehaviour button;

        var split = inputCommand.Trim().ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 2 || split.Length > 6 || split[0] != "press")
            yield break;

        if (_play == null || _buttons[0] == null || _buttons[1] == null || _buttons[2] == null || _buttons[3] == null)
            yield break;

        foreach (var cmd in split.Skip(1))
            switch (cmd)
            {
                case "play": if (split.Length > 2) yield break; break;
                case "$": case "#": case "*": case "&": if (split.Length != 6) yield break; break;
                default:
                    if (cmd.Length != 5 || split.Length != 2) yield break;
                    foreach(var x in cmd)
                        if (!"$#*&".Contains(x))
                            yield break;
                    break;
            }   //Check for any invalid commands.  Abort entire sequence if any invalid commands are present.

        yield return "Listening Solve Attempt";
        foreach (var cmd in split.Skip(1))
        {
            switch (cmd)
            {
                case "play": button = _play; break;

                case "$": case "#": case "*": case "&": button = _buttons["$#*&".IndexOf(cmd, StringComparison.Ordinal)]; break;

                default:
                    foreach (var x in cmd)
                    {
                        button = _buttons["$#*&".IndexOf(x)];
                        DoInteractionStart(button);
                        yield return new WaitForSeconds(0.1f);
                        DoInteractionEnd(button);
                    }
                    yield break;
            }

            DoInteractionStart(button);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(button);
        }
    }

    static ListeningComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("Listening");
        _playField = _componentType.GetField("PlayButton", BindingFlags.Public | BindingFlags.Instance);
        _dollarField = _componentType.GetField("DollarButton", BindingFlags.Public | BindingFlags.Instance);
        _poundField = _componentType.GetField("PoundButton", BindingFlags.Public | BindingFlags.Instance);
        _starField = _componentType.GetField("StarButton", BindingFlags.Public | BindingFlags.Instance);
        _ampersandField = _componentType.GetField("AmpersandButton", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _playField = null;
    private static FieldInfo _dollarField = null;
    private static FieldInfo _poundField = null;
    private static FieldInfo _starField = null;
    private static FieldInfo _ampersandField = null;

    private MonoBehaviour _play = null;
    private MonoBehaviour[] _buttons = null;
}