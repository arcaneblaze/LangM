using Microsoft.Win32;

namespace LangM;

public class AppHelper
{
    public static void Logic(bool value)
    {
        if (value) AddToStartup();
        else RemoveFromStartup();
    }

    static void AddToStartup()
    {
        string appPath = Application.ExecutablePath;

        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        registryKey.SetValue("LangM", $"\"{appPath}\"");
        registryKey.Close();
    }

    static void RemoveFromStartup()
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (registryKey != null)
        {
            registryKey.DeleteValue("LangM", false);
            registryKey.Close();
        }
    }

    public static bool AddedToStartup()
    {
        return (Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
            true).GetValueNames().Contains("LangM"));
    }
}