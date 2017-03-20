using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class MissionTracker
{
    static MissionTracker()
    {
        Type missionManagerType = ReflectionHelper.FindType("Assets.Scripts.Missions.MissionManager");
        PropertyInfo instanceProperty = missionManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        object missionManager = instanceProperty.GetValue(null, null);

        PropertyInfo missionDBProperty = missionManagerType.GetProperty("MissionDB", BindingFlags.Public | BindingFlags.Instance);
        object missionDatabase = missionDBProperty.GetValue(missionManager, null);

        Type missionDatabaseType = ReflectionHelper.FindType("Assets.Scripts.Missions.MissionDatabase");
        FieldInfo missionsField = missionDatabaseType.GetField("Missions", BindingFlags.Public | BindingFlags.Instance);       
        IList missions = (IList)missionsField.GetValue(missionDatabase);

        Type missionType = ReflectionHelper.FindType("Assets.Scripts.Missions.Mission");
        PropertyInfo idProperty = missionType.GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);

        foreach(object mission in missions)
        {
            _missionIDs.Add((string)idProperty.GetValue(mission, null));
        }
    }

    private static List<string> _missionIDs = new List<string>();
}
