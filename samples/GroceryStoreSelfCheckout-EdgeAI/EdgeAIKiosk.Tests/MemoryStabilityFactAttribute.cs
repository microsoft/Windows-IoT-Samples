namespace EdgeAIKiosk.Tests;

public sealed class MemoryStabilityFactAttribute : FactAttribute
{
    public MemoryStabilityFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("RUN_MEMORY_STABILITY_TESTS") != "1")
            Skip = "Opt-in stress test. Set RUN_MEMORY_STABILITY_TESTS=1 in the test process environment to enable.";
    }
}

// Process-wide memory measurements must not overlap other tests.
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MemoryStabilityCollection
{
    public const string Name = "Memory stability";
}
