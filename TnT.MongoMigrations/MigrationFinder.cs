using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TnT.MongoMigrations
{
    public class MigrationFinder
    {
        protected readonly List<Assembly> Assemblies = new List<Assembly>();

        public virtual void FindMigrationsInAssemblyOfType<T>()
        {
            var assembly = typeof(T).Assembly;
            FindMigrationsInAssembly(assembly);
        }

        public void FindMigrationsInAssembly(Assembly assembly)
        {
            if (Assemblies.Contains(assembly))
            {
                return;
            }
            Assemblies.Add(assembly);
        }

        public virtual IEnumerable<Migration> GetAllMigrations()
        {
            return Assemblies
                .SelectMany(GetMigrationsFromAssembly)
                .OrderBy(m => m.Version);
        }

        protected virtual IEnumerable<Migration> GetMigrationsFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes()
                    .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract)
                    .Select(Activator.CreateInstance)
                    .OfType<Migration>();
            }
            catch (Exception exception)
            {
                throw new MigrationException("Cannot load migrations from assembly: " + assembly.FullName, exception);
            }
        }

        public virtual long LatestVersion()
        {
            if (!GetAllMigrations().Any())
            {
                return 0;
            }
            return GetAllMigrations()
                .Max(m => m.Version);
        }

        public virtual IEnumerable<Migration> GetMigrationsAfter(AppliedMigration currentVersion)
        {
            var migrations = GetAllMigrations();

            if (currentVersion != null)
            {
                migrations = migrations.Where(m => m.Version > currentVersion.Version);
            }

            return migrations.OrderBy(m => m.Version);
        }
    }
}