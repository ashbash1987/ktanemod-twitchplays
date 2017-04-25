using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

public class BombCommander : ICommandResponder
{
    #region Constructors
    static BombCommander()
    {
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
        _bombTimeStamp = DateTime.Now;

        _selectableType = ReflectionHelper.FindType("Selectable");
        _handleSelectMethod = _selectableType.GetMethod("HandleSelect", BindingFlags.Public | BindingFlags.Instance);
        _onInteractEndedMethod = _selectableType.GetMethod("OnInteractEnded", BindingFlags.Public | BindingFlags.Instance);

        _selectableManagerType = ReflectionHelper.FindType("SelectableManager");
        if (_selectableManagerType == null)
        {
            return;
        }
        _selectMethod = _selectableManagerType.GetMethod("Select", BindingFlags.Public | BindingFlags.Instance);
        _handleInteractMethod = _selectableManagerType.GetMethod("HandleInteract", BindingFlags.Public | BindingFlags.Instance);
        _handleCancelMethod = _selectableManagerType.GetMethod("HandleCancel", BindingFlags.Public | BindingFlags.Instance);
        _setZSpinMethod = _selectableManagerType.GetMethod("SetZSpin", BindingFlags.Public | BindingFlags.Instance);
        _setControlsRotationMethod = _selectableManagerType.GetMethod("SetControlsRotation", BindingFlags.Public | BindingFlags.Instance);
        _getBaseHeldObjectTransformMethod = _selectableManagerType.GetMethod("GetBaseHeldObjectTransform", BindingFlags.Public | BindingFlags.Instance);
        _handleFaceSelectionMethod = _selectableManagerType.GetMethod("HandleFaceSelection", BindingFlags.Public | BindingFlags.Instance);

        _inputManagerType = ReflectionHelper.FindType("KTInputManager");
        if (_inputManagerType == null)
        {
            return;
        }
        _instanceProperty = _inputManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        _selectableManagerProperty = _inputManagerType.GetProperty("SelectableManager", BindingFlags.Public | BindingFlags.Instance);

        _inputManager = (MonoBehaviour)_instanceProperty.GetValue(null, null);
    }

    public BombCommander(MonoBehaviour bomb)
    {
        Bomb = bomb;
        Selectable = (MonoBehaviour)Bomb.GetComponent(_selectableType);
        FloatingHoldable = (MonoBehaviour)Bomb.GetComponent(_floatingHoldableType);
        SelectableManager = (MonoBehaviour)_selectableManagerProperty.GetValue(_inputManager, null);
    }
    #endregion

