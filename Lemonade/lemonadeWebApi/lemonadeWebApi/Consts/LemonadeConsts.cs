using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lemonadeWebApi.Consts
{
    public static class LemonadeConsts
    {
        public const char DefaultGroupChar = '~';
        public static readonly char[] CollectionsNames = new char[] {'A','B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '~'};
        public static readonly char[] PunctuationChars = new char[]
        {
            '.',',','?',';',':','!','(',')','[',']','"','_','-','@','#','$','%','^','&','*',//'\''
            '+','=','`','~','\\','/','{','}','>','<',' ','\r','\n','\0'
        };
    }
}
