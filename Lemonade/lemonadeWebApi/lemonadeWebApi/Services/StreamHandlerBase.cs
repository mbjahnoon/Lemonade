using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using lemonadeWebApi.Aggragators;
using lemonadeWebApi.Consts;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;

namespace lemonadeWebApi.Services
{
    /// <summary>
    /// The Idea is to construct a pipeline: stream -> buffer - > in memory aggragators - > saving to archiver 
    /// </summary>
    public abstract class StreamHandlerBase : IStreamHandler
    {
        protected readonly Dictionary<char, WordsAggragator> aggragators = new Dictionary<char, WordsAggragator>();
        protected StreamReader _stream;

        private StreamHandlerConfigurations _config;
        private bool _inDisposing = false;

        private readonly StringBuilder _builder = new StringBuilder();

        // Blocks are part of c# TPL dataFlows. super fast async library for manipulate data.
        // we chain the blocks and each block has a specific task
        // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library 
        private readonly BufferBlock<char> _bufferBlock;
        private readonly ActionBlock<char> _actionBlock;

        //Implementing RAII stram should open in constructor
        protected StreamHandlerBase(IArchiver archiver,StreamHandlerConfigurations config)
        {
            _config = config;
            // creating aggragators per wors this seems like a reasonable load balancing.
            // (don't support the case of all words in string are starting with the same letter)
            // each aggragator holds in memory data and once reach the limit it writes to archiver and free some space
            foreach (var c in LemonadeConsts.CollectionsNames)
            {
                aggragators[c] = new WordsAggragator(archiver, c.ToString());
            }
            aggragators['~'] = new WordsAggragator(archiver, "~"); // for all that doesn't start with a letter
            _bufferBlock = new BufferBlock<char>(); // accepts buffer and pass to the action block
            _actionBlock = new ActionBlock<char>(c => //action block receive word and run this 
            {
                if (LemonadeConsts.PunctuationChars.Any(c.Equals))
                {
                    var word = _builder.ToString();
                    _builder.Clear();
                    if (word.Length > 1)
                    {
                        MoveWordToOpenAggragator(word);
                    }
                    return;
                }
                _builder.Append(c);
            });
            _bufferBlock.LinkTo(_actionBlock); // linking blocks
        }

        public async Task ContinueParse()
        {
            var counter = 1;
            long memorySizeToPause = 1024L * 1024 * 1024 * _config.MaxMemoryUsage; //4gb
            try
            {
                while (!_stream.EndOfStream)
                {
                    if (counter % _config.CheckForMemoryUsageIterationsInterval == 0)
                    {
                        var memory = GC.GetTotalMemory(true);
                        // asp net garbage collector is designed to collect when server is not busy
                        // here we are asking him to work.
                        // normally we would have done something more robust.
                        if (memory > memorySizeToPause)
                        {
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                            GC.WaitForPendingFinalizers();
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            
                            continue;
                        }
                        counter = 1;
                    }
                    counter++;
                    ReadAndPostData();

                    void ReadAndPostData() // using local function as span not allowed to be in an async context.
                    {
                        var buffer = new Span<char>(new char[100]);
                        _stream.ReadBlock(buffer);
                        ProcessBuffer(buffer); // passing the buffer into dataFlow
                    }
                }
            }
            finally
            {
                _bufferBlock.Complete();//bufferBlock is not allowed to post any more data
            }
        }

        private async void MoveWordToOpenAggragator(string word, int recursionLevel = 0)
        {
            // the use of error handeling as part of logic is not pretty and could be expansive.
            // but assuming all words are valid words this is a very rare case and we get good performance.
            try
            {
                await aggragators[Char.ToUpper(word[0])].AddWord(word);
            }
            catch (Exception ex1)
            {
                try
                {
                    //log this
                    await aggragators['~'].AddWord(word.ToString());
                }
                catch (Exception ex2)
                {
                    //log this
                }

            }

        }

        private void ProcessBuffer(Span<char> buffer)
        {
            foreach (var c in buffer)
            {
                _bufferBlock.SendAsync(c);
            }
        }


        protected virtual async Task Dispose(bool disposing)
        {
            if (_inDisposing) return;
            if (disposing)
            {
                _inDisposing = true;
                _bufferBlock.Complete();
                await _bufferBlock.Completion;
                _actionBlock.Complete();
                await _actionBlock.Completion;
                foreach (var agg in aggragators.Values)
                {
                    await agg.StartSaveAsync();
                }
                _stream?.Dispose();
            }
        }

        public void Dispose()
        {
            
            Dispose(true).Wait();
            GC.SuppressFinalize(this);
        }
    }
}
