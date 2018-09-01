using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlovakEidSignTool
{
    public class ConsolePinProvider : IPinProvider
    {
        public ConsolePinProvider()
        {

        }

        public byte[] GetBokPin()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("BOK pin: ");
                string line = ReadPassword('*').Trim();

                return Encoding.UTF8.GetBytes(line);
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public byte[] GetZepPin()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("KEP pin: ");
                string line = ReadPassword('*').Trim();

                return Encoding.UTF8.GetBytes(line);
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static string ReadPassword(char mask)
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ };

            Stack<char> pass = new Stack<char>();
            char chr = (char)0;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP)
                {
                    if (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == CTRLBACKSP)
                {
                    while (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else
                {
                    pass.Push((char)chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }
    }
}
