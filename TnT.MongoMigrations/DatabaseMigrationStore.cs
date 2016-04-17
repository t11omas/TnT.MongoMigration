using System;
using MongoDB.Driver;

namespace TnT.MongoMigrations
{
    public class DatabaseMigrationStore
    {
        private readonly IMongoCollection<AppliedMigration> collection;

        public string VersionCollectionName = "DatabaseVersion";

        public DatabaseMigrationStore(IMongoDatabase database)
        {
            collection = database.GetCollection<AppliedMigration>(VersionCollectionName);
        }

        public virtual long GetVersion()
        {
            var lastAppliedMigration = GetLastAppliedMigration();
            return lastAppliedMigration?.Version ?? 0;
        }

        public virtual AppliedMigration GetLastAppliedMigration()
        {
            return
                collection
                    .Find(Builders<AppliedMigration>.Filter.Empty)
                    .Sort(Builders<AppliedMigration>.Sort.Descending(v => v.Version))
                    .FirstOrDefault();
        }

        public virtual AppliedMigration StartMigration(Migration migration)
        {
            var appliedMigration = new AppliedMigration(migration);
            collection.InsertOne(appliedMigration);
            return appliedMigration;
        }

        public virtual void CompleteMigration(AppliedMigration appliedMigration)
        {
            appliedMigration.CompletedOn = DateTime.Now;
            collection.ReplaceOne(x => x.Version == appliedMigration.Version, appliedMigration);
        }
    }
}