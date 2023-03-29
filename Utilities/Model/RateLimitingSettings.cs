namespace Utilities.Model
{
    public class RateLimitingSettings
    {
        public const string UserBasedTokenBucket = "RateLimiter:RateLimitUserBasedTokenBucket";
        public const string UserBasedSlidingWindow = "RateLimiter:RateLimitUserBasedSlidingWindow";
        public const string RateLimitGlobalFixedWindow = "RateLimiter:RateLimitGlobalFixedWindow";
        public const string TokenBucket = "RateLimiter:RateLimitTokenBucket";
        public const string Concurrency = "RateLimiter:RateLimitConcurrency";
        public const string SlidingWindow = "RateLimiter:RateLimitSlidingWindow";
        public const string FixedWindow = "RateLimiter:RateLimitFixedWindow";
        public int PermitLimit { get; set; } = 100;
        public int Window { get; set; } = 10;
        public int ReplenishmentPeriod { get; set; } = 2;
        public int QueueLimit { get; set; } = 2;
        public int SegmentsPerWindow { get; set; } = 8;
        public int TokenLimit { get; set; } = 10;
        public int TokensPerPeriod { get; set; } = 4;
        public bool AutoReplenishment { get; set; } = false;
        public string PolicyName { get; set; } = string.Empty;
    }
}