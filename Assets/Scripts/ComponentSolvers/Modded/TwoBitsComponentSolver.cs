using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TwoBitsComponentSolver : ComponentSolver
{
    private Component c;

    private const string ButtonLabels = "bcdegkptvz";

    public TwoBitsComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        c = bombComponent.GetComponent(_componentSolverType);

        _submit = (MonoBehaviour)_submitButtonField.GetValue(c);
        _query = (MonoBehaviour)_queryButtonField.GetValue(c);
        _buttons = (MonoBehaviour[])_buttonsField.GetValue(c);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        var split = inputCommand.ToLowerInvariant().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        int beforeTwoBitsStrikeCount = StrikeCount;
        string correctresponse = ((string)_calculateCorrectSubmissionMethod.Invoke(c, null)).ToLowerInvariant();


        if (split[0] != "query" && split[0] != "submit")
        {
            yield break;
        }
        foreach (var x in split.Skip(1))
        {
            foreach (var y in x)
            {
                if (!ButtonLabels.Contains(y))
                {
                    yield break;
                }
            }
        }

        yield return "TwoBits Solve Attempt";
        foreach (var x in split.Skip(1))
        {
            foreach (var y in x)
            {
                yield return HandlePress(y);
                /*if (beforeTwoBitsStrikeCount != StrikeCount)
                {
                    yield break;
                }*/
                yield return "trycancel";
            }
        }

        if (split[0] == "query")
        {
            DoInteractionStart(_query);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_query);
        }
        else
        {
            string currentQuery = ((string)_getCurrentQueryStringMethod.Invoke(c, null)).ToLowerInvariant();
            yield return correctresponse.Equals(currentQuery) ? "solve" : "strike";
            DoInteractionStart(_submit);
            yield return new WaitForSeconds(0.1f);
            DoInteractionEnd(_submit);
        }
    }

    private IEnumerator HandlePress(char c)
    {
        var pos = ButtonLabels.IndexOf(c);
        DoInteractionStart(_buttons[pos]);
        yield return new WaitForSeconds(0.1f);
        DoInteractionEnd(_buttons[pos]);
    }

    static TwoBitsComponentSolver()
    {
        _componentSolverType = ReflectionHelper.FindType("TwoBitsModule");
        _submitButtonField = _componentSolverType.GetField("SubmitButton", BindingFlags.Public | BindingFlags.Instance);
        _queryButtonField = _componentSolverType.GetField("QueryButton", BindingFlags.Public | BindingFlags.Instance);
        _buttonsField = _componentSolverType.GetField("Buttons", BindingFlags.Public | BindingFlags.Instance);
        _calculateCorrectSubmissionMethod = _componentSolverType.GetMethod("CalculateCorrectSubmission",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _getCurrentQueryStringMethod = _componentSolverType.GetMethod("GetCurrentQueryString",
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static Type _componentSolverType = null;
    private static FieldInfo _submitButtonField = null;
    private static FieldInfo _queryButtonField = null;
    private static FieldInfo _buttonsField = null;
    private static MethodInfo _calculateCorrectSubmissionMethod = null;
    private static MethodInfo _getCurrentQueryStringMethod = null;


    private MonoBehaviour[] _buttons = null;
    private MonoBehaviour _query = null;
    private MonoBehaviour _submit = null;
}
