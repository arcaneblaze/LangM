using Newtonsoft.Json;

namespace LangM;

public class LanguageMapper
{
    private Dictionary<int, string> _languageMappings;

    public LanguageMapper(string jsonFilePath)
    {
        LoadLanguageMappings(jsonFilePath);
    }

    private void LoadLanguageMappings(string jsonFilePath)
    {
        try
        {
            string json = File.ReadAllText(jsonFilePath);
            _languageMappings = JsonConvert.DeserializeObject<Dictionary<int, string>>(json);
        }
        catch (Exception ex)
        {
            throw new Exception("Error loading language mappings from JSON", ex);
        }
    }

    public string GetLanguageFromHKL(IntPtr hkl)
    {
        try
        {
            int languageId = (int)((uint)hkl & 0xFFFF);
            if (_languageMappings.TryGetValue(languageId, out var language))
            {
                return $"ID: {languageId} => {language}";
            }
            else
            {
                return $"ID: {languageId} => Unknown";
            }
        }
        catch (Exception ex)
        {
            return $"{ex.Message}";
        }
    }
}