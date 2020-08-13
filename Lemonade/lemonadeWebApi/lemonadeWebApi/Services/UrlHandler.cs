using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using lemonadeWebApi.Aggragators;
using lemonadeWebApi.Consts;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;

namespace lemonadeWebApi.Services
{
    public class UrlHandler:StreamHandlerBase
    {
        public UrlHandler(string Url, IArchiver archiver, StreamHandlerConfigurations config) : base(archiver, config)
        {
            
            HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(Url);
            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
              _stream = new StreamReader(aResponse.GetResponseStream() ?? 
                                         throw new InvalidOperationException("url does not return stream"));
        }
    }
}
