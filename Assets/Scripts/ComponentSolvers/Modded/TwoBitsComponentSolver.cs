using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class TwoBitsComponentSolver : ComponentSolver
{
    private Component c;

    private string pressedQuery;

    public TwoBitsComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        c = bombComponent.GetComponent(_componentSolverType);

        _submit = (MonoBehaviour)_submitButtonField.GetValue(c);
        _query = (MonoBehaviour)_queryButtonField.GetValue(c);
        _buttons = (Array)_buttonsField.GetValue(c);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        int beforeTwoBitsStrikeCount = StrikeCount;
        string correctresponse = (string)_calculateCorrectSubmissionMethod.Invoke(c, null);
        pressedQuery = string.Empty;

        if (inputCommand.StartsWith("query ", StringComparison.InvariantCultureIgnoreCase))
        {
            var querystring = inputCommand.Substring(5).ToLowerInvariant();
            foreach (var q in querystring)
            {
                yield return HandlePress(q);
                if (beforeTwoBitsStrikeCount != StrikeCount) break;
            }
            if (beforeTwoBitsStrikeCount != StrikeCount) yield break;
            yield return HandleQuery();
        }
        else if (inputCommand.StartsWith("submit ", StringComparison.InvariantCultureIgnoreCase))
        {
            var querystring = inputCommand.Substring(6).ToLowerInvariant();
            foreach (var q in querystring)
            {
                yield return HandlePress(q);
                if (beforeTwoBitsStrikeCount != StrikeCount) break;
            }
            if (beforeTwoBitsStrikeCount != StrikeCount) yield break;
            yield return correctresponse.Equals(pressedQuery) ? "solve" : "strike";
            yield return HandleSubmit();
        }
    }

    private IEnumerator HandleQuery()
    {
        yield return "query";
        DoInteractionStart(_query);
        yield return new WaitForSeconds(0.1f);
        DoInteractionEnd(_query);
    }

    private IEnumerator HandleSubmit()
    {
        yield return "submit";
        DoInteractionStart(_submit);
        yield return new WaitForSeconds(0.1f);
        DoInteractionEnd(_submit);
    }

    private IEnumerator HandlePress(char c)
    {
        var buttonLabels = "bcdegkptvz";
        var pos = buttonLabels.IndexOf(c);
        if (pos < 0) yield break;
        pressedQuery += c;

        yield return c;
        MonoBehaviour button = (MonoBehaviour)_buttons.GetValue(pos);
        DoInteractionStart(button);
        yield return new WaitForSeconds(0.1f);
        DoInteractionEnd(button);
    }

    static TwoBitsComponentSolver()
    {
        _componentSolverType = ReflectionHelper.FindType("TwoBitsModule");
        _submitButtonField = _componentSolverType.GetField("SubmitButton", BindingFlags.Public | BindingFlags.Instance);
        _queryButtonField = _componentSolverType.GetField("QueryButton", BindingFlags.Public | BindingFlags.Instance);
        _buttonsField = _componentSolverType.GetField("Buttons", BindingFlags.Public | BindingFlags.Instance);
        _calculateCorrectSubmissionMethod = _componentSolverType.GetMethod("CalculateCorrectSubmission",
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static Type _componentSolverType = null;
    private static FieldInfo _submitButtonField = null;
    private static FieldInfo _queryButtonField = null;
    private static FieldInfo _buttonsField = null;
    private static MethodInfo _calculateCorrectSubmissionMethod = null;


    private Array _buttons = null;
    private MonoBehaviour _query = null;
    private MonoBehaviour _submit = null;
}
