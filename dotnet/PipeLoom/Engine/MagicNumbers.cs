namespace PipeLoom.Engine;

internal static class MagicNumbers
{
    public const int EnginePoolSetSize = 32;

    public const int MemCachedPoolExpirationScanSeconds = 60;

    public const int StepStatePoolSize = 16;
    
    public const int VariantBundlePoolsize = 64;

    public const int EngineReadLockTimeoutGraceMs = 1000;
    public const int EngineWriteLockTimeoutGraceMs = 3000;

    public const int FuseLockWaitMs = 2000;

    public const int MaxSubsetPath = 32;
}