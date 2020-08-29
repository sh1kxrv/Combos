using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Combos
{
    class Program
    {
        private static int maxLines = 0;
        private static volatile int CurrentLines = 0;
        private static volatile int CurrentPart = 1;
        static Program() => Console.Title = "Combo's v0.1";
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (!Directory.Exists("parts"))
                Directory.CreateDirectory("parts");

            ColorWrite("[@] Enter your words across space: ", ConsoleColor.Yellow);
            string words = Console.ReadLine();

            ColorWrite("[@] Enter your separator: ", ConsoleColor.Yellow);
            char separator = char.Parse(Console.ReadLine());

            ColorWrite("[@] Enter max line in one part: ", ConsoleColor.Yellow);
            maxLines = int.Parse(Console.ReadLine());

            HashSet<string> h_words = words.Split(' ').ToHashSet();

            Stopwatch watch = new Stopwatch();
            IAsyncEnumerator<string> combos = AddAllFrom(h_words, separator).GetAsyncEnumerator();
            watch.Start();

            StreamWriter writer = new StreamWriter($"parts//part{CurrentPart}.txt", false);
            while (await combos.MoveNextAsync())
            {
                Console.Title = $"Combo's v0.1 | {CurrentPart}p | {CurrentLines}/{maxLines}";
                await writer.WriteLineAsync(combos.Current);
                if (CurrentLines >= maxLines)
                {
                    CurrentLines = 0;
                    CurrentPart++;
                    writer.Dispose();
                    writer = new StreamWriter($"parts//part{CurrentPart}.txt", false);
                    await Task.Delay(20);
                }
                writer.Flush();
                CurrentLines++;
            }
            writer.Dispose();
            watch.Stop();

            Console.Title = $"Combo's v0.1 | Done! | {CurrentPart}p";
            ColorWriteLine($"[!] Done! Elapsed: {watch.ElapsedMilliseconds / 1000.0}sec", ConsoleColor.Blue);
            Console.Read();
        }

        static void ColorWrite(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
        static void ColorWriteLine(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static async IAsyncEnumerable<string> AddAllFrom(HashSet<string> words, char separator)
        {
            if (words.Count == 0)
                yield return "";
            foreach (var c in words.ToList())
            {
                words.Remove(c);
                await foreach (var s in AddAllFrom(words, separator))
                    yield return c + separator + s;
                words.Add(c);
            }
        }
    }
}
