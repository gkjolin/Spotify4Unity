using UnityEngine;

/// <summary>
/// Class to handle debug information and logging for the Spotify4Unity plugin
/// Can enabled all logs from the plugin by adding "S4U_LOGS" into the Scripting Define Symbols inside Unity Player Settings
/// </summary>
public class Analysis
{
    static string PLUGIN_NAME = "Spotify4Unity";

    public static void Log(string message)
    {
#if S4U_LOGS
        Debug.Log(GetFormat(message));
#endif
    }

    public static void LogError(string message)
    {
#if S4U_LOGS
        Debug.LogError(GetFormat(message));
#endif
    }

    static string GetFormat(string message)
    {
        return $"{PLUGIN_NAME} - {message}";
    }
}
