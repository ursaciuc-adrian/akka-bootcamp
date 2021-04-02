using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            var consoleWriterActor = MyActorSystem.ActorOf(Props.Create<ConsoleWriterActor>(), "consoleWriterActor");
            var validationActor = MyActorSystem.ActorOf(Props.Create<ValidationActor>(consoleWriterActor), "validationActor");
            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create<ConsoleReaderActor>(validationActor), "consoleReaderActor");
            
            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
