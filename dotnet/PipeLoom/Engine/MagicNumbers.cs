using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PipeLoom.Engine;

internal static class MagicNumbers
{
    public const int EnginePoolSetSize = 32;

    public const int MemCachedPoolExpirationScanSeconds = 60;

    public const int StepStatePoolSize = 16;
    public const int BundleStatePoolSize = 32;
    public const int BundlePoolSize = 32;
    public const int PartitionPathPoolSize = 128;
    public const int LeasedListPoolSize = 16;
    
    public const int VariantBundlePoolsize = 64;

    public const int EngineReadLockTimeoutGraceMs = 1000;
    public const int EngineWriteLockTimeoutGraceMs = 3000;

    public const int FuseLockWaitMs = 2000;

    public const int MaxSubsetPath = 32;

    public const int MaxBundlePartitionLevels = 12;

    public const int BundleFactoryPoolSize = 12;

    public const int CapacityForUnknownEnumerable = 16;
    public const int MinimumLeasedListCapacity = 16;

    public const int MaxArrayPoolArrayLength = 1024 * 1024;
    public const int MaxArrayPoolBucketSize = 150;

    public const int ParallelLinearCutoff = 512;
    public static int ParallelMaxDegree = Math.Min(Environment.ProcessorCount, 4);
    public static bool ParallelForcedLinear => ParallelMaxDegree <= 1;

    public static TimeSpan BundleOpLockWaitTime => Debugger.IsAttached ? TimeSpan.MaxValue : TimeSpan.FromMinutes(1);
    
    public static ParallelOptions DefaultParallelOptions = new() { MaxDegreeOfParallelism = ParallelMaxDegree };
}