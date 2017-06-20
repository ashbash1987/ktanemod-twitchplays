using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ModuleCameras : MonoBehaviour
{
    public class ModuleCamera : MonoBehaviour
    {
        public Camera cameraInstance = null;
        public int priority = CameraNotInUse;
        public int index = 0;
        public MonoBehaviour component = null;

        private ModuleCameras parent = null;

        public ModuleCamera(Camera instantiatedCamera, ModuleCameras parentInstance)
        {
            cameraInstance = instantiatedCamera;
            parent = parentInstance;
        }

        public void Refresh()
        {
            component = null;
            while ( (parent.priorityModuleStack.Count > 0) && (component == null) )
            {
                component = parent.priorityModuleStack.Pop();
                if (ModuleIsSolved)
                {
                    component = null;
                }
                else
                {
                    priority = CameraPrioritised;
                }
            }

            while ( (parent.moduleStack.Count > 0) && (component == null) )
            {
                component = parent.moduleStack.Pop();
                if (ModuleIsSolved)
                {
                    component = null;
                }
                else
                {
                    priority = CameraInUse;
                }
            }

            if (component == null)
            {
                priority = CameraNotInUse;
                cameraInstance.gameObject.SetActive(false);
                return;
            }

            index = ++ModuleCameras.index;
            cameraInstance.transform.SetParent(component.transform, false);
            cameraInstance.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            component = null;
            priority = CameraNotInUse;
            cameraInstance.gameObject.SetActive(false);
        }

        private bool ModuleIsSolved
        {
            get
            {
                return (bool)CommonReflectedTypeInfo.IsSolvedField.GetValue(component);
            }
        }

    }


    #region Public Fields
    public Camera[] cameraPrefabs = null;
    #endregion

    #region Private Fields
    private Stack<MonoBehaviour> moduleStack = new Stack<MonoBehaviour>();
    private Stack<MonoBehaviour> priorityModuleStack = new Stack<MonoBehaviour>();
    private List<ModuleCamera> cameras = new List<ModuleCamera>();
    #endregion

    #region Public Statics
    public static int index = 0;
    #endregion

    #region Private Static Readonlys
    private static readonly int CameraNotInUse = 0;
    private static readonly int CameraInUse = 1;
    private static readonly int CameraPrioritised = 2;
    private static readonly string LogPrefix = "[ModuleCameras] ";
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        
    }

    private void Start()
    {
        foreach (Camera camera in cameraPrefabs)
        {
            Camera instantiatedCamera = Instantiate<Camera>(camera);
            cameras.Add( new ModuleCamera(instantiatedCamera, this) );
        }
    }

    private void LateUpdate()
    {
        
    }
    #endregion

    #region Public Methods
    public void AttachToModule(MonoBehaviour module, bool priority = false)
    {
        int existingCamera = CurrentModulesContains(module);
        if (existingCamera > -1)
        {
            cameras[existingCamera].index = ++index;
            return;
        }

        ModuleCamera camera = AvailableCamera(priority);

        try
        {
            // If the camera is in use, return its module to the appropriate stack
            if ((camera.priority > CameraNotInUse) && (camera.component != null))
            {
                bool oldPriority = (camera.priority == CameraPrioritised);
                AddModuleToStack(camera.component, oldPriority);
                camera.priority = CameraNotInUse;
            }

            // Add the new module to the stack
            AddModuleToStack(module, priority);

            // Refresh the camera
            camera.Refresh();
        }
        catch (Exception e)
        {
            Debug.Log(LogPrefix + "Error: " + e.Message);
        }
    }

    public void DetachFromModule(MonoBehaviour module, bool delay = false)
    {
        StartCoroutine(DetachFromModuleCoroutine(module, delay));
    }

    public void Hide()
    {
        SetCameraVisibility(false);
    }

    public void Show()
    {
        SetCameraVisibility(true);
    }
    #endregion

    #region Private Methods
    private void AddModuleToStack(MonoBehaviour module, bool priority)
    {
        if (priority)
        {
            priorityModuleStack.Push(module);
        }
        else if (!moduleStack.Contains(module))
        {
            moduleStack.Push(module);
        }
    }

    private IEnumerator DetachFromModuleCoroutine(MonoBehaviour module, bool delay)
    {
        foreach (ModuleCamera camera in cameras)
        {
            if (object.ReferenceEquals(camera.component, module))
            {
                if (delay)
                {
                    yield return new WaitForSeconds(1.5f);
                }
                camera.Refresh();
            }
        }

        yield break;
    }

    private ModuleCamera AvailableCamera(bool priority = false)
    {
        ModuleCamera bestCamera = null;
        int minPriority = CameraPrioritised + 1;
        int minIndex = int.MaxValue;

        foreach (ModuleCamera cam in cameras)
        {
            // First available unused camera
            if (cam.priority == CameraNotInUse)
            {
                return cam;
                // And we're done!
            }
            else if ( (cam.priority < minPriority) ||
                ( (cam.priority == minPriority) && (cam.index < minIndex) )  )
            {
                bestCamera = cam;
                minPriority = cam.priority;
                minIndex = cam.index;
            }
        }

        // If no unused camera...
        // return the "best" camera (topmost camera of lowest priority)
        // but not if it's already prioritised and we're not demanding priority
        return ((minPriority < CameraPrioritised) || (priority)) ? bestCamera : null;
    }

    private int CurrentModulesContains(MonoBehaviour module)
    {
        int i = 0;
        foreach (ModuleCamera cam in cameras)
        {
            if (object.ReferenceEquals(module, cam.component))
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    private void SetCameraVisibility(bool visible)
    {
        foreach (ModuleCamera cam in cameras)
        {
            if (cam.priority > CameraNotInUse)
            {
                cam.cameraInstance.gameObject.SetActive(visible);
            }
        }
    }
    #endregion

    #region Private Properties

    #endregion
}
