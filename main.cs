
public abstract class RedisCacheBase
    {
        private readonly ILogger _logger;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        protected RedisCacheBase(ILogger logger, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _connectionMultiplexer = connectionMultiplexer;
        }

        protected async Task<T> SafeExecute<T>(Func<IDatabase, Task<T>> func)
        {
            try
            {
                var db = _connectionMultiplexer.GetDatabase();
                return await func(db);
            }
            catch (RedisTimeoutException ex)
            {
                var waitTimeSeconds = DateTime.Now.Ticks % 10 + 10;
                _logger.LogInformation($"Redis Timeout encountered. Waiting {waitTimeSeconds}s before retrying. Exception body:{ex.Message}");

                // Retry with a delay of 10-20 seconds so not all retry at the same time
                await Task.Delay(TimeSpan.FromSeconds(waitTimeSeconds));

                var db = _connectionMultiplexer.GetDatabase();
                return await func(db);
            }
        }
   }
     public interface IBrandCacheService
    {
        string Get(string code);
    }
 public class BrandCacheService : RedisCacheBase, IBrandCacheService
    {
    private readonly TimeSpan _defaultAccountCacheExpiry;
        private const string CACHE_KEY = "CAI_Brand_Code|{0}";

        public BrandCacheService(ILogger<BrandCacheService> logger, IConnectionMultiplexer connectionMultiplexer) : base(logger, connectionMultiplexer)
        {
            _defaultAccountCacheExpiry = TimeSpan.FromDays(1);
        }
        
            public win_brand Get(string code)
            {
              var key = string.Format(CACHE_KEY, code);

              return SafeExecute(db => db.StringGet(key));
            }
    }
