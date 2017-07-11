namespace SignatureApp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class ThreadService : IDisposable
    {
        private readonly LinkedList<Thread> _workers;
        private readonly LinkedList<Action> _tasks;

        private bool _disposed;
        private bool _stopped;

        public ThreadService(int threadNumber)
        {
            this._workers = new LinkedList<Thread>();
            this._tasks = new LinkedList<Action>();

            for (int i = 0; i < threadNumber; i++)
            {
                var worker = new Thread(this.Worker);
                worker.Start();

                this._workers.AddLast(worker);
            }
        }

        public void AddTask(Action task)
        {
            lock (this._tasks)
            {
                if (this._stopped)
                {
                    throw new InvalidOperationException(
                        "This threadService is in the process of being disposed");
                }

                if (this._disposed)
                {
                    throw new ObjectDisposedException(
                        "ThreadService",
                        "This threadService has already been disposed");
                }

                this._tasks.AddLast(task);
                Monitor.PulseAll(this._tasks);
            }
        }

        public void Dispose()
        {
            var awaiting = false;

            lock (this._tasks)
            {
                if (!this._disposed)
                {
                    GC.SuppressFinalize(this);

                    this._stopped = true;

                    while (this._tasks.Count > 0)
                    {
                        Monitor.Wait(this._tasks);
                    }

                    this._disposed = true;

                    Monitor.PulseAll(this._tasks);
                    awaiting = true;
                }
            }

            if (awaiting)
            {
                foreach (var worker in this._workers)
                {
                    worker.Join();
                }
            }
        }

        private void Worker()
        {
            var task = null as Action;

            while (true)
            {
                lock (this._tasks)
                {
                    while (true)
                    {
                        if (this._disposed)
                        {
                            return;
                        }

                        var equals = object.ReferenceEquals(Thread.CurrentThread, this._workers.First.Value);

                        if (null != this._workers.First && this._tasks.Count > 0 && equals)
                        {
                            task = this._tasks.First.Value;
                            this._tasks.RemoveFirst();
                            this._workers.RemoveFirst();
                            Monitor.PulseAll(this._tasks);
                            break;
                        }

                        Monitor.Wait(this._tasks);
                    }
                }

                task();

                lock (this._tasks)
                {
                    this._workers.AddLast(Thread.CurrentThread);
                }

                task = null;
            }
        }
    }
}
