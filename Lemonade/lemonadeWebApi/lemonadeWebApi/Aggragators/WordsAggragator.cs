using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;

namespace lemonadeWebApi.Aggragators
{
    public class WordsAggragator
    {
        private int _currentCachingSize;
        private readonly int _maxAllowedCacheSizeMB;
        private ConcurrentDictionary<string, int> _wordsAggragator;
        private ConcurrentDictionary<string, int> _trailerWordsAggragator;
        private readonly IArchiver _archiver;
        private readonly string _type;
        private Semaphore _semaphore;
        private bool isCurrentlySavingData;
        public WordsAggragator(IArchiver archiver, string type, int maxCachedSize = 1024*1024)
        {
            _archiver = archiver;
            _trailerWordsAggragator = new ConcurrentDictionary<string, int>();
            _wordsAggragator = new ConcurrentDictionary<string, int>();
            _maxAllowedCacheSizeMB = maxCachedSize;
            _type = type;
            _semaphore = new Semaphore(1,1);
        }

        //TODO need to fix for case where all input start with same letter
        public async Task AddWord(string word)
        {
            try
            {
                _semaphore.WaitOne();
                _currentCachingSize += word.Length;
                var str = word.ToLower();
                AggregateWord(str, isCurrentlySavingData ? _trailerWordsAggragator : _wordsAggragator);
                if (_currentCachingSize > _maxAllowedCacheSizeMB)
                {
                    _currentCachingSize = 0;
                    if (!isCurrentlySavingData)
                    {
                        isCurrentlySavingData = true;
                        await StartSaveAsync();
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
            
        }

        public async Task StartSaveAsync()
        {
            isCurrentlySavingData = true;
            var words = _wordsAggragator.Select(kvp => new WordDto {Word = kvp.Key, Count = kvp.Value});
            await _archiver.AddOrUpdateWordAsync(_type,words);
            var memoryUsed = CopyDictionary(_trailerWordsAggragator, _wordsAggragator);
             _currentCachingSize =  memoryUsed;
            isCurrentlySavingData = false;
        }

        private static int CopyDictionary(ConcurrentDictionary<string, int> source, ConcurrentDictionary<string, int> dest)
        {
            var memoryUsed = 0;
            foreach (var kvp in source)
            {
                memoryUsed += kvp.Key.Length;
                AggregateWord(kvp.Key, dest, kvp.Value);
            }

            return memoryUsed;
        }

        private static void AggregateWord(string word, ConcurrentDictionary<string, int> aggregator,int count = 1)
        {
            if (aggregator.ContainsKey(word))
            {
                aggregator[word]+= count;
                return;
            }
            aggregator[word] = count;
        }
    }
}
