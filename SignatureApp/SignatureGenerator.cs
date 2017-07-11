namespace SignatureApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;

    public class SignatureGenerator
    {
        private readonly int _degree;

        private volatile bool _cancel;
        private int _chunkSize;
        private int _counter;
        private Stream _stream;

        public SignatureGenerator(int chunkSize = 1024 * 1024)
        {
            this._chunkSize = chunkSize;
            this._degree = Environment.ProcessorCount;
        }

        public delegate void GenerationHandler(object sender, GenerationArgs e);
        public static event GenerationHandler GenerationEvent;

        public int ChunkSize
        {
            get
            {
                return this._chunkSize;
            }
            set
            {
                this._chunkSize = value;
            }
        }

        public void ComputeHash<T>(Stream stream)
            where T : HashAlgorithm, new()
        {
            this._counter = 0;
            this._stream = stream;

            using (var service = new ThreadService(this._degree))
            {
                Action<byte[]> action = ((chunk) =>
                {
                    var algorithm = new T();
                    var hash = algorithm.ComputeHash(chunk);
                    var hashValue = hash.ConvertToHexString();
                    var currIndex = Interlocked.Increment(ref _counter);

                    if (GenerationEvent != null && !this._cancel)
                    {
                        GenerationEvent(this, new GenerationArgs(currIndex, hashValue));
                    }
                });

                foreach (var chunk in this.GetFileChunks())
                {
                    if (!this._cancel)
                    {
                        service.AddTask(() => action(chunk));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Thread.Sleep(1000);
        }

        public void Cancel()
        {
            this._cancel = true;
        }

        protected IEnumerable<byte[]> GetFileChunks()
        {
            this._cancel = false;

            byte[] buffer, additionalBuffer;
            int additionalBytesRead, bytesRead;

            additionalBuffer = new byte[_chunkSize];
            additionalBytesRead = _stream.Read(additionalBuffer, 0, additionalBuffer.Length);

            do
            {
                bytesRead = additionalBytesRead;
                buffer = additionalBuffer;

                additionalBuffer = new byte[_chunkSize];
                additionalBytesRead = _stream.Read(additionalBuffer, 0, additionalBuffer.Length);

                yield return buffer;
            }
            while (additionalBytesRead != 0);
        }
    }
}
