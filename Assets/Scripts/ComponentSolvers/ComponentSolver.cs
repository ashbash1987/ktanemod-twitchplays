using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class ComponentSolver : ICommandResponder
{
    public delegate IEnumerator RegexResponse(Match match);

    #region Constructors
    static ComponentSolver()
    {
        _selectableType = ReflectionHelper.FindType("Selectable");
        _interactMethod = _selectableType.GetMethod("HandleInteract", BindingFlags.Public | BindingFlags.Instance);
        _interactEndedMethod = _selectableType.GetMethod("OnInteractEnded", BindingFlags.Public | BindingFlags.Instance);
        _setHighlightMethod = _selectableType.GetMethod("SetHighlight", BindingFlags.Public | BindingFlags.Instance);
        _getFocusDistanceMethod = _selectableType.GetMethod("GetFocusDistance", BindingFlags.Public | BindingFlags.Instance);

        _floatingHoldableType = ReflectionHelper.FindType("FloatingHoldable");
        if (_floatingHoldableType == null)
        {
            return;
        }
        _focusMethod = _floatingHoldableType.GetMethod("Focus", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Transform), typeof(float), typeof(bool), typeof(bool), typeof(float) }, null);
        _defocusMethod = _floatingHoldableType.GetMethod("Defocus", BindingFlags.Public | BindingFlags.Instance);
        _focusTimeField = _floatingHoldableType.GetField("FocusTime", BindingFlags.Public | BindingFlags.Instance);
        _pickupTimeField = _floatingHoldableType.GetField("PickupTime", BindingFlags.Public | BindingFlags.Instance);
        _holdStateProperty = _floatingHoldableType.GetProperty("HoldState", BindingFlags.Public | BindingFlags.Instance);

        _selectableManagerType = ReflectionHelper.FindType("SelectableManager");
        if (_selectableManagerType == null)
        {
            return;
        }
        _holdMethod = _selectableManagerType.GetMethod("Hold", BindingFlags.Public | BindingFlags.Instance);

        _inputManagerType = ReflectionHelper.FindType("KTInputManager");
        if (_inputManagerType == null)
        {
            return;
        }
        _instanceProperty = _inputManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        _selectableManagerProperty = _inputManagerType.GetProperty("SelectableManager", BindingFlags.Public | BindingFlags.Instance);

        _inputManager = (MonoBehaviour)_instanceProperty.GetValue(null, null);    
    }

    public ComponentSolver(MonoBehaviour bomb, MonoBehaviour bombComponent)
    {
        Bomb = bomb;
        BombComponent = bombComponent;        
    }
    #endregion

    public IEnumerator RespondToCommand(string message)
    {
        IEnumerator subcoroutine = RespondToCommandInternal(message);
        if (!subcoroutine.MoveNext())
        {
            yield break;
        }

        MonoBehaviour floatingHoldable = (MonoBehaviour)Bomb.GetComponent(_floatingHoldableType);

        int holdState = (int)_holdStateProperty.GetValue(floatingHoldable, null);
        if (holdState != 0)
        {
            MonoBehaviour selectableManager = (MonoBehaviour)_selectableManagerProperty.GetValue(_inputManager, null);
            _holdMethod.Invoke(selectableManager, new object[] { floatingHoldable });
        }

        float focusTime = (float)_focusTimeField.GetValue(floatingHoldable);
        _focusMethod.Invoke(floatingHoldable, new object[] { BombComponent.transform, FocusDistance, true, true, focusTime });
        yield return new WaitForSeconds(focusTime * 1.5f);

        while (subcoroutine.MoveNext())
        {
            yield return subcoroutine.Current;
        }
        yield return new WaitForSeconds(focusTime * 1.5f);

        _defocusMethod.Invoke(floatingHoldable, new object[] { true, true });
        yield return new WaitForSeconds(focusTime * 1.5f);
    }

    #region Abstract Interface
    protected abstract IEnumerator RespondToCommandInternal(string message);
    #endregion

    #region Protected Helper Methods
    protected void DoInteractionStart(MonoBehaviour interactable)
    {
        MonoBehaviour selectable = (MonoBehaviour)interactable.GetComponent(_selectableType);
        _interactMethod.Invoke(selectable, null);
    }

    protected void DoInteractionEnd(MonoBehaviour interactable)
    {
        MonoBehaviour selectable = (MonoBehaviour)interactable.GetComponent(_selectableType);
        _interactEndedMethod.Invoke(selectable, null);
        _setHighlightMethod.Invoke(selectable, new object[] { false });
    }
    #endregion

    #region Public Properties
    public bool Solved
    {
        get
        {
            return (bool)CommonReflectedTypeInfo.IsSolvedField.GetValue(BombComponent);
        }
    }

    public int StrikeCount
    {
        get
        {
            return (int)CommonReflectedTypeInfo.NumStrikesField.GetValue(Bomb);
        }
    }

    public float FocusDistance
    {
        get
        {
            MonoBehaviour selectable = (MonoBehaviour)BombComponent.GetComponent(_selectableType);
            return (float)_getFocusDistanceMethod.Invoke(selectable, null);
        }
    }
    #endregion

    #region Readonly Fields
    public readonly MonoBehaviour Bomb = null;
    public readonly MonoBehaviour BombComponent = null;
    #endregion

    #region Private Static Fields
    private static Type _selectableType = null;
    private static MethodInfo _interactMethod = null;
    private static MethodInfo _interactEndedMethod = null;
    private static MethodInfo _setHighlightMethod = null;
    private static MethodInfo _getFocusDistanceMethod = null;

    private static Type _floatingHoldableType = null;
    private static MethodInfo _focusMethod = null;
    private static MethodInfo _defocusMethod = null;
    private static FieldInfo _focusTimeField = null;
    private static FieldInfo _pickupTimeField = null;
    private static PropertyInfo _holdStateProperty = null;

    private static Type _selectableManagerType = null;
    private static MethodInfo _holdMethod = null;

    private static Type _inputManagerType = null;
    private static PropertyInfo _instanceProperty = null;
    private static PropertyInfo _selectableManagerProperty = null;

    private static MonoBehaviour _inputManager = null;
    #endregion
}
