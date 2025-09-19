using StackExchange.Redis;

namespace ProductAPI.Helpers
{
    public class UniqueIdGenerator  : IUniqueIdGenerator
    {
        private readonly IDatabase _redisDb;
        private const string RedisKey = "product_ids";

        public UniqueIdGenerator(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task<int> GenerateUniqueIdAsync()
        {
            // Check if Redis set is full (all possible 6-digit numbers used)
            long count = await _redisDb.SetLengthAsync(RedisKey);
            if (count >= 900000)
            {
                throw new Exception("Database full: All possible 6-digit IDs are used.");
            }

            var random = new Random();
            int id;
            do
            {
                id = random.Next(100000, 1000000); // 6-digit number
            }
            while (await _redisDb.SetContainsAsync(RedisKey, id));

            await _redisDb.SetAddAsync(RedisKey, id);
            return id;
        }
    }
}