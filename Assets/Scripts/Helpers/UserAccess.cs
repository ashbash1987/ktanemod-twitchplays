using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

[Flags()]
public enum AccessLevel
{
    SuperUser = 0x80,
    Admin = 0x40,
    Mod = 0x20,

    User = 0x00
}

public static class UserAccess
{ 
    private static Dictionary<string, AccessLevel> AccessLevels = new Dictionary<string, AccessLevel>();

    static UserAccess()
    {
        /*
         * Enter here the list of special user roles, giving them bitwise enum flags to determine the level of access each user has.
         * 
         * The access level enum can be extended further per your requirements.
         * 
         * Use the helper method below to determine if the user has access for a particular access level or not.
         * TODO: Extend this to a JSON-serializable type, and/or inspect the decorated PRIVMSG lines from IRC to infer moderator status from the Twitch Chat moderator flag.
         */

        //Twitch Usernames can't actually begin with an underscore, so these are safe to include as examples
        AccessLevels["_UserNickName1"] = AccessLevel.SuperUser | AccessLevel.Admin | AccessLevel.Mod;
        AccessLevels["_UserNickName2"] = AccessLevel.Mod;

        LoadAccessList();
    }

    public static void WriteAccessList()
    {
        string path = Path.Combine(Application.persistentDataPath, usersSavePath);
        try
        {
            Debug.Log("UserAccess: Writing User Access information data to file: " + path);
            File.WriteAllText(path, JsonConvert.SerializeObject(AccessLevels,Formatting.Indented,new StringEnumConverter()));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void LoadAccessList()
    {
        string path = Path.Combine(Application.persistentDataPath, usersSavePath);
        try
        {
            Debug.Log("UserAccess: Loading User Access information data from file: " + path);
            AccessLevels = JsonConvert.DeserializeObject<Dictionary<string, AccessLevel>>(File.ReadAllText(path), new StringEnumConverter());
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarningFormat("UserAccess: File {0} was not found.", path);
            WriteAccessList();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    public static string usersSavePath = "AccessLevels.json";

    public static bool HasAccess(string userNickName, AccessLevel accessLevel)
    {
        AccessLevel userAccessLevel = AccessLevel.User;
        return AccessLevels.TryGetValue(userNickName, out userAccessLevel) && (accessLevel & userAccessLevel) == accessLevel;
    }
}
