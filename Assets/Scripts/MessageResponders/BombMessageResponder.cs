using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombMessageResponder : MessageResponder
{
    public TwitchBombHandle twitchBombHandlePrefab = null;
    public TwitchComponentHandle twitchComponentHandlePrefab = null;
    public Leaderboard leaderboard = null;
    public TwitchPlaysService parentService = null;

    private BombCommander _bombCommander = null;
    private TwitchBombHandle _bombHandle = null;
    private List<TwitchComponentHandle> _componentHandles = new List<TwitchComponentHandle>();

    #region Unity Lifecycle
    private void OnEnable()
    {
        InputInterceptor.DisableInput();

        leaderboard.ClearSolo();
        StartCoroutine(CheckForBomb());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        leaderboard.BombsAttempted++;
        string bombMessage = null;
        if ((bool)CommonReflectedTypeInfo.HasDetonatedProperty.GetValue(_bombCommander.Bomb, null))
        {
            bombMessage = string.Format("KAPOW KAPOW The bomb has exploded, with {0} remaining! KAPOW KAPOW", _bombCommander.CurrentTimerFormatted);
            leaderboard.BombsExploded++;
            leaderboard.Success = false;
        }
        else
        {
            bombMessage = string.Format("PraiseIt PraiseIt The bomb has been defused, with {0} remaining! PraiseIt PraiseIt", _bombCommander.CurrentTimerFormatted);
            leaderboard.BombsCleared++;
            leaderboard.Success = true;
            if (leaderboard.CurrentSolvers.Count == 1)
            {
                float previousRecord = 0.0f;
                float elapsedTime = _bombCommander._bombStartingTimer - _bombCommander.CurrentTimer;
                string userName = "";
                foreach (string uName in leaderboard.CurrentSolvers.Keys)
                {
                    userName = uName;
                    break;
                }
                if (leaderboard.CurrentSolvers[userName] == Leaderboard.RequiredSoloSolves)
                {
                    leaderboard.AddSoloClear(userName, elapsedTime, out previousRecord);
                    TimeSpan elapsedTimeSpan = TimeSpan.FromSeconds(elapsedTime);
                    bombMessage = string.Format("PraiseIt PraiseIt {0} completed a solo defusal in {1}:{2:00}!", leaderboard.SoloSolver.UserName, (int)elapsedTimeSpan.TotalMinutes, elapsedTimeSpan.Seconds);
                    if (elapsedTime < previousRecord)
                    {
                        TimeSpan previousTimeSpan = TimeSpan.FromSeconds(previousRecord);
                        bombMessage += string.Format(" It's a new record! (Previous record: {0}:{1:00})", (int)previousTimeSpan.TotalMinutes, previousTimeSpan.Seconds);
                    }
                    bombMessage += " PraiseIt PraiseIt";
                }
                else
                {
                    leaderboard.ClearSolo();
                }
            }
        }

        parentService.StartCoroutine(SendDelayedMessage(1.0f, bombMessage));

        if (_bombHandle != null)
        {
            Destroy(_bombHandle.gameObject, 2.0f);
        }
        _bombHandle = null;
        _bombCommander = null;

        if (_componentHandles != null)
        {
            foreach (TwitchComponentHandle handle in _componentHandles)
            {
                Destroy(handle.gameObject, 2.0f);
            }
        }
        _componentHandles.Clear();

        InputInterceptor.EnableInput();

        MusicPlayer.StopAllMusic();
    }
    #endregion

    #region Protected/Private Methods
    private IEnumerator CheckForBomb()
    {
        yield return null;

        while (true)
        {
            UnityEngine.Object[] bombs = FindObjectsOfType(CommonReflectedTypeInfo.BombType);
            if (bombs != null && bombs.Length > 0)
            {
                SetBomb((MonoBehaviour)bombs[0]);
                break;
            }

            yield return null;
        }
    }

    private void SetBomb(MonoBehaviour bomb)
    {
        _bombCommander = new BombCommander(bomb);
        CreateBombHandleForBomb(bomb);
        CreateComponentHandlesForBomb(bomb);

        _ircConnection.SendMessage("The next bomb is now live! Start sending your commands! MrDestructoid");

        _bombHandle.OnMessageReceived("The Bomb", "red", "!bomb hold");
    }

    protected override void OnMessageReceived(string userNickName, string userColorCode, string text)
    {
        if (_bombHandle != null)
        {
            _bombHandle.OnMessageReceived(userNickName, userColorCode, text);
        }

        foreach (TwitchComponentHandle componentHandle in _componentHandles)
        {
            componentHandle.OnMessageReceived(userNickName, userColorCode, text);
        }
    }

    private void CreateBombHandleForBomb(MonoBehaviour bomb)
    {
        _bombHandle = Instantiate<TwitchBombHandle>(twitchBombHandlePrefab);
        _bombHandle.ircConnection = _ircConnection;
        _bombHandle.bombCommander = _bombCommander;
        _bombHandle.coroutineQueue = _coroutineQueue;
        _bombHandle.coroutineCanceller = _coroutineCanceller;
    }

    private bool CreateComponentHandlesForBomb(MonoBehaviour bomb)
    {
        bool foundComponents = false;

        IList bombComponents = (IList)CommonReflectedTypeInfo.BombComponentsField.GetValue(bomb);

        if (bombComponents.Count > 12)
        {
            _bombCommander._multiDecker = true;
        }

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
            handle.leaderboard = leaderboard;

            Vector3 idealOffset = handle.transform.TransformDirection(GetIdealPositionForHandle(handle, bombComponents, out handle.direction));
            handle.transform.SetParent(bombComponent.transform.parent, true);
            handle.basePosition = handle.transform.localPosition;
            handle.idealHandlePositionOffset = bombComponent.transform.parent.InverseTransformDirection(idealOffset);

            _componentHandles.Add(handle);
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

    private IEnumerator SendDelayedMessage(float delay, string message)
    {
        yield return new WaitForSeconds(delay);
        _ircConnection.SendMessage(message);
    }
    #endregion
}
