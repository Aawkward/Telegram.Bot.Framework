using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Telegram.Bot.Types.Internal
{
    internal class NonDisposableStream : Stream
    {
        private readonly Stream _baseStream;

        public NonDisposableStream(Stream baseStream)
        {
            _baseStream = baseStream;
            if (_baseStream.CanSeek) _baseStream.Position = 0;
        }

#pragma warning disable CA2215 // Dispose methods should call base class dispose
        protected override void Dispose(bool disposing) { }
#pragma warning restore CA2215 // Dispose methods should call base class dispose

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckAutoReset();
            return _baseStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckAutoReset();
            return _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        private void CheckAutoReset()
        {
            if (_baseStream.CanSeek && _baseStream.Position == _baseStream.Length && _baseStream.Length > 0)
            {
                _baseStream.Position = 0;
            }
        }

        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);
        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }
    }
}
