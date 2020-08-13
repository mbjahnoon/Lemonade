using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lemonadeWebApi.Aggragators;
using lemonadeWebApi.Consts;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace lemonadeWebApi.Services
{
    
    public class FileHandler: StreamHandlerBase
    {
        public FileHandler(string filePath,IArchiver archiver,StreamHandlerConfigurations config):base(archiver,config)
        {
            _stream = new StreamReader(filePath); // Don't handle file accessabillity throughout this class
        }
    }
}
