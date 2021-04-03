﻿using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    internal class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Messages.InputError error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error.Reason);
                    break;
                case Messages.InputSuccess success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(success.Reason);
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }

            Console.ResetColor();
        }
    }
}
