namespace Marketplace.OtherModsAPIs;

internal static class Cozyheim_LevelingSystem
{
    private static readonly bool _isInstalled;
    private static readonly FieldInfo FI_UIManagerInstance;
    private static readonly FieldInfo FI_Level;
    private static readonly MethodInfo MI_AddEXP;

    public static int GetLevel()
    {
        if (!_isInstalled || FI_UIManagerInstance.GetValue(null) is not { } ui) return 0;
        return (int)FI_Level.GetValue(ui);
    }
    
    public static void AddExp(int amount)
    {
        if (!_isInstalled || FI_UIManagerInstance.GetValue(null) is not { } ui) return;
        MI_AddEXP.Invoke(ui, new object[] { amount, 0 });
    }

    static Cozyheim_LevelingSystem()
    {
        if (Type.GetType("Cozyheim.LevelingSystem.UIManager, Cozyheim_LevelingSystem") is not { } chls_API)
        {
            _isInstalled = false;
            return;
        }
        _isInstalled = true;
        FI_UIManagerInstance = AccessTools.Field(chls_API, "Instance");
        FI_Level = AccessTools.Field(chls_API, "playerLevel");
        MI_AddEXP = AccessTools.Method(chls_API, "AddExperience");
    }
}