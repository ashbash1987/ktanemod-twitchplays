using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class MissionMessageResponder : MessageResponder
{
    private BombBinderCommander _bombBinderCommander = null;

    #region Unity Lifecycle
    private void OnEnable()
    {
        InputInterceptor.DisableInput();

        StartCoroutine(CheckForBombBinder());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        _bombBinderCommander = null;
    }
    #endregion

    #region Protected/Private Methods
    private IEnumerator CheckForBombBinder()
    {
        yield return null;

        while (true)
        {
            UnityEngine.Object[] bombBinders = FindObjectsOfType(CommonReflectedTypeInfo.BombBinderType);
            if (bombBinders != null && bombBinders.Length > 0)
            {
                _bombBinderCommander = new BombBinderCommander((MonoBehaviour)bombBinders[0]);
                break;
            }

            yield return null;
        }
    }

    protected override void OnMessageReceived(string userNickName, string userColorCode, string text)
    {
        if (_bombBinderCommander == null)
        {
            return;
        }

        Match match = Regex.Match(text, "^!binder (.+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return;
        }

        _coroutineQueue.AddToQueue(_bombBinderCommander.RespondToCommand(userNickName, match.Groups[1].Value, null));
    }
    #endregion
}
