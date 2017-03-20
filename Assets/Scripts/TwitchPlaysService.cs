using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class TwitchPlaysService : MonoBehaviour
{
    public class ModSettingsJSON
    {
        public string authToken;
        public string userName;
        public string channelName;
        public string serverName;
        public int serverPort;
    }

    public TwitchComponentHandle twitchComponentHandlePrefab = null;
    public TwitchBombHandle twitchBombHandlePrefab = null;

    private KMGameInfo _gameInfo = null;
    private KMModSettings _modSettings = null;
    private IRCConnection _ircConnection = null;
    private CoroutineQueue _coroutineQueue = null;
    private BombCommander _bombCommander = null;
    private PostGameCommander _postGameCommander = null;

    private CoroutineCanceller _coroutineCanceller = null;

    private void Start()
    {
        _gameInfo = GetComponent<KMGameInfo>();
        _gameInfo.OnStateChange += OnStateChange;

        _modSettings = GetComponent<KMModSettings>();

        ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(_modSettings.Settings);
        if (settings == null)
        {
            Debug.LogError("[TwitchPlays] Failed to read connection settings from mod settings.");
            return;
        }

        _ircConnection = new IRCConnection(settings.authToken, settings.userName, settings.channelName, settings.serverName, settings.serverPort);
        _ircConnection.Connect();
        _ircConnection.OnMessageReceived.AddListener(OnMessageReceived);

        _coroutineQueue = GetComponent<CoroutineQueue>();
        _coroutineCanceller = new CoroutineCanceller();
    }

    private void Update()
    {
        if (_ircConnection != null)
        {
            _ircConnection.Update();
        }
    }

    private void OnDestroy()
    {
        if (_ircConnection != null)
        {
            _ircConnection.Disconnect();
            _ircConnection.OnMessageReceived.RemoveListener(OnMessageReceived);
        }
    }

    private void OnStateChange(KMGameInfo.State state)
    {
        if (_ircConnection == null)
        {
            return;
        }

        StopEveryCoroutine();

        switch (state)
        {
            case KMGameInfo.State.Gameplay:
                _postGameCommander = null;
                StartCoroutine(CheckForBombs());
                break;

            case KMGameInfo.State.PostGame:
                _bombCommander = null;
                DestroyBombHandles();
                StartCoroutine(CheckForResultsPage());
                break;

            case KMGameInfo.State.Setup:
                _bombCommander = null;
                _postGameCommander = null;
                DestroyBombHandles();
                break;

            default:
                break;
        }
    }

    private void DestroyBombHandles()
    {
        foreach (TwitchBombHandle bombHandle in FindObjectsOfType<TwitchBombHandle>())
        {
            Destroy(bombHandle.gameObject);
        }
    }

    private void StopEveryCoroutine()
    {
        StopAllCoroutines();
        _coroutineQueue.StopQueue();
        _coroutineQueue.CancelFutureSubcoroutines();
    }

    private IEnumerator CheckForBombs()
    {
        bool foundComponents = false;
        yield return null;

        while (!foundComponents)
        {
            UnityEngine.Object[] bombs = FindObjectsOfType(CommonReflectedTypeInfo.BombType);
            foreach (UnityEngine.Object bomb in bombs)
            {
                MonoBehaviour bombBehaviour = (MonoBehaviour)bomb;
                _bombCommander = new BombCommander(bombBehaviour);

                CreateBombHandleForBomb(bombBehaviour);

                if (CreateComponentHandlesForBomb(bombBehaviour))
                {
                    foundComponents = true;
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator CheckForResultsPage()
    {
        bool foundResultsPage = false;
        yield return null;

        while (!foundResultsPage)
        {
            UnityEngine.Object[] resultPages = FindObjectsOfType(CommonReflectedTypeInfo.ResultPageType);
            foreach (UnityEngine.Object resultPage in resultPages)
            {
                MonoBehaviour resultPageBehaviour = (MonoBehaviour)resultPage;
                _postGameCommander = new PostGameCommander(resultPageBehaviour);
                foundResultsPage = true;
                break;
            }

            yield return null;
        }
    }

    private void CreateBombHandleForBomb(MonoBehaviour bomb)
    {
        TwitchBombHandle handle = Instantiate<TwitchBombHandle>(twitchBombHandlePrefab);
        handle.ircConnection = _ircConnection;
        handle.bombCommander = _bombCommander;
        handle.coroutineQueue = _coroutineQueue;
        handle.coroutineCanceller = _coroutineCanceller;
    }

    private bool CreateComponentHandlesForBomb(MonoBehaviour bomb)
    {
        bool foundComponents = false;

        IList bombComponents = (IList)CommonReflectedTypeInfo.BombComponentsField.GetValue(bomb);

        foreach (MonoBehaviour bombComponent in bombComponents)
        {
            object componentType = CommonReflectedTypeInfo.ComponentTypeField.GetValue(bombComponent);
            int componentTypeInt = (int)Convert.ChangeType(componentType, typeof(int));
            ComponentTypeEnum componentTypeEnum = (ComponentTypeEnum)componentTypeInt;

            switch (componentTypeEnum)
            {
                case ComponentTypeEnum.Empty:
                case ComponentTypeEnum.Timer:
                    continue;

                default:
                    foundComponents = true;
                    break;
            }

            TwitchComponentHandle handle = (TwitchComponentHandle)Instantiate(twitchComponentHandlePrefab, bombComponent.transform, false);
            handle.ircConnection = _ircConnection;
            handle.bombCommander = _bombCommander;
            handle.bombComponent = bombComponent;
            handle.componentType = componentTypeEnum;
            handle.coroutineQueue = _coroutineQueue;
            handle.coroutineCanceller = _coroutineCanceller;

            Vector3 idealOffset = handle.transform.TransformDirection(GetIdealPositionForHandle(handle, bombComponents, out handle.direction));
            handle.transform.SetParent(bombComponent.transform.parent, true);
            handle.basePosition = handle.transform.localPosition;
            handle.idealHandlePositionOffset = bombComponent.transform.parent.InverseTransformDirection(idealOffset);
        }

        return foundComponents;
    }

    private Vector3 GetIdealPositionForHandle(TwitchComponentHandle thisHandle, IList bombComponents, out TwitchComponentHandle.Direction direction)
    {
        Rect handleBasicRect = new Rect(-0.155f, -0.1f, 0.31f, 0.2f);
        Rect bombComponentBasicRect = new Rect(-0.1f, -0.1f, 0.2f, 0.2f);

        float baseUp = (handleBasicRect.height + bombComponentBasicRect.height) * 0.55f;
        float baseRight = (handleBasicRect.width + bombComponentBasicRect.width) * 0.55f;

        Vector2 extentUp = new Vector2(0.0f, baseUp * 0.1f);
        Vector2 extentRight = new Vector2(baseRight * 0.2f, 0.0f);

        Vector2 extentResult = Vector2.zero;

        while (true)
        {
            Rect handleRect = handleBasicRect;
            handleRect.position += extentRight;
            if (!HasOverlap(thisHandle, handleRect, bombComponentBasicRect, bombComponents))
            {
                extentResult = extentRight;
                direction = TwitchComponentHandle.Direction.Left;
                break;
            }

            handleRect = handleBasicRect;
            handleRect.position -= extentRight;
            if (!HasOverlap(thisHandle, handleRect, bombComponentBasicRect, bombComponents))
            {
                extentResult = -extentRight;
                direction = TwitchComponentHandle.Direction.Right;
                break;
            }

            handleRect = handleBasicRect;
            handleRect.position += extentUp;
            if (!HasOverlap(thisHandle, handleRect, bombComponentBasicRect, bombComponents))
            {
                extentResult = extentUp;
                direction = TwitchComponentHandle.Direction.Down;
                break;
            }

            handleRect = handleBasicRect;
            handleRect.position -= extentUp;
            if (!HasOverlap(thisHandle, handleRect, bombComponentBasicRect, bombComponents))
            {
                extentResult = -extentUp;
                direction = TwitchComponentHandle.Direction.Up;
                break;
            }

            extentUp.y += baseUp * 0.1f;
            extentRight.x += baseRight * 0.1f;
        }

        return new Vector3(extentResult.x, 0.0f, extentResult.y);
    }

    private bool HasOverlap(TwitchComponentHandle thisHandle, Rect handleRect, Rect bombComponentBasicRect, IList bombComponents)
    {
        foreach (MonoBehaviour bombComponent in bombComponents)
        {
            Vector3 bombComponentCenter = thisHandle.transform.InverseTransformPoint(bombComponent.transform.position);

            Rect bombComponentRect = bombComponentBasicRect;
            bombComponentRect.position += new Vector2(bombComponentCenter.x, bombComponentCenter.z);

            if (bombComponentRect.Overlaps(handleRect))
            {
                return true;
            }
        }

        return false;
    }

    private void OnMessageReceived(string userNickName, string userColor, string text)
    {
        if (CheckForMiscellaneousMessages(userNickName, text))
        {
            return;
        }
    }   

    private bool CheckForMiscellaneousMessages(string userNickName, string text)
    {
        if (_postGameCommander != null)
        {
            _coroutineQueue.AddToQueue(_postGameCommander.RespondToCommand(userNickName, text, null));
        }
        else
        {
            if (text.Equals("!cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                _coroutineCanceller.SetCancel();
                return true;
            }

            if (text.Equals("!stop", StringComparison.InvariantCultureIgnoreCase))
            {
                _coroutineCanceller.SetCancel();
                _coroutineQueue.CancelFutureSubcoroutines();
                return true;
            }
        }

        return false;
    }
}
