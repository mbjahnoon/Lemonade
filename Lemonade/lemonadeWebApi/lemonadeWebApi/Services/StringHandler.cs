using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lemonadeWebApi.Interfaces;
using lemonadeWebApi.Models;

namespace lemonadeWebApi.Services
{
    public class StringHandler:StreamHandlerBase
    {
        public StringHandler(string str,IArchiver archiver, StreamHandlerConfigurations config) : base(archiver, config)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(str);
            _stream = new StreamReader(new MemoryStream(byteArray));
        }

    }
}
