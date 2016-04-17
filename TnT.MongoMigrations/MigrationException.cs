using System;

namespace TnT.MongoMigrations
{
    public class MigrationException : Exception
    {
        public MigrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}