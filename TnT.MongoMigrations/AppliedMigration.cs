using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TnT.MongoMigrations
{
    public class AppliedMigration
    {
        public AppliedMigration()
        {
        }

        public AppliedMigration(Migration migration)
        {
            Version = migration.Version;
            StartedOn = DateTime.Now;
            Description = migration.Description;
        }

        [BsonId]
        public long Version { get; set; }

        public string Description { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }

        public override string ToString()
        {
            return Version + " started on " + StartedOn + " completed on " + CompletedOn;
        }
    }
}