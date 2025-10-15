namespace Marketplace.OtherModsAPIs;

public static class RustyClassesAPI
{
    private static readonly MethodInfo? API_AddExperience;
    private static readonly MethodInfo? API_GetLevel;
    private static readonly MethodInfo? API_GetCharacteristic;
    public static void AddEXP(int amount)
    {
        API_AddExperience?.Invoke(null, new object[] { amount });
    }

    public static int GetLevel()
    {
        return (int)(API_GetLevel?.Invoke(null, null) ?? 0);
    }

    public static int GetCharacteristic(string type)
    {
        return (int)(API_GetCharacteristic?.Invoke(null, new object[] { type }) ?? 0);
    }
    
    public static int GetConstitution() => GetCharacteristic("Constitution");
    public static int GetDexterity() => GetCharacteristic("Dexterity");
    public static int GetStrength() => GetCharacteristic("Strength");
    public static int GetIntelligence() => GetCharacteristic("Intelligence");
    public static int GetWisdom() => GetCharacteristic("Wisdom");

    static RustyClassesAPI()
    {
        if (Type.GetType("AlmanacClasses.API.API, AlmanacClasses") is not { } api)
        {
            return;
        }
        
        API_AddExperience = api.GetMethod("AddExperience", BindingFlags.Public | BindingFlags.Static);
        API_GetLevel = api.GetMethod("GetLevel", BindingFlags.Public | BindingFlags.Static);
        API_GetCharacteristic = api.GetMethod("GetCharacteristic", BindingFlags.Public | BindingFlags.Static);
    }
}