namespace Marketplace;

public static class GameEvents
{
    public static Action OnPlayerFirstSpawn;
    public static Action<string, int> OnCreatureKilled;
    public static Action<string> OnStructureBuilt;
    public static Action<string, int> OnItemCrafted;
    public static Action<string> KilledBy;
    public static Action<string> OnHarvest;
    public static Action OnPlayerDeath;

    [HarmonyPatch(typeof(Player),nameof(Player.OnSpawned))]
    [ClientOnlyPatch]
    private static class Player_SetLocalPlayer_Patch
    {
        public static bool firstTime = true;
        [UsedImplicitly]
        private static void Postfix()
        {
            if (firstTime)
            {
                firstTime = false;
                OnPlayerFirstSpawn?.Invoke();
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup),nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() => Player_SetLocalPlayer_Patch.firstTime = true;
    }
    
    [HarmonyPatch(typeof(Player),nameof(Player.OnDeath))]
    [ClientOnlyPatch]
    private static class Player_OnDeath_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player __instance)
        {
            if (Player.m_localPlayer != __instance) return;
            OnPlayerDeath?.Invoke();
        }
    }
    
    [HarmonyPatch(typeof(Character),nameof(Character.ApplyDamage))]
    [ClientOnlyPatch]
    private static class Character_ApplyDamage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Character __instance, ref HitData hit)
        {
            if(__instance != Player.m_localPlayer) return;
            
            if (Player.m_localPlayer.GetHealth() <= 0)
            {
                if (hit.GetAttacker() is { } killer)
                {
                    string prefabName = killer.IsPlayer() ? "PLAYER_" + ((Player)killer).GetPlayerName() : global::Utils.GetPrefabName(killer.gameObject);
                    KilledBy?.Invoke(prefabName);
                }
            }
        }
    }
    
    
    
}