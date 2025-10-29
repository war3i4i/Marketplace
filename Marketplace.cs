using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;
using AsyncModLoader;
using BepInEx.Logging;
using Marketplace.ExternalLoads;
using Marketplace.Paths;
using UnityEngine.Rendering;
using Object = System.Object;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

namespace Marketplace 
{ 
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    [BepInDependency("kg.AsyncModLoader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("org.bepinex.plugins.groups", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.guilds", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
    public class Marketplace : BaseUnityPlugin
    { 
        private const string GUID = "MarketplaceAndServerNPCs"; 
        private const string PluginName = "MarketplaceAndServerNPCs"; 
        public const string PluginVersion = "9.7.1";
        internal static Marketplace _thistype = null!; 
        private static readonly Harmony _harmony = new(GUID); 
        public static Action<float> Global_Updator;
        public static Action<float> Global_FixedUpdator; 
        public static Action Global_OnGUI_Updator;
        public static Action Global_Start;
        public static Type TempProfessionsType;

        public static ManualLogSource DebugLogger = BepInEx.Logging.Logger.CreateLogSource("MarketplaceDebug");
        
        public static readonly ConfigSync configSync = new(GUID)
        { 
            DisplayName = GUID, ModRequired = true, MinimumRequiredVersion = PluginVersion, CurrentVersion = PluginVersion, IsLocked = true
        };
          
        public enum WorkingAs
        {
            Client,
            Server,
            Both
        } 
        
        public static WorkingAs WorkingAsType;
        public static Dictionary<MarketplaceRPC, MethodInfo> RoutedRPCs = new();
        private void Awake() => StartCoroutine(AsyncAwake());
        private IEnumerator AsyncAwake()
        {
            this.AsyncModLoaderInit();
            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server
                : Config.Bind("General", "Use Marketplace Locally", false, "Enable Market Local Usage").Value
                    ? WorkingAs.Both
                    : WorkingAs.Client;
            Utils.print($"Marketplace Working As: {WorkingAsType}");
            _thistype = this;
            HarmonyLib.Tools.Logger.ChannelFilter = HarmonyLib.Tools.Logger.LogChannel.Error;
            Localizer.Load();
            TempProfessionsType = Type.GetType("Professions.Professions, Professions")!;
            JSON.Parameters = new JSONParameters
            {
                UseExtensions = false,
                SerializeNullValues = false,
                DateTimeMilliseconds = false,
                UseUTCDateTime = true,
                UseOptimizedDatasetSchema = true,
                UseValuesOfEnums = true,
            };
            yield return AssetStorage.Initialize();
            IEnumerable<KeyValuePair<Market_Autoload, Type>> toAutoload = AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
                .Where(t => t.GetCustomAttribute<Market_Autoload>() != null)
                .Select(x => new KeyValuePair<Market_Autoload, Type>(x.GetCustomAttribute<Market_Autoload>(), x))
                .OrderBy(x => x.Key.priority).Where(x => WorkingAsType switch
                {
                    WorkingAs.Client => x.Key.type != Market_Autoload.Type.Server,
                    WorkingAs.Server => x.Key.type != Market_Autoload.Type.Client,
                    _ => true
                }); 
            foreach (KeyValuePair<Market_Autoload, Type> autoload in toAutoload)
            {
                if (autoload.Key.OnWatcherNames != null && autoload.Key.OnWatcherMethods != null &&
                    autoload.Key.OnWatcherNames.Length == autoload.Key.OnWatcherMethods.Length)
                {
                    for (int i = 0; i < autoload.Key.OnWatcherNames.Length; ++i)
                    {
                        MethodInfo configWatcherMethod = autoload.Value.GetMethod(autoload.Key.OnWatcherMethods[i],
                            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                        if (configWatcherMethod == null) 
                            Utils.print(
                                $"Error loading {autoload.Value.Name} class, method {autoload.Key.OnWatcherMethods[i]} not found", 
                                ConsoleColor.Red);
                        else
                            FSW_Lookup.Add(autoload.Key.OnWatcherNames[i],
                                () => configWatcherMethod.Invoke(null, null));
                    }
                }

                MethodInfo method = autoload.Value.GetMethod(autoload.Key.InitMethod ?? "OnInit",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    Utils.print(
                        $"Error loading {autoload.Value.Name} class, method {autoload.Key.InitMethod} not found",
                        ConsoleColor.Red);
                    continue;
                }

                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Utils.print($"Autoload exception on method {method}. Class {autoload.Value}\n:{ex}", ConsoleColor.Red);
                }
            }

            RoutedRPCs = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
                .Where(m => m.GetCustomAttribute<MarketplaceRPC>() != null)
                .Select(x => new KeyValuePair<MarketplaceRPC, MethodInfo>(x.GetCustomAttribute<MarketplaceRPC>(), x))
                .Where(x => WorkingAsType switch
                {
                    WorkingAs.Client => x.Key.type != Market_Autoload.Type.Server,
                    WorkingAs.Server => x.Key.type != Market_Autoload.Type.Client,
                    _ => true 
                }).ToDictionary(x => x.Key, x => x.Value);

            InitFSW(Market_Paths.MainPath);
            AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
                .Where(t => WorkingAsType switch
                {
                    WorkingAs.Client => t.GetCustomAttribute<ServerOnlyPatch>() == null,
                    WorkingAs.Server => t.GetCustomAttribute<ClientOnlyPatch>() == null, 
                    _ => true 
                }) 
                .Where(t => t.GetCustomAttribute<ConditionalPatch>() is not { } cond || cond.Check(t)) 
                .Do(type => _harmony.CreateClassProcessor(type).Patch());
            this.AsyncModLoaderDone();
        }
        
        private void Update() => Global_Updator?.Invoke(Time.deltaTime);
        private void OnGUI() => Global_OnGUI_Updator?.Invoke();
        private void FixedUpdate() => Global_FixedUpdator?.Invoke(Time.fixedDeltaTime);
        private void Start() => Global_Start?.Invoke();

        private static void InitFSW(string folderPath)
        {
            if (WorkingAsType is WorkingAs.Client) return;
            FillFolderRoutes();
            try
            {
                FileSystemWatcher cfgWatcher = new FileSystemWatcher(folderPath)
                {
                    Filter = "*.cfg",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    SynchronizingObject = ThreadingHelper.SynchronizingObject,
                    NotifyFilter = NotifyFilters.LastWrite
                };
                cfgWatcher.Changed += MarketplaceConfigChanged;
                FileSystemWatcher ymlWatcher = new FileSystemWatcher(folderPath)
                {
                    Filter = "*.yml",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    SynchronizingObject = ThreadingHelper.SynchronizingObject,
                    NotifyFilter = NotifyFilters.LastWrite
                };
                ymlWatcher.Changed += MarketplaceConfigChanged;
                FileSystemWatcher yamlWatcher = new FileSystemWatcher(folderPath)
                {
                    Filter = "*.yaml",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    SynchronizingObject = ThreadingHelper.SynchronizingObject,
                    NotifyFilter = NotifyFilters.LastWrite
                };
                yamlWatcher.Changed += MarketplaceConfigChanged;
            }
            catch (Exception ex)
            {
                Utils.print($"Can't start FSW, error: {ex}", ConsoleColor.Red);
            }
        }

        private static readonly Dictionary<string, string> FoldersToFiles = new();

        private static void FillFolderRoutes() 
        {
            FoldersToFiles.Add(Market_Paths.QuestsDatabaseFolder, "QD");
            FoldersToFiles.Add(Market_Paths.QuestsEventsFolder, "QE");
            FoldersToFiles.Add(Market_Paths.QuestsProfilesFolder, "QP");
            FoldersToFiles.Add(Market_Paths.DialoguesCustomSpawnDataFolder, "DSCD");
            FoldersToFiles.Add(Market_Paths.DialoguesFolder, "DI");
            FoldersToFiles.Add(Market_Paths.TerritoriesFolder, "TD");
            FoldersToFiles.Add(Market_Paths.BankerProfilesFolder, "BA");
            FoldersToFiles.Add(Market_Paths.TeleportHubProfilesFolder, "TE");
            FoldersToFiles.Add(Market_Paths.TraderProfilesFolder, "TR");
            FoldersToFiles.Add(Market_Paths.TransmogrificationFolder, "TM");
            FoldersToFiles.Add(Market_Paths.BufferDatabaseFolder, "BD");
            FoldersToFiles.Add(Market_Paths.BufferProfilesFolder, "BP");
            FoldersToFiles.Add(Market_Paths.ServerInfoProfilesFolder, "SI");
            FoldersToFiles.Add(Market_Paths.GamblerProfilesFolder, "GP");
            FoldersToFiles.Add(Market_Paths.LeaderboardAchievementsFolder, "LA");
            FoldersToFiles.Add(Market_Paths.LootboxesFolder, "LB");
        }

        private static readonly Dictionary<string, Action> FSW_Lookup = new();
        private static readonly Dictionary<string, DateTime> LastConfigChangeTime = new();
        public const string _InternalMarketplace_ = "_InternalMarketplace_";
        
        private static void MarketplaceConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType is not (WatcherChangeTypes.Changed or WatcherChangeTypes.Deleted)) return;
            string folderPath = Path.GetDirectoryName(e.FullPath);
            string fName = Path.GetFileName(e.Name);
            
            if (fName.Contains(_InternalMarketplace_)) return;
            
            foreach (KeyValuePair<string, string> foldersToFile in FoldersToFiles) 
            {
                if (!folderPath.Contains(foldersToFile.Key)) continue; 
                fName = foldersToFile.Value;
                break;
            }

            if (!FSW_Lookup.TryGetValue(fName, out Action action)) return;
            if (!ZNet.instance || !ZNet.instance.IsServer())
            {
                Utils.print($"FSW: Not a server, ignoring ({e.Name})", ConsoleColor.Red);
                return;
            }

            if (!LastConfigChangeTime.ContainsKey(fName)) LastConfigChangeTime[fName] = DateTime.MinValue;
            if (LastConfigChangeTime[fName] > DateTime.Now.AddSeconds(-5)) return;
            LastConfigChangeTime[fName] = DateTime.Now;
            Utils.DelayedAction(action);
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class RPC_REGISTERS
        {
            private static Type GetActionRaw(int length)
            {
                switch (length)
                {
                    case 1: return typeof(Action<long>);
                    case 2: return typeof(Action<long, object>);
                    case 3: return typeof(Action<long, object, object>);
                    case 4: return typeof(Action<long, object, object, object>);
                }
                return null;
            }
            
            [UsedImplicitly]
            private static void Postfix()
            {
                foreach (KeyValuePair<MarketplaceRPC, MethodInfo> rpc in RoutedRPCs)
                {
                    try 
                    {
                        List<Type> types = rpc.Value.GetParameters().Select(p => p.ParameterType).ToList();
                        object actionInstance = Delegate.CreateDelegate(Expression.GetActionType(types.ToArray()), rpc.Value);
                        types.RemoveAt(0);
                        if (types.Count >= 4)
                        { 
                            Utils.print($"Error registering RPC: {rpc.Key.RpcName}, method: {rpc.Value}, too many parameters", ConsoleColor.Red);
                            continue;
                        }  

                        MethodInfo registerMethod = types.Count == 0 ? AccessTools.Method(typeof(ZRoutedRpc), nameof(ZRoutedRpc.Register), [typeof(string), GetActionRaw(types.Count + 1)]) : AccessTools.Method(typeof(ZRoutedRpc), nameof(ZRoutedRpc.Register), [typeof(string), GetActionRaw(types.Count + 1)]).MakeGenericMethod(types.ToArray());
                        registerMethod.Invoke(ZRoutedRpc.instance, [rpc.Key.RpcName, actionInstance]);
                    }
                    catch (Exception ex) 
                    {
                        Utils.print($"Error registering RPC: {rpc.Key.RpcName}, method: {rpc.Value}, exception: {ex}", ConsoleColor.Red);
                    }
                }
            } 
        }
    }
}