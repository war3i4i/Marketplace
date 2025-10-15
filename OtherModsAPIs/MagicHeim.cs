namespace Marketplace.OtherModsAPIs;

internal static class MH_API
{
    private static readonly MethodInfo MI_AddSkillUseCondition;
    private static readonly MethodInfo MI_AddEXP;
    private static readonly MethodInfo MI_GetLevel;
    private static readonly MethodInfo MI_GetClass;
    private static readonly MethodInfo MI_GetEXP;
    private static readonly MethodInfo MI_GetEXPToNextLevel;

    public enum API_Class
    {
        None,
        Warrior,
        Mage,
        Druid
    }

    public static void AddSkillUseCondition(Func<bool> Method, string message = null)
    {
        MI_AddSkillUseCondition?.Invoke(null, new object[] { Method, message });
    }

    public static void AddEXP(int amount)
    {
        MI_AddEXP?.Invoke(null, new object[] { amount });
    }

    public static int GetLevel()
    {
        return (int)MI_GetLevel?.Invoke(null, null);
    }

    public static API_Class GetClass()
    {
        return (API_Class)MI_GetClass?.Invoke(null, null);
    }

    public static long GetEXP()
    {
        return (long)MI_GetEXP?.Invoke(null, null);
    }

    public static long GetEXPToNextLevel()
    {
        return (long)MI_GetEXPToNextLevel?.Invoke(null, null);
    }


    private static int DefaultRetInt() => 0;
    private static long DefaultRetLong() => 0;

    static MH_API()
    {
        if (Type.GetType("MagicHeim.API.API, MagicHeim") is not { } mh_API)
        {
            MethodInfo defaultRetInt = new Func<int>(DefaultRetInt).Method;
            MethodInfo defaultRetLong = new Func<long>(DefaultRetLong).Method;
            MI_GetClass = defaultRetInt;
            MI_GetLevel = defaultRetInt;
            MI_GetEXP = defaultRetLong;
            MI_GetEXPToNextLevel = defaultRetLong;
            return;
        }

        MI_AddSkillUseCondition =
            mh_API.GetMethod("MH_AddSkillUseCondition", BindingFlags.Public | BindingFlags.Static);
        MI_AddEXP = mh_API.GetMethod("MH_AddEXP", BindingFlags.Public | BindingFlags.Static);
        MI_GetLevel = mh_API.GetMethod("MH_GetLevel", BindingFlags.Public | BindingFlags.Static);
        MI_GetClass = mh_API.GetMethod("MH_GetClass", BindingFlags.Public | BindingFlags.Static);
        MI_GetEXP = mh_API.GetMethod("MH_GetEXP", BindingFlags.Public | BindingFlags.Static);
        MI_GetEXPToNextLevel = mh_API.GetMethod("MH_GetEXPToNextLevel", BindingFlags.Public | BindingFlags.Static);
    }
}