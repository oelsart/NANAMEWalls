namespace NanameWalls;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HotSwapAllAttribute : Attribute { }

public sealed class HotSwapAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class IgnoreHotSwapAttribute : Attribute { }