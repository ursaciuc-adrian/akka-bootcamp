using Akka.Actor;

namespace WinTail
{
    internal class TailCoordinatorActor : UntypedActor
    {
        #region Message types
        
        /// <summary>
        /// Start tailing the file at user-specified path.
        /// </summary>
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }

            public IActorRef ReporterActor { get; private set; }
        }
        
        /// <summary>
        /// Stop tailing the file at user-specified path.
        /// </summary>
        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; private set; }
        }
        
        #endregion
        
        protected override void OnReceive(object message)
        {
            if (message is StartTail msg)
            {
                Context.ActorOf(Props.Create<TailActor>(msg.ReporterActor, msg.FilePath));
            }
        }
    }
}