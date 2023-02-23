using UnityEditor;
using UnityEngine;

public class AutoAddScenesInBuildSettings : MonoBehaviour
{
    [InitializeOnLoadMethod]
    private static void OnBuildSetting()
    {
#if GAME_SOCKET
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/SparkAssets/Scenes/Startup.unity", true),
            new EditorBuildSettingsScene("Assets/SparkAssets/Scenes/Stage_01_KTV.unity", true),
//            new EditorBuildSettingsScene("Assets/SparkAssets/Scenes/AmongUS.unity", true),
            new EditorBuildSettingsScene("Assets/SparkAssets/Scenes/AmongUS/AUS_Map_01.unity", true),
            new EditorBuildSettingsScene("Assets/SparkAssets/Game/PersonalShow/Scenes/PersonalShow.unity", true),
        };
#endif
        EditorApplication.quitting += () =>
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/SparkAssets/Scenes/Startup.unity", true),
            };
        };
    }
}