namespace Marketplace;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Market_Autoload(Market_Autoload.Type type, Market_Autoload.Priority priority = Market_Autoload.Priority.Last,
        string InitMethod = "OnInit", string[] OnWatcherNames = null, string[] OnWatcherMethods = null) : Attribute
{
    public enum Priority
    {
        Init, First, Normal, Last
    }
    public enum Type
    {
        Server, Client, Both
    }
    public readonly Priority priority = priority;
    public readonly Type type = type;
    public readonly string InitMethod = InitMethod;
    public readonly string[] OnWatcherNames = OnWatcherNames;
    public readonly string[] OnWatcherMethods = OnWatcherMethods;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClientOnlyPatch : Attribute{}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServerOnlyPatch : Attribute{}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConditionalPatch(string Condition = "Condition") : Attribute
{
    public bool Check(Type t)
    {
        MethodInfo method = AccessTools.Method(t, Condition);
        if (method != null) return (bool)method.Invoke(null, null);
        Utils.print($"Error loading {t.Name} conditional patch, method {Condition} not found", ConsoleColor.Red);
        return false;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class MarketplaceRPC(string RpcName, Market_Autoload.Type type) : Attribute
{
    public readonly string RpcName = RpcName;
    public readonly Market_Autoload.Type type = type;
}