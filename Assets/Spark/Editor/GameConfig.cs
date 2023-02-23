using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public static class GameConfig
{
	public static string projectRootPath
	{
		get;
	}
	public static string projectOthersPath
	{
		get;
	}
	public static string projectClientPath
	{
		get;
	}
	public static string projectDirectoryName
	{
		get;
	}
	public static string projectLuaPath
	{
		get;
	}
	public static string projectDataPath
	{
		get;
	}
	public static string projectLuaDataPath
	{
		get;
	}
	public static string projectSparkTempPath
	{
		get;
	}

	static GameConfig()
	{
		projectRootPath = Path.GetFullPath(Application.dataPath + "/../..");
		// projectClientPath = Path.GetFullPath(projectRootPath + "/..");
		// projectOthersPath = Path.GetFullPath(projectClientPath + "/../mobmoon-others");
		projectDirectoryName = Path.GetFileName(projectRootPath);
		projectLuaPath = Application.dataPath + "/SparkAssets/Lua";
		// projectLuaDataPath = projectLuaPath + "/Game/Excels";
		projectDataPath = projectOthersPath + "/data/" + projectDirectoryName;
		projectSparkTempPath = Application.dataPath + "/../SparkTemp";
	}
}
