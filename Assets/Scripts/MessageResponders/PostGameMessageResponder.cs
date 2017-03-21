using System.Collections;
using UnityEngine;

public class PostGameMessageResponder : MessageResponder
{
    private PostGameCommander _postGameCommander = null;

    #region Unity Lifecycle
    private void OnEnable()
    {
        StartCoroutine(CheckForResultsPage());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        _postGameCommander = null;
    }
    #endregion

    #region Protected/Private Methods
    protected override void OnMessageReceived(string userNickName, string userColorCode, string text)
    {
        if (_postGameCommander != null)
        {
            _coroutineQueue.AddToQueue(_postGameCommander.RespondToCommand(userNickName, text, null));
        }
    }

    private IEnumerator CheckForResultsPage()
    {
        yield return null;

        while (true)
        {
            UnityEngine.Object[] resultPages = FindObjectsOfType(CommonReflectedTypeInfo.ResultPageType);
            if (resultPages != null && resultPages.Length > 0)
            {
                MonoBehaviour resultPageBehaviour = (MonoBehaviour)resultPages[0];
                _postGameCommander = new PostGameCommander(resultPageBehaviour);
                break;
            }

            yield return null;
        }
    }
    #endregion
}
