using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ModuleCameras : MonoBehaviour
{
    public class ModuleItem
    {
        public MonoBehaviour component = null;
        public MonoBehaviour handle = null;

        public ModuleItem(MonoBehaviour c, MonoBehaviour h)
        {
            component = c;
            handle = h;
        }
    }

    public class ModuleCamera : MonoBehaviour
    {
        public Camera cameraInstance = null;
        public int priority = CameraNotInUse;
        public int index = 0;
        public ModuleItem module = null;

        private ModuleCameras parent = null;
        private int originalLayer = 0;

        public ModuleCamera(Camera instantiatedCamera, ModuleCameras parentInstance)
        {
            cameraInstance = instantiatedCamera;
            parent = parentInstance;
        }

        public void Refresh()
        {
            Deactivate();
            while ( (parent.priorityModuleStack.Count > 0) && (module == null) )
            {
                module = parent.priorityModuleStack.Pop();
                if (ModuleIsSolved)
                {
                    module = null;
                }
                else
                {
                    priority = CameraPrioritised;
                }
            }
            while ( (parent.moduleStack.Count > 0) && (module == null) )
            {
                module = parent.moduleStack.Pop();
                if (ModuleIsSolved)
                {
                    module = null;
                }
                else
                {
                    priority = CameraInUse;
                }
            }
            if (module != null)
            {
                index = ++ModuleCameras.index;
                // We know the camera's culling mask is pointing at a single layer, so let's find out what that layer is
                int newLayer = (int)Math.Log(cameraInstance.cullingMask, 2);
                originalLayer = module.component.gameObject.layer;
                Debug.LogFormat("[ModuleCameras] Switching component's layer from {0} to {1}", originalLayer, newLayer);
                SetRenderLayer(newLayer);
                cameraInstance.transform.SetParent(module.component.transform, false);
                cameraInstance.gameObject.SetActive(true);
                Debug.LogFormat("[ModuleCameras] Component's layer is {0}. Camera's bitmask is {1}", module.component.gameObject.layer, cameraInstance.cullingMask);
            }
        }

        public void Deactivate()
        {
            if (module != null)
            {
                SetRenderLayer(originalLayer);
            }
            cameraInstance.gameObject.SetActive(false);
            module = null;
            priority = CameraNotInUse;
        }

        private bool ModuleIsSolved
        {
            get
            {
                return (bool)CommonReflectedTypeInfo.IsSolvedField.GetValue(module.component);
            }
        }

        private void SetRenderLayer(int layer)
        {
            foreach (Transform trans in module.component.gameObject.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layer;
            }
            if (module.handle != null)
            {
                foreach (Transform trans in module.handle.gameObject.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = layer;
                }
            }
        }

    }


    #region Public Fields
    public Camera[] cameraPrefabs = null;
    #endregion

    #region Private Fields
    private Stack<ModuleItem> moduleStack = new Stack<ModuleItem>();
    private Stack<ModuleItem> priorityModuleStack = new Stack<ModuleItem>();
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
    public void AttachToModule(MonoBehaviour component, MonoBehaviour handle, bool priority = false)
    {
        int existingCamera = CurrentModulesContains(component);
        if (existingCamera > -1)
        {
            cameras[existingCamera].index = ++index;
            return;
        }
        ModuleCamera camera = AvailableCamera(priority);
        try
        {
            // If the camera is in use, return its module to the appropriate stack
            if ((camera.priority > CameraNotInUse) && (camera.module.component != null))
            {
                bool oldPriority = (camera.priority == CameraPrioritised);
                AddModuleToStack(camera.module.component, camera.module.handle, oldPriority);
                camera.priority = CameraNotInUse;
            }

            // Add the new module to the stack
            AddModuleToStack(component, handle, priority);

            // Refresh the camera
            camera.Refresh();
        }
        catch (Exception e)
        {
            Debug.Log(LogPrefix + "Error: " + e.Message);
        }
    }

    public void DetachFromModule(MonoBehaviour component, bool delay = false)
    {
        StartCoroutine(DetachFromModuleCoroutine(component, delay));
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
    private void AddModuleToStack(MonoBehaviour component, MonoBehaviour handle, bool priority)
    {
        ModuleItem item = new ModuleItem(component, handle);
        if (priority)
        {
            priorityModuleStack.Push(item);
        }
        else if (!moduleStack.Any(m => object.ReferenceEquals(component, m.component)))
        {
            moduleStack.Push(item);
        }
    }

    private IEnumerator DetachFromModuleCoroutine(MonoBehaviour component, bool delay)
    {
        foreach (ModuleCamera camera in cameras)
        {
            if ( (camera.module != null) && 
                (object.ReferenceEquals(camera.module.component, component)) )
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

    private int CurrentModulesContains(MonoBehaviour component)
    {
        int i = 0;
        foreach (ModuleCamera camera in cameras)
        {
            if ( (camera.module != null) &&
                (object.ReferenceEquals(camera.module.component, component)) )
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    private void SetCameraVisibility(bool visible)
    {
        foreach (ModuleCamera camera in cameras)
        {
            if (camera.priority > CameraNotInUse)
            {
                camera.cameraInstance.gameObject.SetActive(visible);
            }
        }
    }
    #endregion

    #region Private Properties

    #endregion
}
