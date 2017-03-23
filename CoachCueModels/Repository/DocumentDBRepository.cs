namespace CoachCue.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Models;

    public static class DocumentDBRepository<T> where T : class
    {
        //private static readonly string DatabaseId = "CoachCueData";
        private static readonly string DatabaseId = "coachcue";
        private static DocumentClient client;

        public static async Task<T> GetItemAsync(string id, string collection)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collection, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, string collection)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, collection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public static async Task<Document> CreateItemAsync(T item, string collection)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collection), item);
        }

        public static async Task<Document> UpdateItemAsync(string id, T item, string collection)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collection, id), item);
        }

        public static async Task DeleteItemAsync(string id, string collection)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collection, id));
        }

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task CreateCollectionIfNotExistsAsync(string collection)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = collection },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        #region Matchup

        public static IEnumerable<Matchup> GetPlayerMatchups(string playerId)
        {
            var matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                    .SelectMany(s => s.Players.Where(c => c.Id == playerId).Select(c => s));

            return matchups;
        }

        public static IEnumerable<Matchup> GetPositionMatchups(string position)
        {
            var matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                    .SelectMany(s => s.Players.Where(c => c.Position == position).Select(c => s));

            return matchups;
        }

        #endregion
    }
}