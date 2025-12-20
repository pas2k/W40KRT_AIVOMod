using System;
using System.IO;

namespace AiVoiceoverMod;

public static class Constants
{
    public const string WINDOWS_VOICE_DLL = "WindowsVoice";
    public const string WINDOWS_VOICE_NAME = "WindowsVoice";
    public const string APPLE_VOICE_NAME = "AppleVoice";
    public const string SETTINGS_PREFIX = "pas2k.aivomod";
    public const string NARRATOR_COLOR_CODE = "3c2d0a";

    public static readonly string LOCAL_LOW_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + "Low";

    public static string MOD_NAME = "W40KRT_AIVOMod";
}
