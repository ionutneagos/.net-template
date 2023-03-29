using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Utilities.Model;

namespace Utilities.Extensions
{
    public static class ConfigureRateLimiter
    {
        public static void ConfigureRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            _ = bool.TryParse(configuration["RateLimiter:RateLimitGlobalFixedWindow:UseRateLimitGlobalFixedWindow"], out bool useRateLimitGlobalFixedWindow);
            _ = bool.TryParse(configuration["RateLimiter:RateLimitUserBasedTokenBucket:UseUserBasedPolicy"], out bool useUserBasedPolicy);
            if (!useUserBasedPolicy)
                _ = bool.TryParse(configuration["RateLimiter:RateLimitUserBasedSlidingWindow:UseUserBasedPolicy"], out useUserBasedPolicy);

            _ = bool.TryParse(configuration["RateLimiter:RateLimitTokenBucket:UseRateLimitTokenBucket"], out bool useRateLimitTokenBucket);
            _ = bool.TryParse(configuration["RateLimiter:RateLimitConcurrency:UseRateLimitConcurrency"], out bool useRateLimitConcurrency);
            _ = bool.TryParse(configuration["RateLimiter:RateLimitSlidingWindow:UseRateLimitSlidingWindow"], out bool useRateLimitSlidingWindow);
            _ = bool.TryParse(configuration["RateLimiter:RateLimitFixedWindow:UseRateLimitFixedWindow"], out bool useRateLimitFixedWindow);

            services.AddRateLimiter(config =>
            {
                RateLimitingSettings rateLimitingSettings = new RateLimitingSettings();

                // User Based Policy
                if (useUserBasedPolicy)
                {
                    configuration.GetSection(RateLimitingSettings.UserBasedTokenBucket.ToString()).Bind(rateLimitingSettings);

                    // UserBased, TokenBucket
                    config.AddPolicy(rateLimitingSettings.PolicyName ?? "UserBasedTokenBucketPolicy", context =>
                    {
                        if (context.User.Identity?.IsAuthenticated == true)
                        {
                            // UserBased, TokenBucket

                            return RateLimitPartition.GetTokenBucketLimiter(context.User.Identity.Name!, _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = rateLimitingSettings.TokenLimit,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = rateLimitingSettings.QueueLimit,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitingSettings.ReplenishmentPeriod),
                                TokensPerPeriod = rateLimitingSettings.TokensPerPeriod,
                                AutoReplenishment = rateLimitingSettings.AutoReplenishment,
                            });
                        };
                        // UserBased, SlidingWindow
                        configuration.GetSection(RateLimitingSettings.UserBasedSlidingWindow.ToString()).Bind(rateLimitingSettings);

                        return RateLimitPartition.GetSlidingWindowLimiter("anonymous-user", _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingSettings.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingSettings.Window),
                            SegmentsPerWindow = rateLimitingSettings.SegmentsPerWindow,
                            QueueLimit = rateLimitingSettings.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });

                    });
                }

                // Global Limit
                if (useRateLimitGlobalFixedWindow)
                {
                    config.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
                    {
                        configuration.GetSection(RateLimitingSettings.RateLimitGlobalFixedWindow.ToString()).Bind(rateLimitingSettings);
                        IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

                        if (!IPAddress.IsLoopback(remoteIpAddress!))
                        {
                            return RateLimitPartition.GetFixedWindowLimiter
                            (remoteIpAddress!, _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = rateLimitingSettings.PermitLimit,
                                    Window = TimeSpan.FromSeconds(rateLimitingSettings.Window),
                                    QueueLimit = rateLimitingSettings.QueueLimit,
                                    AutoReplenishment = rateLimitingSettings.AutoReplenishment
                                });
                        }

                        return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
                    });
                }

                //Fixed Window
                if (useRateLimitFixedWindow)
                {
                    configuration.GetSection(RateLimitingSettings.FixedWindow.ToString()).Bind(rateLimitingSettings);

                    config.AddFixedWindowLimiter(rateLimitingSettings.PolicyName ?? "FixedWindowPolicy", options =>
                    {
                        options.PermitLimit = rateLimitingSettings.PermitLimit;
                        options.Window = TimeSpan.FromSeconds(rateLimitingSettings.Window);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = rateLimitingSettings.QueueLimit;
                    });
                }

                // Sliding Window
                if (useRateLimitSlidingWindow)
                {
                    configuration.GetSection(RateLimitingSettings.SlidingWindow.ToString()).Bind(rateLimitingSettings);
                    config.AddSlidingWindowLimiter(rateLimitingSettings.PolicyName ?? "SlidingWindowPolicy", options =>
                    {
                        configuration.GetSection(RateLimitingSettings.SlidingWindow).Bind(rateLimitingSettings);
                        options.PermitLimit = rateLimitingSettings.PermitLimit;
                        options.Window = TimeSpan.FromSeconds(rateLimitingSettings.Window);
                        options.SegmentsPerWindow = rateLimitingSettings.SegmentsPerWindow;
                        options.QueueLimit = rateLimitingSettings.QueueLimit;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });
                }

                // Token Bucket
                if (useRateLimitTokenBucket)
                {
                    configuration.GetSection(RateLimitingSettings.TokenBucket.ToString()).Bind(rateLimitingSettings);

                    config.AddTokenBucketLimiter(policyName: rateLimitingSettings.PolicyName ?? "TokenBucketPolicy", options =>
                    {
                        options.TokenLimit = rateLimitingSettings.TokenLimit;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = rateLimitingSettings.QueueLimit;
                        options.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitingSettings.ReplenishmentPeriod);
                        options.TokensPerPeriod = rateLimitingSettings.TokensPerPeriod;
                        options.AutoReplenishment = rateLimitingSettings.AutoReplenishment;
                    });
                }

                // Concurrency
                if (useRateLimitConcurrency)
                {
                    configuration.GetSection(RateLimitingSettings.Concurrency.ToString()).Bind(rateLimitingSettings);

                    config.AddConcurrencyLimiter(policyName: rateLimitingSettings.PolicyName ?? "ConcurrencyPolicy", options =>
                    {
                        options.PermitLimit = rateLimitingSettings.PermitLimit;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = rateLimitingSettings.QueueLimit;
                    });
                }

                // On Rejected
                config.OnRejected = (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
                    .LogWarning("OnRejected: {RequestPath}", context.HttpContext.Request.Path);

                    return new ValueTask();
                };
            });
        }
    }
}
