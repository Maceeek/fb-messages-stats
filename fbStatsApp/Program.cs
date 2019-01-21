using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbStatsApp
{
    internal class Program
    {
        private string[] jsonForCharts = new string[9];
        private const string serverPrefix = "http://127.0.0.1:4004/";
        private string preparedMessageFile = "null";
        private List<FileInfo> fileInfos = new List<FileInfo>();
        private List<string> existingParticipants = new List<string>();
        private string selectedIdentity = "unknown";


        private int consoleStartIndex = 0;
        private static void Main(string[] args)
        {
            Program p = new Program();
           if(p.SearchForMessageFiles())
                p.StartWebInterface();




            Console.ReadKey();
        }

        private void StartWebInterface()
        {
            Console.WriteLine("Opening browser to show you the data");
            try
            {
                var listener = new HttpListener();
                listener.Prefixes.Add(serverPrefix);
                listener.Start();

                try
                {
                    Process proc = new Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = serverPrefix;
                    proc.Start();

                }
                catch
                {
                    Console.WriteLine("Could not open web browser, please navigate to {0}", serverPrefix);
                }

                consoleStartIndex = Console.CursorTop;
                webInterface(listener); //listener loop

            }
            catch
            {
                Console.WriteLine("Could not start web interface!");
            }

        }

        public bool SearchForMessageFiles()
        {
            try
            {
                Console.WriteLine("Searching for message files...");
                var messagesFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "message.json", SearchOption.AllDirectories);
                int uP = 1;
                foreach (var filePath in messagesFiles)
                {
                    var pSplit = filePath.Split('\\');

                    int ind = pSplit[pSplit.Length - 2].IndexOf('_');

                    string person = "unknownPerson" + uP++.ToString();
                    if (ind != -1)
                    {
                        person = pSplit[pSplit.Length - 2].Substring(0, ind);
                    }

                    double size = Math.Round(((double)new System.IO.FileInfo(filePath).Length) / 1000000, 3); //in MB

                    fileInfos.Add(new FileInfo { realPath = filePath, fileSize = size, personName = person });

                }
              

                fileInfos = fileInfos.OrderByDescending(x => x.fileSize).ToList();



                // we will put all chat participants in a list, the most common is the user using who did the export
                foreach (var file in fileInfos)
                {
                    //we will read only the start of file
                    using (var reader = new StreamReader(file.realPath))
                    {
                        StringBuilder sb = new StringBuilder();
                        bool foundPart = false;
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (!foundPart) foundPart = line.Contains("participants");

                            if (!foundPart) sb.Append(line);
                            else
                            {
                                if (line.Contains("]"))
                                {
                                    sb.Append("]}");
                                    break;
                                }
                                else sb.Append(line);
                            }

                        }


                        var jsonObj = JsonConvert.DeserializeObject<MessagesObject>(sb.ToString());
                        if (jsonObj.participants != null)
                            foreach (var participant in jsonObj.participants)
                            {
                                existingParticipants.Add(GetRidOfEncodings(participant.name));
                            }

                    }
                }
                existingParticipants = existingParticipants.GroupBy(q => q).OrderByDescending(gp => gp.Count()).Select(g => g.Key).ToList();

                int i = 1;
                Console.WriteLine("most occuring users:");
                foreach (var user in existingParticipants.Take(3)) Console.WriteLine("[{0}] {1}", i++, user);
                Console.WriteLine("you are [1] {0} ", existingParticipants[0]);
                selectedIdentity = existingParticipants[0];
                return true;

            }catch
            {
                Console.WriteLine("unable to find message files, please place this program into the folder of your data export");
                return false;
            }

        }

       

        public async void GetDataFromMessageFile(object o)
        {
            Console.CursorTop = consoleStartIndex +1;
        
            string messFilePath = (string)o;
            Console.WriteLine("getting data for user: {0}", fileInfos.Find(x => x.realPath == messFilePath).personName);

            for (int arr = 0; arr < jsonForCharts.Count(); arr++)
            {
                jsonForCharts[arr] = "null";
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string data = String.Empty;
            using (var reader = new StreamReader(messFilePath))
            {
                data = reader.ReadToEnd();
            }

            Console.WriteLine("{0}ms read all data", stopwatch.ElapsedMilliseconds);

            MessagesObject jsonObj = JsonConvert.DeserializeObject<MessagesObject>(data);

            Console.WriteLine("{0}ms deserialized JSON", stopwatch.ElapsedMilliseconds);

            

            

            int i = 0;
            StringBuilder stringBuilder = new StringBuilder();
            HashSet<Char> charsToRemove = new HashSet<Char> { '!', '-', '"', '?', ',', '/', '.', '\\', '§' };

            for (i = jsonObj.messages.Count - 1; i >= 0; i--)
            {
                var mess = jsonObj.messages[i];
                if (mess.content == null) //remove empty messages
                {
                    jsonObj.messages.RemoveAt(i);
                }
                else
                {
                    string veta = GetRidOfEncodings(mess.content);

                    stringBuilder.Clear();
                    bool lastCharSpace = false;
                    foreach (Char ch in veta)
                    {

                        if (charsToRemove.Contains(ch) || Char.IsWhiteSpace(ch))
                        {
                            if (!lastCharSpace)
                            {
                                stringBuilder.Append(' ');
                                lastCharSpace = true;
                            }
                        }
                        else
                        {
                            stringBuilder.Append(Char.ToLower(ch));
                            lastCharSpace = false;
                        }

                    }
                    veta = stringBuilder.ToString();

                    mess.content = veta;

                    if (mess.sender_name != null)
                    {
                        mess.sender_name = GetRidOfEncodings(mess.sender_name);
                    }
                    else
                    {
                        mess.sender_name = "deleted account";
                    }
                }


            }
            Console.WriteLine("{0}ms cleaned JSON contents", stopwatch.ElapsedMilliseconds);

            List<Message> messages = jsonObj.messages;

            messages = messages.OrderBy(x => x.timestamp_ms).ToList();

            Console.WriteLine("{0}ms list ordered by timestamp", stopwatch.ElapsedMilliseconds);

            int w, c;
            ThreadPool.GetMinThreads(out w, out c);

            var countTask1 = Task.Factory.StartNew(() => CountWordsDoubleCores(messages, x => x.sender_name == selectedIdentity, w >= 8));
            var countTask2 = Task.Factory.StartNew(() => CountWordsDoubleCores(messages, x => x.sender_name != selectedIdentity, w >= 8));

            if (w >= 8)
            {
                Console.WriteLine("{0}ms counting will run on 8 threads", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                Console.WriteLine("{0}ms counting will run on 4 threads", stopwatch.ElapsedMilliseconds);
            }


            var info = new GeneralStats();

            /* zpravy za den */
            var count1 = new List<int>();
            var count2 = new List<int>();
            var time = new List<string>();

            var timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            var timePrev = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            var currDay = timeNow.Day;

            int c1 = 0;
            int c2 = 0;

            for (i = 1; i < messages.Count; i++)
            {

                var thisAday = timeNow.Day;
                if (thisAday != currDay)
                {
                    count1.Add(c1);
                    count2.Add(c2);
                    c1 = 0;
                    c2 = 0;
                    time.Add(timePrev.Day + ". " + (timePrev.Month) + ". " + (timePrev.Year - 2000));


                    var diffSec = ((timePrev - timeNow).TotalSeconds);
                    while (diffSec > 86400) //rozdil je vetsi jak den
                    {

                        count1.Add(0);
                        count2.Add(0);
                        time.Add(timePrev.Day + ". " + (timePrev.Month) + ". " + (timePrev.Year - 2000));


                        timePrev = timePrev.AddSeconds(86400);
                        diffSec = ((timePrev - timeNow).TotalSeconds);
                    }






                    currDay = thisAday;
                }

                if (messages[i].sender_name == selectedIdentity)
                {
                    c2++;
                }
                else
                {
                    c1++;
                }

                timePrev = timeNow;
                timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[i].timestamp_ms));
            }

            jsonForCharts[0] = JsonConvert.SerializeObject(new GraphData { count1 = count1, count2 = count2, dates = time });

            Console.WriteLine("{0}ms got messages per day count", stopwatch.ElapsedMilliseconds);

            /* pocet zprav za mesic */
            count1 = new List<int>();
            count2 = new List<int>();
            var prevCount1 = new List<int>();
            var prevCount2 = new List<int>();
            time = new List<string>();

            timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            timePrev = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            var currMonth = timeNow.Month;

            c1 = 0;
            c2 = 0;

            int pc1 = 0;
            int pc2 = 0;

            if (messages[0].sender_name == selectedIdentity)
            {
                c2++;
                info.totalCountMy++;
            }
            else
            {
                c1++;
                info.totalCountP++;
            }


            for (i = 1; i < messages.Count; i++)
            {
                var thisM = timeNow.Month;
                if (thisM != currMonth)
                {
                    count1.Add(c1);
                    count2.Add(c2);
                    prevCount1.Add(c1 - pc1);
                    prevCount2.Add(c2 - pc2);
                    pc1 = c1;
                    pc2 = c2;
                    c1 = 0;
                    c2 = 0;
                    time.Add((timePrev.Month) + ". " + (timePrev.Year - 2000));
                    currMonth = thisM;
                }

                if (messages[i].sender_name == selectedIdentity)
                {
                    c2++;
                    info.totalCountMy++;
                }
                else
                {
                    c1++;
                    info.totalCountP++;
                }

                timePrev = timeNow;
                timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[i].timestamp_ms));
            }
            jsonForCharts[1] = JsonConvert.SerializeObject(new GraphData { count1 = count1, count2 = count2, dates = time });
            jsonForCharts[2] = JsonConvert.SerializeObject(new GraphData { count1 = prevCount1, count2 = prevCount2, dates = time });

            Console.WriteLine("{0}ms got messages per month count", stopwatch.ElapsedMilliseconds);

            count1 = new List<int>();
            count2 = new List<int>();
            time = new List<string>();

            timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            timePrev = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms));
            var currY = timeNow.Year;

            c1 = 0;
            c2 = 0;


            for (i = 1; i < messages.Count; i++)
            {

                var thisY = timeNow.Year;
                if (thisY != currY)
                {
                    count1.Add(c1);
                    count2.Add(c2);

                    c1 = 0;
                    c2 = 0;
                    time.Add((timePrev.Year.ToString()));




                    currY = thisY;
                }

                if (messages[i].sender_name == selectedIdentity)
                {
                    c2++;
                }
                else
                {
                    c1++;
                }

                timePrev = timeNow;
                timeNow = DateTimeOffset.FromUnixTimeMilliseconds((messages[i].timestamp_ms));
            }

            jsonForCharts[3] = JsonConvert.SerializeObject(new GraphData { count1 = count1, count2 = count2, dates = time });

            Console.WriteLine("{0}ms got messages per year count", stopwatch.ElapsedMilliseconds);

            info.firstTime = DateTimeOffset.FromUnixTimeMilliseconds((messages[0].timestamp_ms)).ToString("dd. MM. yyyy HH:mm:ss");
            info.lastTime = DateTimeOffset.FromUnixTimeMilliseconds((messages[messages.Count() - 1].timestamp_ms)).ToString("dd. MM. yyyy HH:mm:ss");
            jsonForCharts[8] = JsonConvert.SerializeObject(info);

            preparedMessageFile = messFilePath;


            var wordList1 = await countTask1;
            var wordList2 = await countTask2;

            Console.WriteLine("{0}ms got word counts {1}", stopwatch.ElapsedMilliseconds, wordList1.Count);

            var counts1_2 = new List<int>(); //matching counts from counts 2
            var counts2_1 = new List<int>(); //matching counts from counts 1

            var counts1 = wordList1.Select(x => x.count).ToList();
            var texts1 = wordList1.Select(x => x.text).ToList();
            var counts2 = wordList2.Select(x => x.count).ToList();
            var texts2 = wordList2.Select(x => x.text).ToList();

            for (i = 0; i < texts1.Count; i++)
            {
                int index = texts2.FindIndex(x => x == texts1[i]);
                if (index == -1)
                {
                    counts1_2.Add(0);
                }
                else
                {
                    counts1_2.Add(counts2[index]);
                }
            }

            for (i = 0; i < texts2.Count; i++)
            {
                int index = texts1.FindIndex(x => x == texts2[i]);
                if (index == -1)
                {
                    counts2_1.Add(0);
                }
                else
                {
                    counts2_1.Add(counts1[index]);
                }
            }

            Console.WriteLine("{0}ms got word counts relatives", stopwatch.ElapsedMilliseconds);

            
           

            jsonForCharts[4] = JsonConvert.SerializeObject(new GraphData { count1 = counts1, count2 = new List<int>(), dates = texts1 });
            jsonForCharts[5] = JsonConvert.SerializeObject(new GraphData { count1 = counts2, count2 = new List<int>(), dates = texts2 });
            jsonForCharts[6] = JsonConvert.SerializeObject(new GraphData { count1 = counts1, count2 = counts1_2, dates = texts1 });
            jsonForCharts[7] = JsonConvert.SerializeObject(new GraphData { count1 = counts2, count2 = counts2_1, dates = texts2 });

           

            Console.WriteLine("{0}ms serialized all jsons", stopwatch.ElapsedMilliseconds);












        }

        private string GetRidOfEncodings(string str)
        {
            var rightString = Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-1").GetBytes(str));
            return Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-8").GetBytes(rightString));
        }

        private List<Slovo> CountWordsDoubleCores(List<Message> mess, Func<Message, bool> predicate, bool nestItself = false, bool isNested = false)
        {
            List<Message> messages;
            if (predicate != null)
            {
                messages = mess.Where(predicate).ToList();
            }
            else
            {
                messages = mess;
            }

            var halflen = messages.Count / 2;
            Task<List<Slovo>> countTask1;
            Task<List<Slovo>> countTask2;

            if (nestItself)
            {
                countTask1 = Task.Factory.StartNew(() => CountWordsDoubleCores(messages.Take(halflen).ToList(), null, false, true));
                countTask2 = Task.Factory.StartNew(() => CountWordsDoubleCores(messages.Skip(halflen).ToList(), null, false, true));
            }
            else
            {
                countTask1 = Task.Factory.StartNew(() => CountWords(messages.Take(halflen).ToList(), null));
                countTask2 = Task.Factory.StartNew(() => CountWords(messages.Skip(halflen).ToList(), null));
            }


            countTask1.Wait();
            countTask2.Wait();

            var wordList1 = countTask1.Result;
            var wordList2 = countTask2.Result;

            var groupedData = wordList1.Concat(wordList2).GroupBy(x => x.text);
            var selectedData = groupedData.Select(x => new Slovo
            {
                text = x.Key,
                count = x.Sum(i => i.count)
            });

            if (!isNested)
            {
                selectedData = selectedData.OrderByDescending(x => x.count);
                selectedData = selectedData.Take(800); //we only show 800 record max
            }
            return selectedData.ToList();
        }

        private List<Slovo> CountWords(List<Message> mess, Func<Message, bool> predicate)
        {
            var SlovaArray = new List<Slovo>();
            List<Message> messages;
            if (predicate != null)
            {
                messages = mess.Where(predicate).ToList();
            }
            else
            {
                messages = mess;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                var veta = messages[i].content;
                var vetaSplit = veta.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int s = 0; s < vetaSplit.Count(); s++)
                {
                    vetaSplit[s] = vetaSplit[s];
                    //if (vetaSplit[s].Length > 3)
                    {
                        var slovo = vetaSplit[s];
                        int index = SlovaArray.FindIndex(x => x.text == slovo);
                        if (index == -1)
                        {
                            SlovaArray.Add(new Slovo { text = slovo, count = 1 });
                        }
                        else
                        {
                            SlovaArray[index].count += 1;
                        }
                    }
                }
            }
            return SlovaArray;
        }

        private void webInterface(object l)
        {

            HttpListener listener = (HttpListener)l;
            string rootDir = Directory.GetCurrentDirectory() + "/fbStatsWeb";
            HttpListenerContext context;
            HttpListenerResponse response;
            while (true)
            {
                context = listener.GetContext();
                response = context.Response;
                response.ContentEncoding = Encoding.UTF8;
                string IP = context.Request.RemoteEndPoint.Address.ToString();
                try
                {
                    byte[] buffer = new byte[1];

                    string pozadavek = context.Request.Url.LocalPath;

                    // predelat na case!!
                    if (!(pozadavek.Contains("json")))
                    {
                        if (pozadavek.Contains("getPrepared"))
                        {
                            buffer = Encoding.UTF8.GetBytes(preparedMessageFile);
                        }
                        else if (pozadavek.Contains("getDropdowns"))
                        {

                            // maybe create a new list without paths

                            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileInfos));
                        }
                        else
                        if (pozadavek.Contains("setPerson"))
                        {
                            string[] pr = pozadavek.Split('$');
                            int numberOfPerson = int.Parse(pr[1]);
                            Thread t = new Thread(GetDataFromMessageFile);
                            t.Start(fileInfos[numberOfPerson].realPath);

                            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileInfos));
                        }
                        else
                        {



                            string page = rootDir + pozadavek;

                            TextReader tr = null;

                            try
                            {
                                tr = new StreamReader(page);
                            }
                            catch
                            {
                                page = rootDir + "/index.html";
                                tr = new StreamReader(page);
                            }
                            finally
                            {
                                string msg = tr.ReadToEnd();
                                tr.Dispose();
                                buffer = Encoding.UTF8.GetBytes(msg);
                            }

                            if (pozadavek.Contains(".css"))
                            {
                                response.ContentType = "text/css";
                            }
                        }
                    }
                    else
                    {


                        string[] pr = pozadavek.Split('$');

                        int numberOfJson = int.Parse(pr[1]);

                        buffer = Encoding.UTF8.GetBytes(jsonForCharts[numberOfJson]);


                    }




                    response.ContentLength64 = buffer.Length;
                    Stream st = response.OutputStream;
                    st.Write(buffer, 0, buffer.Length);

                    st.Dispose();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                context.Response.Close();
                //  GC.Collect();
            }


        }


        public class FileInfo
        {
            public string realPath;
            public string personName;
            public double fileSize;
        }

        public class Slovo
        {
            public string text;
            public int count;
        }

        public class GraphData
        {
            public List<int> count1;
            public List<int> count2;
            public List<string> dates;
        }

        public class GeneralStats
        {
            public string firstTime;
            public string lastTime;
            public int totalCountMy;
            public int totalCountP;
        }


    }


}
