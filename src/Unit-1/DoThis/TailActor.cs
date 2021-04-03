﻿using System;
using System.IO;
using System.Text;
using Akka.Actor;
using WinTail.Helpers;

namespace WinTail
{
    internal class TailActor : UntypedActor
    {
        #region Message types

        /// <summary>
        /// Signal that the file has changed, and we need to 
        /// read the next line of the file.
        /// </summary>
        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; private set; }
        }

        /// <summary>
        /// Signal that the OS had an error accessing the file.
        /// </summary>
        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; private set; }

            public string Reason { get; private set; }
        }

        /// <summary>
        /// Signal to read the initial contents of the file at actor startup.
        /// </summary>
        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; private set; }
            public string Text { get; private set; }
        }

        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case FileWrite _:
                {
                    // move file cursor forward
                    // pull results from cursor to end of file and write to output
                    // (this is assuming a log file type format that is append-only)
                    var text = _fileStreamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(text))
                    {
                        _reporterActor.Tell(text);
                    }

                    break;
                }
                case FileError fe:
                    _reporterActor.Tell($"Tail error: {fe.Reason}");
                    break;
                case InitialRead ir:
                    _reporterActor.Tell(ir.Text);
                    break;
            }
        }
        
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy (
                (int?) 10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // withinTimeRange
                x => // localOnlyDecider
                {
                    return x switch
                    {
                        //Maybe we consider ArithmeticException to not be application critical
                        //so we just ignore the error and keep going.
                        //Error that we cannot recover from, stop the failing actor
                        ArithmeticException _ => Directive.Resume,
                        //In all other cases, just restart the failing actor
                        NotSupportedException _ => Directive.Stop,
                        _ => Directive.Restart
                    };
                });
        }
    }
}