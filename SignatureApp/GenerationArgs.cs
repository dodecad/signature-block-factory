namespace SignatureApp
{
    using System;

    public class GenerationArgs : EventArgs
    {
        public GenerationArgs(long chunkNumber, string hashValue)
        {
            this.ChunkNumber = chunkNumber;
            this.HashValue = hashValue;
        }

        public long ChunkNumber { get; private set; }

        public string HashValue { get; private set; }
    }
}
