using CrispyEureka.Domain;
using MongoDB.Driver;

namespace CrispyEureka.Persistence
{
    public class DbContext<TEntity>
        where TEntity : AggregateRoot
    {
        private readonly MongoUrl _mongoUrl;
        private readonly IMongoClient _mongoClient;

        private IMongoDatabase DataBase => _mongoClient.GetDatabase(_mongoUrl.DatabaseName);

        public DbContext(MongoUrl mongoUrl)
        {
            _mongoUrl = mongoUrl;
            _mongoClient = new MongoClient(_mongoUrl);
        }

        public IMongoCollection<TDto> GetCollection<TDto>(string figi)
            where TDto : IDto<TEntity>
        {
            var collectionName = typeof(TEntity).Name.ToLowerInvariant();
            return DataBase.GetCollection<TDto>($"{collectionName}@{figi}");
        }
    }
}