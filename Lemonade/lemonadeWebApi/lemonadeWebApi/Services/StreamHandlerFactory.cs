using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;
using Microsoft.Extensions.Options;
using SharpCompress.Archives;

namespace lemonadeWebApi.Services
{
    public class StreamHandlerFactory
    {
        private StreamHandlerConfigurations _config;
        private readonly IArchiver _archiver;

        public StreamHandlerFactory(IOptions< StreamHandlerConfigurations> config,IArchiver archiver)
        {
            _config = config.Value;
            _archiver = archiver;
        }

        public FileHandler CreateFileHandler(string fullPath)
        {
            return new FileHandler(fullPath,_archiver,_config);
        }
        public UrlHandler CreateUrlHandler(string url)
        {
            return new UrlHandler(url, _archiver, _config);
        }
        public StringHandler CreateStringHandler(string body)
        {
            return new StringHandler(body, _archiver, _config);
        }
    }
}
