using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using MongoDB.Driver;

namespace TnT.MongoMigrations
{
    public class Migrator
    {
        private ILog log = LogManager.GetLogger<Migrator>();

        public Migrator(string mongoServerLocation, string databaseName)
            : this(new MongoClient(mongoServerLocation).GetDatabase(databaseName))
        {
        }

        public Migrator(IMongoDatabase database)
        {
            Database = database;
            DatabaseStore = new DatabaseMigrationStore(database);
            MigrationFinder = new MigrationFinder();
        }

        public IMongoDatabase Database { get; set; }
        public MigrationFinder MigrationFinder { get; set; }
        public DatabaseMigrationStore DatabaseStore { get; set; }

        public virtual void FindMigrationsInAssemblyOfType<T>()
        {
            MigrationFinder.FindMigrationsInAssemblyOfType<T>();
        }

        public virtual void UpdateToLatest()
        {
            this.log.Info(WhatWeAreUpdating() + " to latest...");
            UpdateTo(MigrationFinder.LatestVersion());
        }

        private string WhatWeAreUpdating()
        {
            return $"Updating server(s) \"{ServerAddresses()}\" for database \"{Database.DatabaseNamespace}\"";
        }

        private string ServerAddresses()
        {
            return string.Join(",", Database.Client.Settings.Servers.Select(s => s.Host.ToString()));
        }

        protected virtual void ApplyMigrations(IEnumerable<Migration> migrations)
        {
            migrations.ToList()
                .ForEach(ApplyMigration);
        }

        protected virtual void ApplyMigration(Migration migration)
        {
            this.log.Info(new { Message = "Applying migration", migration.Version, migration.Description, DatabaseName = Database.DatabaseNamespace });

            var appliedMigration = DatabaseStore.StartMigration(migration);
            try
            {
                migration.Up(Database);
            }
            catch (Exception exception)
            {
                OnMigrationException(migration, exception);
            }
            DatabaseStore.CompleteMigration(appliedMigration);
        }

        protected virtual void OnMigrationException(Migration migration, Exception exception)
        {
            var message = new
            {
                Message = "Migration failed to be applied: " + exception.Message,
                migration.Version,
                Name = migration.GetType(),
                migration.Description,
                DatabaseName = Database.DatabaseNamespace
            };
            this.log.Error(message);
            throw new MigrationException(message.ToString(), exception);
        }

        public virtual void UpdateTo(long updateToVersion)
        {
            if (!IsNotLatestVersion())
            {
                return;
            }

            var currentVersion = DatabaseStore.GetLastAppliedMigration();
            this.log.Info(new { Message = WhatWeAreUpdating(), currentVersion, updateToVersion, DatabaseName = Database.DatabaseNamespace });

            var migrations = MigrationFinder.GetMigrationsAfter(currentVersion)
                .Where(m => m.Version <= updateToVersion);

            ApplyMigrations(migrations);
        }

        public virtual bool IsNotLatestVersion()
        {
            return MigrationFinder.LatestVersion()
                   != DatabaseStore.GetVersion();
        }

        public virtual void ThrowIfNotLatestVersion()
        {
            if (!IsNotLatestVersion())
            {
                return;
            }
            var databaseVersion = DatabaseStore.GetVersion();
            var migrationVersion = MigrationFinder.LatestVersion();
            throw new ApplicationException("Database is not the expected version, database is at version: " + databaseVersion + ", migrations are at version: " + migrationVersion);
        }
    }
}