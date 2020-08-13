using System;
using System.Linq;
using System.Threading;
using lemonadeWebApi.Archiver;
using lemonadeWebApi.Models;
using lemonadeWebApi.Services;
using Xunit;

namespace LemoTest
{
    public class UnitTest1
    {
        private readonly StreamHandlerConfigurations config =
            new StreamHandlerConfigurations
                {CheckForMemoryUsageIterationsInterval = 1000, MaxMemoryUsage = 1024L * 1024 * 1024 * 4};
        [Fact]
        public void Test1()
        {
            var archiver = new MongoArchiver();
            archiver.Clear();
            {
                using var fileHandler =
                    new FileHandler(@"C:\Users\mbenita\source\repos\lemonadeWebApi\LemoTest\Files\TestTextFile.txt",
                        archiver, config);
                fileHandler.ContinueParse();
            }
            Thread.Sleep(1000);
            Assert.Equal(2, archiver.GetWordCount("whats").Count);
        }
        [Fact]
        public void Test2()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            using var fileHandler =
                new FileHandler(@"C:\Users\mbenita\source\repos\lemonadeWebApi\LemoTest\Files\MobyDick.txt",
                    archiver, config);
            fileHandler.ContinueParse();
        }
        [Fact]
        public void Test3()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            using var fileHandler =
                new FileHandler(@"C:\Users\mbenita\source\repos\lemonadeWebApi\LemoTest\Files\MobyDick.txt",
                    archiver, config);
            HandlersRunner runner = new HandlersRunner();
            runner.AddHandlerAndRun(fileHandler);
        }

        [Fact]
        public void Test4()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            using var fileHandler =
                new FileHandler(@"C:\Users\mbenita\source\repos\lemonadeWebApi\LemoTest\Files\big_file.txt",
                    archiver, config);
            HandlersRunner runner = new HandlersRunner();
            runner.AddHandlerAndRun(fileHandler);
        }

        [Fact]
        public void Test5()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            var str = "hey hey I'm Here hey";
            var fileHandler =
                    new StringHandler(str,
                        archiver, config);
                HandlersRunner runner = new HandlersRunner();
                runner.AddHandlerAndRun(fileHandler);
            fileHandler.Dispose();
            Thread.Sleep(1000);
            Assert.Equal(3, archiver.GetWordCount("hey").Count);
        }
        [Fact]
        public void Test6()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            var str = "";
            var base_str = "hey I'm Here hey ";
            foreach (var VARIABLE in Enumerable.Range(0, 10000))
            {
                str += base_str;
            }
            var fileHandler =
                new StringHandler(str,
                    archiver, config);
            HandlersRunner runner = new HandlersRunner();
            runner.AddHandlerAndRun(fileHandler);
            fileHandler.Dispose();
            Thread.Sleep(10000);
            Assert.Equal(20000, archiver.GetWordCount("hey").Count);
        }
        [Fact]
        public void Test7()
        {
            var archiver = new MongoArchiver();
            archiver.Clear().Wait();
            {
                using var fileHandler =
                    new UrlHandler(@"http://www.cnet.com/",
                        archiver, config);
                HandlersRunner runner = new HandlersRunner();
                runner.AddHandlerAndRun(fileHandler);
            }

        }

    }
}
