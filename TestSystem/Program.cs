using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            
            bool isSelectTest = false;
            string selectedTest = "";
            Console.Write("Chose test:\n" +
                     "F for fixed image size test\n" +
                     "D for different image size test\n" +
                     "K for kill server test\n" +
                     "Selected: ");
            while (!isSelectTest)
            {
                string input = Console.ReadLine();
                if (input == "F" || input == "D" || input == "K")
                {
                    isSelectTest = true;
                    selectedTest = input;
                }
                else
                {
                    Console.Write("Incorrect input. Please, input one of this chars: F D K\n" +
                        "Selected: ");
                }
            }
            switch (selectedTest)
            {
                case "F":
                    {
                        FixedSizeTest(args[0]);
                        break;
                    }
                case "D":
                    {
                        DifferendSizeTest(args[0]);
                        break;
                    }
                case "K":
                    {
                        Console.WriteLine("Server doesn't work with {0} users", KillServerTest());
                        break;
                    }
            }
        }

        private static void FixedSizeTest(string filePath)
        {
            List<string> resultTest = new List<string>();
            bool isCorrectPath = false;
            string path = "";
            while (!isCorrectPath)
            {
                Console.Write("Input image file path: ");
                path = Console.ReadLine();
                try
                {
                    Bitmap img = new Bitmap(path);
                    isCorrectPath = true;
                }
                catch
                {
                }
            }
            
            bool isCorrectRPS = false;
            int numberOfRPS = 0;
            while (!isCorrectRPS)
            {
                Console.Write("Input number of requests per second: ");
                try
                {
                    numberOfRPS = Int32.Parse(Console.ReadLine());
                    isCorrectRPS = true;
                }
                catch
                {
                }
            }

            bool isCorrectNumberOfClients = false;
            int numberOfClients = 0;
            while (!isCorrectNumberOfClients)
            {
                Console.Write("Input number of clients: ");
                try
                {
                    numberOfClients = Int32.Parse(Console.ReadLine());
                    isCorrectNumberOfClients = true;
                }
                catch
                {
                }
            }

            var clientsWorkList = new List<Task<TimeSpan>>();

            for (int i = 0; i < numberOfClients; ++i)
            {
                for (int j = 0; j < numberOfRPS; ++j)
                {
                    TestClass client = new TestClass(path);
                    clientsWorkList.Add(new Task<TimeSpan>(() => client.Work()));
                    clientsWorkList.Last().Start();
                }
                Thread.Sleep(1000);
            }
            foreach (var t in clientsWorkList)
            {
                resultTest.Add(t.Result.TotalMilliseconds.ToString());
            }
            File.AppendAllLines(filePath, resultTest);
        }

        private static void DifferendSizeTest(string filePath)
        {
            var random = new Random(DateTime.Now.Millisecond);
            List<string> resultTest = new List<string>();
            bool isCorrectNumberOfImages = false;
            int numberOfImages = 0;
            while (!isCorrectNumberOfImages)
            {
                Console.Write("Input number of requests per second: ");
                try
                {
                    numberOfImages = Int32.Parse(Console.ReadLine());
                    isCorrectNumberOfImages = true;
                }
                catch
                {
                }
            }
            var imagePaths = new List<string>();
            for (int i = 0; i < numberOfImages; ++i)
            {
                bool isCorrectPath = false;
                string path = "";
                while (!isCorrectPath)
                {
                    Console.Write("Input {0} image file path: ",i);
                    path = Console.ReadLine();
                    try
                    {
                        Bitmap img = new Bitmap(path);
                        isCorrectPath = true;
                        imagePaths.Add(path);
                    }
                    catch
                    {
                    }
                }
            }
            bool isCorrectRPS = false;
            int numberOfRPS = 0;
            while (!isCorrectRPS)
            {
                Console.Write("Input number of requests per second: ");
                try
                {
                    numberOfRPS = Int32.Parse(Console.ReadLine());
                    isCorrectRPS = true;
                }
                catch
                {
                }
            }

            bool isCorrectNumberOfClients = false;
            int numberOfClients = 0;
            while (!isCorrectNumberOfClients)
            {
                Console.Write("Input number of clients: ");
                try
                {
                    numberOfClients = Int32.Parse(Console.ReadLine());
                    isCorrectNumberOfClients = true;
                }
                catch
                {
                }
            }

            var clientsWorkDictionary = new Dictionary<int,Task<TimeSpan>>();

            for (int i = 0; i < numberOfClients; ++i)
            {
                for (int j = 0; j < numberOfRPS; ++j)
                {
                    TestClass client = new TestClass(imagePaths[random.Next(imagePaths.Count)]);
                    clientsWorkDictionary.Add(client.ImageSize,new Task<TimeSpan>(() => client.Work()));
                    clientsWorkDictionary.Last().Value.Start();
                }
                Thread.Sleep(1000);
            }
            foreach (var t in clientsWorkDictionary)
            {
                foreach (var size in clientsWorkDictionary.Keys)
                {
                    resultTest.Add(t.Key.ToString());
                    if (clientsWorkDictionary[size] == t.Value)
                    {
                        resultTest.Add(t.Value.Result.TotalMilliseconds.ToString());
                    } 
                }
            }
            File.AppendAllLines(filePath, resultTest);
        }

        private static int KillServerTest()
        {
            int result = 0;
            bool isCorrectPath = false;
            string path = "";
            while (!isCorrectPath)
            {
                Console.Write("Input image file path: ");
                path = Console.ReadLine();
                try
                {
                    Bitmap img = new Bitmap(path);
                    isCorrectPath = true;
                }
                catch
                {
                }
            }

            var clientsWorkList = new List<Task<TimeSpan>>();
            bool isContainsErrorTask = false;

            while (!isContainsErrorTask)
            {
                try
                {
                    TestClass client = new TestClass(path);
                    clientsWorkList.Add(new Task<TimeSpan>(() => client.Work()));
                    clientsWorkList.Last().Start();
                    if (DateTime.Now.Second > 50)
                    {
                        foreach (var t in clientsWorkList.Where((t) => t.Status == TaskStatus.Running))
                        {
                            t.Wait();
                        }
                        if (clientsWorkList.Where((t) => (t.Status == TaskStatus.Faulted || t.Status == TaskStatus.Canceled)).Count() > 0)
                        {
                            result = clientsWorkList.IndexOf(clientsWorkList.First((t) => (t.Status == TaskStatus.Faulted || t.Status == TaskStatus.Canceled)));
                            isContainsErrorTask = true;
                        }
                    }
                }
                catch (Exception)
                { 
                    if (clientsWorkList.Where((t) => (t.Status == TaskStatus.Faulted || t.Status == TaskStatus.Canceled)).Count() > 0)
                    {
                        result = clientsWorkList.IndexOf(clientsWorkList.First((t) => (t.Status == TaskStatus.Faulted || t.Status == TaskStatus.Canceled)));
                        isContainsErrorTask = true;
                    }
                }
            }

            return result;
        }
    }
}
