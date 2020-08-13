using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lemonadeWebApi.Consts;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;
using MongoDB.Driver;

namespace lemonadeWebApi.Archiver
{
    public class MongoArchiver : IArchiver
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly Semaphore _openConnectionSemaphore;
        private readonly FindOneAndUpdateOptions<WordDto> _options;
        private readonly int _numberOfConcurrentTasks;
        public MongoArchiver()
        {
            _options = new FindOneAndUpdateOptions<WordDto> { IsUpsert = true };
            _client = new MongoClient();
            _db = _client.GetDatabase("lemonade");
            foreach (var c in LemonadeConsts.CollectionsNames)
            {
                if (_db.GetCollection<WordDto>(c.ToString()) != null) continue;
                _db.CreateCollection(c.ToString());
                var collection = _db.GetCollection<WordDto>(c.ToString());
                collection.Indexes.CreateOne(Builders<WordDto>.IndexKeys.Ascending(_ => _.Word));
            }
            _numberOfConcurrentTasks = _client.Settings.MaxConnectionPoolSize / 10;
            _openConnectionSemaphore = new Semaphore(_numberOfConcurrentTasks, _numberOfConcurrentTasks);
        }

        public async Task AddWordAsync(string collectionName, IEnumerable<WordDto> words)
        {
            _openConnectionSemaphore.WaitOne();
            try
            {
                await _db.GetCollection<WordDto>(collectionName).InsertManyAsync(words);
            }
            finally
            {
                _openConnectionSemaphore.Release();
            }
        }
        public async Task AddOrUpdateWordAsync(string collectionName, IEnumerable<WordDto> words)
        {
            _openConnectionSemaphore.WaitOne();
            try
            {
                var wordDtos = words.ToList();
                var numberOfWordsInBulk = _client.Settings.MaxConnectionPoolSize / _numberOfConcurrentTasks;
                foreach (var wordBulk in SplitList<WordDto>(wordDtos, numberOfWordsInBulk))
                {
                    await Task.WhenAll(wordBulk.Select(w => _db.GetCollection<WordDto>(collectionName)
                        .FindOneAndUpdateAsync(new FilterDefinitionBuilder<WordDto>().Eq<string>((WordDto x) => x.Word, w.Word),
                            new UpdateDefinitionBuilder<WordDto>().Inc<int>((x) => x.Count, w.Count), _options)));
                }
            }
            finally
            {
                _openConnectionSemaphore.Release();
            }
        }
        public WordDto GetWordCount(string word)
        {
            _openConnectionSemaphore.WaitOne();
            try
            {
                var firstChar = Char.ToUpper(word.First());
                var collectionName = LemonadeConsts.CollectionsNames.Any(firstChar.Equals)
                    ? firstChar
                    : LemonadeConsts.DefaultGroupChar;

                var res = _db.GetCollection<WordDto>(collectionName.ToString()).Find(
                    new FilterDefinitionBuilder<WordDto>().Eq<string>
                        ((WordDto x) => x.Word, word)).Limit(1).SingleOrDefault();
                return res ?? new WordDto { Count = 0, Word = word };
            }
            finally
            {
                _openConnectionSemaphore.Release();
            }
        }

        public async Task Clear()
        {
            await Task.WhenAll(LemonadeConsts.CollectionsNames.Select
                (c => _db.DropCollectionAsync(c.ToString()))); //TODO only for debug should not run in real context otherwise wait is not good
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
