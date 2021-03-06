﻿namespace CoachCue.Repository
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
        private static readonly string DatabaseId = "CoachCueDB";
        //private static readonly string DatabaseId = "coachcue";
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

        #region Games

        public static async Task<Schedule> GetScheduleAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, "Players", id));
                return (Schedule)(dynamic)document;
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

        #endregion

        #region Users

        public static IEnumerable<Models.User> GetNotificationsByMatchup(string matchupId)
        {
            return client.CreateDocumentQuery<Models.User>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Users"))
                    .SelectMany(s => s.Notifications.Where(c => c.Matchup == matchupId && c.Read == false && c.Sent == false).Select(c => s)).ToList();
        }

        #endregion

        #region Messages

        public static IEnumerable<Message> GetPlayerMessages(string playerId)
        {
            var messages = client.CreateDocumentQuery<Message>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Messages"))
                    .SelectMany(s => s.PlayerMentions.Where(c => c.Id == playerId).Select(c => s));

            return messages;
        }

        #endregion

        #region Matchup

        public static IEnumerable<Matchup> GetPlayerMatchups(string playerId)
        {
            var matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                    .SelectMany(s => s.Players.Where(c => c.Id == playerId).Select(c => s));

            return matchups;
        }

        public static IEnumerable<Matchup> GetPositionMatchups(string position)
        {
            var matchups = new List<Matchup>();

            if (position == "WR")
            {
                matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                        .SelectMany(s => s.Players.Where(c => c.Position == position || c.Position == "TE").Select(c => s)).Take(200).ToList(); ;
            }
            else if (position == "DEF")
            {
                matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                        .SelectMany(s => s.Players.Where(c => c.Position == position || c.Position == "K").Select(c => s)).Take(200).ToList(); ;
            }
            else
            {
                matchups = client.CreateDocumentQuery<Matchup>(UriFactory.CreateDocumentCollectionUri(DatabaseId, "Matchups"))
                        .SelectMany(s => s.Players.Where(c => c.Position == position).Select(c => s)).Take(200).ToList();
            }

            return matchups.GroupBy(mt => mt.Id).Select(grp => grp.First()).Take(100).ToList();
        }

        #endregion
    }
}