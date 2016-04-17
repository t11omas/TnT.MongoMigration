using MongoDB.Driver;

namespace TnT.MongoMigrations
{
    public abstract class Migration
    {
        protected Migration(long version, string description)
        {
            Version = version;
            Description = description;
        }

        public long Version { get; protected set; }
        public string Description { get; protected set; }

        public abstract void Up(IMongoDatabase database);

        public virtual void Down(IMongoDatabase database)
        {
            
        }
    }
}