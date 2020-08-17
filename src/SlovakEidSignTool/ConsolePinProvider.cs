using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace SlovakEidSignTool
{
    public class ConsolePinProvider : IPinProvider
    {
        public ConsolePinProvider()
        {

        }

        public SecureString GetBokPin()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("BOK pin: ");

                return ReadPassword('*');
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public SecureString GetKepPin()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("KEP pin: ");

                return ReadPassword('*');
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static SecureString ReadPassword(char mask)
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ };

            SecureString pass = new SecureString();
            char chr = (char)0;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP)
                {
                    if (pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass.RemoveAt(pass.Length - 1);
                    }
                }
                else if (chr == CTRLBACKSP)
                {
                    while (pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass.RemoveAt(pass.Length - 1);
                    }
                }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else
                {
                    pass.AppendChar((char)chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();

            pass.MakeReadOnly();
            return pass;
        }
    }
}
