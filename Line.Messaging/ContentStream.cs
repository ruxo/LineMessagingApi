using System.Net.Http.Headers;

namespace Line.Messaging;

/// <summary>
/// Stream object for content such as image, file, etc.
/// </summary>
[PublicAPI]
public class ContentStream(Stream baseStream, HttpContentHeaders contentHeaders) : Stream
{
    protected Stream BaseStream { get; private set; } = baseStream;

    public HttpContentHeaders ContentHeaders { get; } = contentHeaders;

    public override bool CanRead => BaseStream.CanRead;

    public override bool CanSeek => BaseStream.CanSeek;

    public override bool CanWrite => BaseStream.CanWrite;

    public override long Length => BaseStream.Length;

    public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

    public override void Flush() => BaseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public override void SetLength(long value) => BaseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            BaseStream.Dispose();
        base.Dispose(disposing);
    }
}