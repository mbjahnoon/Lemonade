using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;
using lemonadeWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lemonadeWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LemonadeApiController : ControllerBase
    {
        
        private readonly ILogger<LemonadeApiController> _logger;
        private readonly StreamHandlerFactory _factory;
        private readonly IArchiver _archiver;
        private readonly IHandlersRunner _runner;
        public LemonadeApiController(ILogger<LemonadeApiController> logger,IArchiver archiver,IHandlersRunner runner, StreamHandlerFactory factory)
        {
            _runner = runner;
            _factory = factory;
            _logger = logger;
            _archiver = archiver;
        }
        [HttpGet("")]
        public string Hello()
        {
            return @"Hi these are the urls:
byFullPath/{fullPath} - > for file stored locally
byUrl/{Url} - > for URL
fromString - > to srting with post request
fromStringGet/{str} -> to string
wordStatistics/{word} - > get word
clear - > clear database";
        }

        [HttpGet("byFullPath/{fullPath}")]
        public bool GetByFullPath(string fullPath)
        {
            // TODO what to return
            _runner.AddHandlerAndRun(_factory.CreateFileHandler(fullPath));
            return true;
        }

        [HttpGet("byUrl/{Url}")]
        public bool GetByUrl(string Url)
        {
            _runner.AddHandlerAndRun(_factory.CreateUrlHandler(Url));
            return true;
        }
        [HttpPost("fromString")]
        public bool GetFromString([FromBody]string str)
        {
            _runner.AddHandlerAndRun(_factory.CreateStringHandler(str));
            return true;
        }
        [HttpGet("fromStringGet/{str}")]
        public bool GetFromStringGet( string str)
        {
            _runner.AddHandlerAndRun(_factory.CreateStringHandler(str));
            return true;
        }

        [HttpGet("wordStatistics/{word}")]
        public int GetWordStatistics(string word)
        {
            return  _archiver.GetWordCount(word).Count;
        }
        [HttpGet("clear")]
        public bool ClearArchiver(string word)
        {
            try
            {
                _archiver.Clear();
                return true;
            }
            catch (Exception e)
            {
                //log this
                
            }
            return false;
        }
    }
}
