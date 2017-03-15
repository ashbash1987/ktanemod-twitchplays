using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class MorseCodeComponentSolver : ComponentSolver
{
    public MorseCodeComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent, IRCConnection ircConnection) :
        base(bomb, bombComponent, ircConnection)
    {
        _upButton = (MonoBehaviour)_upButtonField.GetValue(bombComponent);
        _downButton = (MonoBehaviour)_downButtonField.GetValue(bombComponent);
        _transmitButton = (MonoBehaviour)_transmitButtonField.GetValue(bombComponent);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        string[] commandParts = inputCommand.Split(' ');

        if (commandParts.Length != 2)
        {
            yield break;
        }

        if (!commandParts[0].Equals("transmit", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }

        int targetFrequency = 0;
        if (!int.TryParse(commandParts[1], out targetFrequency))
        {
            yield break;
        }

        if (!Frequencies.Contains(targetFrequency))
        {
            yield break;
        }

        int initialFrequency = CurrentFrequency;
        MonoBehaviour buttonToShift = targetFrequency < initialFrequency ? _downButton : _upButton;

        while (CurrentFrequency != targetFrequency && Mathf.Sign(CurrentFrequency - initialFrequency) != Mathf.Sign(CurrentFrequency - targetFrequency))
        {
            yield return "change frequency";

            DoInteractionStart(buttonToShift);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(buttonToShift);
        }

        if (CurrentFrequency == targetFrequency)
        {
            yield return "transmit";

            DoInteractionStart(_transmitButton);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_transmitButton);
        }
    }    

    private int CurrentFrequency
    {
        get
        {
            return (int)_currentFrequencyProperty.GetValue(BombComponent, null);
        }
    }

    static MorseCodeComponentSolver()
    {
        _morseCodeComponentType = ReflectionHelper.FindType("MorseCodeComponent");
        _upButtonField = _morseCodeComponentType.GetField("UpButton", BindingFlags.Public | BindingFlags.Instance);
        _downButtonField = _morseCodeComponentType.GetField("DownButton", BindingFlags.Public | BindingFlags.Instance);
        _transmitButtonField = _morseCodeComponentType.GetField("TransmitButton", BindingFlags.Public | BindingFlags.Instance);
        _currentFrequencyProperty = _morseCodeComponentType.GetProperty("CurrentFrequency", BindingFlags.Public | BindingFlags.Instance);
    }

    private static readonly int[] Frequencies = new int[]
    {
        502,
        505,
        512,
        515,
        522,
        525,
        532,
        535,
        542,
        545,
        552,
        555,
        562,
        565,
        572,
        575,
        582,
        585,
        592,
        595,
        600
    };

    private static Type _morseCodeComponentType = null;
    private static FieldInfo _upButtonField = null;
    private static FieldInfo _downButtonField = null;
    private static FieldInfo _transmitButtonField = null;
    private static PropertyInfo _currentFrequencyProperty = null;

    private MonoBehaviour _upButton = null;
    private MonoBehaviour _downButton = null;
    private MonoBehaviour _transmitButton = null;
}
