using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

public class ModuleInformation
{
    public string moduleID;
    public string helpText;
    public string manualCode;
    public bool statusLightLeft;
    public bool statusLightDown;
    public float chatRotation;
    public string[] validCommands;
    public bool DoesTheRightThing;
}

public static class ModuleData
{
    public static ModuleInformation[] modInfo;
    private static bool _dataRead = false;
    public static void WriteDataToFile()
    {
        if (!_dataRead) return;
        string path = Path.Combine(Application.persistentDataPath, usersSavePath);
        try
        {
            ModuleInformation[] infoList = ComponentSolverFactory.GetModuleInformation();
            File.WriteAllText(path,JsonConvert.SerializeObject(infoList, Formatting.Indented));
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarningFormat("ModuleData: File {0} was not found.", path);
            return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return;
        }
    }

    public static void LoadDataFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, usersSavePath);
        try
        {
            Debug.Log("ModuleData: Loading Module information data from file: " + path);
            modInfo = JsonConvert.DeserializeObject<ModuleInformation[]>(File.ReadAllText(path));
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarningFormat("ModuleData: File {0} was not found.", path);
            return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return;
        }
        foreach (ModuleInformation info in modInfo)
        {
           ComponentSolverFactory.AddModuleInformation(info);
        }
        _dataRead = true;
        WriteDataToFile();
    }

    public static string usersSavePath = "ModuleInformation.json";
}
