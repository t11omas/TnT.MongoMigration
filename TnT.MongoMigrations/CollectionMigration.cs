using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TnT.MongoMigrations
{
    public abstract class CollectionMigration : Migration
    {
        protected string CollectionName;

        protected CollectionMigration(long version, string description, string collectionName) : base(version, description)
        {
            CollectionName = collectionName;
        }

        public virtual FilterDefinition<BsonDocument> Filter()
        {
            return Builders<BsonDocument>.Filter.Empty;
        }

        public override void Up(IMongoDatabase database)
        {
            var collection = database.GetCollection<BsonDocument>(CollectionName);
            var documents = GetDocuments(collection);
            UpdateDocuments(collection, documents);
        }

        public virtual void UpdateDocuments(IMongoCollection<BsonDocument> collection, IAsyncCursor<BsonDocument> documents)
        {
            documents.ForEachAsync(document =>
            {
                try
                {
                    UpdateDocument(collection, document);
                }
                catch (Exception exception)
                {
                    OnErrorUpdatingDocument(document, exception);
                }
            }).Wait();
        }

        protected virtual void OnErrorUpdatingDocument(BsonDocument document, Exception exception)
        {
            var message =
                new
                {
                    Message = "Failed to update document",
                    CollectionName,
                    // Id = document.TryGetDocumentId(),
                    MigrationVersion = Version,
                    MigrationDescription = Description
                };
            throw new MigrationException(message.ToString(), exception);
        }

        public abstract void UpdateDocument(IMongoCollection<BsonDocument> collection, BsonDocument document);

        protected virtual IAsyncCursor<BsonDocument> GetDocuments(IMongoCollection<BsonDocument> collection)
        {
            var query = Filter();
            return query != null
                ? collection.Find(query).ToCursor()
                : collection.Find(Builders<BsonDocument>.Filter.Empty).ToCursor();
        }
    }
}