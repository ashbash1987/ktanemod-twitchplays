using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class ProfileManager
{
	public static HashSet<string> ActiveProfiles = new HashSet<string>();
	public static readonly string TPProfiles = Path.Combine(Application.persistentDataPath, "TPprofiles.txt");
	public static readonly string ActiveConfiguration = Path.Combine(Application.persistentDataPath, "modSelectorConfig.json");

	public static bool ProfilesManagable
	{
		get
		{
			return File.Exists(TPProfiles) && File.Exists(ActiveConfiguration);
		}
	}

	public static void UpdateActiveProfiles()
	{
		File.WriteAllText(ActiveConfiguration, JsonConvert.SerializeObject(ActiveProfiles));
	}

	public static void AddProfile(string profile)
	{
		ActiveProfiles.Add(profile);
		UpdateActiveProfiles();
	}

	public static bool RemoveProfile(string profile)
	{
		if (ActiveProfiles.Remove(profile))
		{
			UpdateActiveProfiles();
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void SetJSON(string json)
	{
		ActiveProfiles = (HashSet<string>) JsonConvert.DeserializeObject(json);
	}

	public static bool ProfileExists(string profileName)
	{
		return File.ReadAllLines(TPProfiles).Any(x => x.StartsWith(profileName));
	}
}