    #region Interface Implementation
    public IEnumerator RespondToCommand(string userNickName, string message, ICommandResponseNotifier responseNotifier)
    {
        if (message.Equals("hold", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("pick up", StringComparison.InvariantCultureIgnoreCase))
        {
            responseNotifier.ProcessResponse(CommandResponse.Start);

            IEnumerator holdCoroutine = HoldBomb(_heldFrontFace);
            while (holdCoroutine.MoveNext())
            {
                yield return holdCoroutine.Current;
            }

            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }
        else if (message.Equals("turn", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("turn round", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("turn around", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("flip", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("spin", StringComparison.InvariantCultureIgnoreCase))
        {
            responseNotifier.ProcessResponse(CommandResponse.Start);

            IEnumerator holdCoroutine = HoldBomb(!_heldFrontFace);
            while (holdCoroutine.MoveNext())
            {
                yield return holdCoroutine.Current;
            }

            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }
        else if (message.Equals("drop", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("let go", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("put down", StringComparison.InvariantCultureIgnoreCase))
        {
            responseNotifier.ProcessResponse(CommandResponse.Start);

            IEnumerator letGoCoroutine = LetGoBomb();
            while (letGoCoroutine.MoveNext())
            {
                yield return letGoCoroutine.Current;
            }

            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }
        else if (message.Equals("edgework", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("edgework right", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("edgework bottom", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("edgework left", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("edgework top", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("right", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("bottom", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("left", StringComparison.InvariantCultureIgnoreCase) ||
            message.Equals("top", StringComparison.InvariantCultureIgnoreCase))
        {
            responseNotifier.ProcessResponse(CommandResponse.Start);

            IEnumerator edgeworkCoroutine = ShowEdgework(message.Replace("edgework","").Trim().ToLowerInvariant());
            while (edgeworkCoroutine.MoveNext())
            {
                yield return edgeworkCoroutine.Current;
            }

            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }
        else if (message.Equals("time", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("timestamp", StringComparison.InvariantCultureIgnoreCase) ||
                 message.Equals("date", StringComparison.InvariantCultureIgnoreCase))
        {
            //Some modules depend on the date/time the bomb, and therefore that Module instance has spawned, in the bomb defusers timezone.

            responseNotifier.ProcessResponse(CommandResponse.Start);

            StringBuilder sb = new StringBuilder();
            sb.Append("sendtochat ");
            sb.Append("The Date/Time this bomb started is ");
            sb.Append(string.Format("{0:F}", _bombTimeStamp));
            yield return sb.ToString();

            responseNotifier.ProcessResponse(CommandResponse.EndNotComplete);
        }
        else
        {
            responseNotifier.ProcessResponse(CommandResponse.NoResponse);
        }
    }
    #endregion

    #region Helper Methods
    public IEnumerator HoldBomb(bool frontFace = true)
    {
        int holdState = (int)_holdStateProperty.GetValue(FloatingHoldable, null);
        bool doForceRotate = false;

        if (holdState != 0)
        {
            SelectObject(Selectable);
            doForceRotate = true;
        }
        else if (frontFace != _heldFrontFace)
        {
            doForceRotate = true;
        }

        if (doForceRotate)
        {
            float holdTime = (float)_pickupTimeField.GetValue(FloatingHoldable);
            IEnumerator forceRotationCoroutine = ForceHeldRotation(frontFace, holdTime);
            while (forceRotationCoroutine.MoveNext())
            {
                yield return forceRotationCoroutine.Current;
            }
        }
    }

    public IEnumerator LetGoBomb()
    {
        int holdState = (int)_holdStateProperty.GetValue(FloatingHoldable, null);
        if (holdState == 0)
        {
            IEnumerator turnBombCoroutine = HoldBomb(true);
            while (turnBombCoroutine.MoveNext())
            {
                yield return turnBombCoroutine.Current;
            }

            DeselectObject(Selectable);
        }
    }

    public IEnumerator ShowEdgework(string edge)
    {
        IEnumerator holdCoroutine = HoldBomb(_heldFrontFace);
        while (holdCoroutine.MoveNext())
        {
            yield return holdCoroutine.Current;
        }
        IEnumerator returnToFace;

        float initialZSpin = _heldFrontFace ? 0.0f : 180.0f;

        if (edge == "" || edge == "right")
        {
            IEnumerator firstEdge = DoFreeRotate(initialZSpin, 0.0f, 90.0f, 0.0f, 0.3f);
            while (firstEdge.MoveNext())
            {
                yield return firstEdge.Current;
            }
            yield return new WaitForSeconds(2.0f);
        }

        if (edge == "" || edge == "bottom")
        {

            IEnumerator secondEdge = edge == ""
                ? DoFreeRotate(90.0f, 0.0f, initialZSpin, 90.0f, 0.3f)
                : DoFreeRotate(initialZSpin, 0.0f, initialZSpin, 90.0f, 0.3f);
            while (secondEdge.MoveNext())
            {
                yield return secondEdge.Current;
            }
            yield return new WaitForSeconds(2.0f);
        }


        if (edge == "" || edge == "left")
        {
            IEnumerator thirdEdge = edge == ""
                ? DoFreeRotate(initialZSpin, 90.0f, -90.0f, 0.0f, 0.3f)
                : DoFreeRotate(initialZSpin, 0.0f, -90.0f, 0.0f, 0.3f);
            while (thirdEdge.MoveNext())
            {
                yield return thirdEdge.Current;
            }
            yield return new WaitForSeconds(2.0f);
        }

        if (edge == "" || edge == "top")
        {
            IEnumerator fourthEdge = edge == ""
                ? DoFreeRotate(-90.0f, 0.0f, initialZSpin, -90.0f, 0.3f)
                : DoFreeRotate(initialZSpin, 0.0f, initialZSpin, -90.0f, 0.3f);
            while (fourthEdge.MoveNext())
            {
                yield return fourthEdge.Current;
            }
            yield return new WaitForSeconds(2.0f);
        }

        switch (edge)
        {
            case "right":
                returnToFace = DoFreeRotate(90.0f, 0.0f, initialZSpin, 0.0f, 0.3f);
                break;
            case "bottom":
                returnToFace = DoFreeRotate(initialZSpin, 90.0f, initialZSpin, 0.0f, 0.3f);
                break;
            case "left":
                returnToFace = DoFreeRotate(-90.0f, 0.0f, initialZSpin, 0.0f, 0.3f);
                break;
            case "top":
            default:
                returnToFace = DoFreeRotate(initialZSpin, -90.0f, initialZSpin, 0.0f, 0.3f);
                break;
        }
        
        while (returnToFace.MoveNext())
        {
            yield return returnToFace.Current;
        }
    }

    public IEnumerator Focus(MonoBehaviour selectable, float focusDistance, bool frontFace)
    {
        IEnumerator holdCoroutine = HoldBomb(frontFace);
        while (holdCoroutine.MoveNext())
        {
            yield return holdCoroutine.Current;
        }

        float focusTime = (float)_focusTimeField.GetValue(FloatingHoldable);
        _focusMethod.Invoke(FloatingHoldable, new object[] { selectable.transform, focusDistance, false, false, focusTime });
    }

    public IEnumerator Defocus(bool frontFace)
    {
        _defocusMethod.Invoke(FloatingHoldable, new object[] { false, false });
        yield break;
    }

    public void RotateByLocalQuaternion(Quaternion localQuaternion)
    {
        Transform baseTransform = (Transform)_getBaseHeldObjectTransformMethod.Invoke(SelectableManager, null);

        float currentZSpin = _heldFrontFace ? 0.0f : 180.0f;

        _setControlsRotationMethod.Invoke(SelectableManager, new object[] { baseTransform.rotation * Quaternion.Euler(0.0f, 0.0f, currentZSpin) * localQuaternion });
        _handleFaceSelectionMethod.Invoke(SelectableManager, null);
    }

    private void SelectObject(MonoBehaviour selectable)
    {
        _handleSelectMethod.Invoke(selectable, new object[] { true });
        _selectMethod.Invoke(SelectableManager, new object[] { selectable, true });
        _handleInteractMethod.Invoke(SelectableManager, null);
        _onInteractEndedMethod.Invoke(selectable, null);
    }

    private void DeselectObject(MonoBehaviour selectable)
    {
        _handleCancelMethod.Invoke(SelectableManager, null);
    }

    private IEnumerator ForceHeldRotation(bool frontFace, float duration)
    {
        Transform baseTransform = (Transform)_getBaseHeldObjectTransformMethod.Invoke(SelectableManager, null);

        float oldZSpin = _heldFrontFace ? 0.0f : 180.0f;
        float targetZSpin = frontFace ? 0.0f : 180.0f;

        float initialTime = Time.time;
        while (Time.time - initialTime < duration)
        {
            float lerp = (Time.time - initialTime) / duration;
            float currentZSpin = Mathf.SmoothStep(oldZSpin, targetZSpin, lerp);

            Quaternion currentRotation = Quaternion.Euler(0.0f, 0.0f, currentZSpin);

            _setZSpinMethod.Invoke(SelectableManager, new object[] { currentZSpin });
            _setControlsRotationMethod.Invoke(SelectableManager, new object[] { baseTransform.rotation * currentRotation });
            _handleFaceSelectionMethod.Invoke(SelectableManager, null);
            yield return null;
        }

        _setZSpinMethod.Invoke(SelectableManager, new object[] { targetZSpin });
        _setControlsRotationMethod.Invoke(SelectableManager, new object[] { baseTransform.rotation * Quaternion.Euler(0.0f, 0.0f, targetZSpin) });
        _handleFaceSelectionMethod.Invoke(SelectableManager, null);

        _heldFrontFace = frontFace;
    }

    private IEnumerator DoFreeRotate(float initialZSpin, float initialPitch, float targetZSpin, float targetPitch, float duration)
    {
        Transform baseTransform = (Transform)_getBaseHeldObjectTransformMethod.Invoke(SelectableManager, null);

        float initialTime = Time.time;
        while (Time.time - initialTime < duration)
        {
            float lerp = (Time.time - initialTime) / duration;
            float currentZSpin = Mathf.SmoothStep(initialZSpin, targetZSpin, lerp);
            float currentPitch = Mathf.SmoothStep(initialPitch, targetPitch, lerp);

            Quaternion currentRotation = Quaternion.Euler(currentPitch, 0.0f, currentZSpin);

            _setZSpinMethod.Invoke(SelectableManager, new object[] { currentZSpin });
            _setControlsRotationMethod.Invoke(SelectableManager, new object[] { baseTransform.rotation * currentRotation });
            _handleFaceSelectionMethod.Invoke(SelectableManager, null);
            yield return null;
        }

        _setZSpinMethod.Invoke(SelectableManager, new object[] { targetZSpin });
        _setControlsRotationMethod.Invoke(SelectableManager, new object[] { baseTransform.rotation * Quaternion.Euler(targetPitch, 0.0f, targetZSpin) });
        _handleFaceSelectionMethod.Invoke(SelectableManager, null);
    }
    #endregion

    #region Readonly Fields
    public readonly MonoBehaviour Bomb = null;
    public readonly MonoBehaviour Selectable = null;
    public readonly MonoBehaviour FloatingHoldable = null;
    private readonly MonoBehaviour SelectableManager = null;
    #endregion

    #region Private Static Fields
    private static Type _floatingHoldableType = null;
    private static MethodInfo _focusMethod = null;
    private static MethodInfo _defocusMethod = null;
    private static FieldInfo _focusTimeField = null;
    private static FieldInfo _pickupTimeField = null;
    private static PropertyInfo _holdStateProperty = null;
    private static DateTime _bombTimeStamp;

    private static Type _selectableType = null;
    private static MethodInfo _handleSelectMethod = null;
    private static MethodInfo _onInteractEndedMethod = null;

    private static Type _selectableManagerType = null;
    private static MethodInfo _selectMethod = null;
    private static MethodInfo _handleInteractMethod = null;
    private static MethodInfo _handleCancelMethod = null;
    private static MethodInfo _setZSpinMethod = null;
    private static MethodInfo _setControlsRotationMethod = null;
    private static MethodInfo _getBaseHeldObjectTransformMethod = null;
    private static MethodInfo _handleFaceSelectionMethod = null;

    private static Type _inputManagerType = null;
    private static PropertyInfo _instanceProperty = null;
    private static PropertyInfo _selectableManagerProperty = null;

    private static MonoBehaviour _inputManager = null;
    #endregion

    private bool _heldFrontFace = true;
}

