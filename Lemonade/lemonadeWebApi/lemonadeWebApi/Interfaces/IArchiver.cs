using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lemonadeWebApi.Models;

namespace lemonadeWebApi.Interfaces
{
    public interface IArchiver
    {
        
        Task AddOrUpdateWordAsync(string collectionName, IEnumerable<WordDto> words);
        WordDto GetWordCount(string word);
        Task Clear();
    }
}
