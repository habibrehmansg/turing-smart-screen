namespace TuringSmartScreenLib;

using System;
using System.IO.Ports;

public sealed class TuringSmartScreenRevisionC2 : IDisposable
{
    private static readonly byte[] CommandHello = [0x01, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xc5, 0xd3];

    //    //public static readonly byte[] OPTIONS = { 0x7d, 0xef, 0x69, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x2d };
    //    public static readonly byte[] RESTART = [0x84, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01];

    private static readonly byte[] CommandSetBrightness = { 0x7b, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

    //    // IMAGE QUERY STATUS
    //    public static readonly byte[] QUERY_STATUS = [0xcf, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01];

    //    // STATIC IMAGE
    //    public static readonly byte[] START_DISPLAY_BITMAP = [0x2c];
    //    public static readonly byte[] PRE_UPDATE_BITMAP = [0x86, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01];
    //    public static readonly byte[] UPDATE_BITMAP = [0xcc, 0xef, 0x69, 0x00, 0x00];

    //    //public static readonly byte[] RESTARTSCREEN = { 0x84, 0xef, 0x69, 0x00, 0x00, 0x00, 0x01 };
    //    public static readonly byte[] DISPLAY_BITMAP = [0xc8, 0xef, 0x69, 0x00, 0x17, 0x70];

    //    //public static readonly byte[] STARTMODE_DEFAULT = { 0x00 };
    //    //public static readonly byte[] STARTMODE_IMAGE = { 0x01 };
    //    //public static readonly byte[] STARTMODE_VIDEO = { 0x02 };
    //    //public static readonly byte[] FLIP_180 = { 0x01 };
    //    //public static readonly byte[] NO_FLIP = { 0x00 };
    //    //public static readonly byte[] SEND_PAYLOAD = { 0xFF };

    public enum Orientation : byte
    {
        Portrait = 0,
        ReversePortrait = 1,
        Landscape = 2,
        ReverseLandscape = 3
    }

    private readonly SerialPort port;

    public TuringSmartScreenRevisionC2(string name)
    {
        port = new SerialPort(name)
        {
            DtrEnable = true,
            RtsEnable = true,
            ReadTimeout = 1000,
            WriteTimeout = 1000,
            BaudRate = 115200,
            StopBits = StopBits.One,
            Parity = Parity.None
        };
    }

    public void Dispose()
    {
        Close();
    }

    public void Close()
    {
        if (port.IsOpen)
        {
            port.Close();
        }
    }

    public void Open()
    {
        port.Open();
        port.DiscardInBuffer();
        port.DiscardOutBuffer();

        WriteCommand(CommandHello);

        using var response = new ByteBuffer(23);
        var read = ReadResponse(response.Buffer, 23);
        if ((read != 23) || !response.Buffer.AsSpan(0, 9).SequenceEqual("chs_5inch"u8))
        {
            throw new IOException($"Unknown response. response=[{Convert.ToHexString(response.Buffer.AsSpan(0, read))}]");
        }
    }

    private int ReadResponse(byte[] response, int length)
    {
        var offset = 0;
        while (offset < length)
        {
            var read = port.Read(response, offset, length - offset);
            if (read <= 0)
            {
                break;
            }

            offset += read;
        }

        return offset;
    }

    private void WriteCommand(ReadOnlySpan<byte> command, byte padValue = 0x00)
    {
        var commandLength = ((command.Length + 249) / 250) * 250;

        using var buffer = new ByteBuffer(commandLength);
        var span = buffer.GetSpan(commandLength);
        command.CopyTo(span);
        span[commandLength..].Fill(padValue);
        buffer.Advance(commandLength);

        port.Write(buffer.Buffer, 0, buffer.WrittenCount);
    }

    // TODO Clear

    public void SetBrightness(int level)
    {
        using var command = new ByteBuffer(CommandSetBrightness.Length + 1);
        var span = command.GetSpan();
        CommandSetBrightness.CopyTo(span);
        span[CommandSetBrightness.Length] = (byte)level;
        command.Advance(CommandSetBrightness.Length + 1);

        WriteCommand(command.WrittenSpan);
    }

    //        var cmd = new List<byte>
    //        {
    //            0x7b,
    //            0xef,
    //            0x69,
    //            0x00,
    //            0x00,
    //            0x00,
    //            0x01,
    //            0x00,
    //            0x00,
    //            0x00,
    //            (byte)level
    //        };
    //        WriteCommand(cmd.ToArray());
    //    }

    // TODO DisplayBitmap

    //    public void Reset() => WriteCommand(RESTART);

    //    public void DisplayBitmap(int x, int y, int width, int height, IScreenBuffer buffer)
    //    {
    //        var cBuffer = (TuringSmartScreenBufferC)buffer;
    //        if (cBuffer.IsEmpty())
    //        {
    //            ClearScreen();
    //        }
    //        else
    //        {
    //            var isFullScreen = height == HEIGHT && width == WIDTH;
    //            //var isRotated = width == HEIGHT && height == WIDTH;
    //            if (!isFullScreen)
    //            {
    //                DisplayPartialImage(x, y, width, height, cBuffer);
    //                WriteCommand(QUERY_STATUS);
    //                var resp = ReadResponse();
    //                if (resp?.Contains("needReSend:1", StringComparison.InvariantCulture) ?? false)
    //                {
    //                    DisplayPartialImage(x, y, width, height, cBuffer);
    //                    WriteCommand(QUERY_STATUS);
    //                }
    //            }
    //            else
    //            {
    //                if (x != 0 || y != 0 || width != WIDTH || height != HEIGHT)
    //                {
    //                    throw new InvalidOperationException("Invalid parameters for full screen image");
    //                }
    //                WriteCommand(START_DISPLAY_BITMAP, 0x2c);
    //                WriteCommand(DISPLAY_BITMAP);
    //                var blockSize = 249;
    //                var currentPosition = 0;
    //                while (currentPosition < cBuffer.Length)
    //                {
    //                    var block = cBuffer.ImgBuffer.Skip(currentPosition).Take(blockSize).ToArray();
    //                    WriteCommand(block);
    //                    currentPosition += blockSize;
    //                }
    //                WriteCommand(PRE_UPDATE_BITMAP);
    //                ReadResponse();
    //                WriteCommand(QUERY_STATUS);
    //                ReadResponse();
    //            }
    //        }
    //    }

    //private static byte[] ConvertAndPad(int number, int fixedLength)
    //{
    //    var byteArray = BitConverter.GetBytes(number);
    //    // Apply zero padding if necessary
    //    Array.Resize(ref byteArray, fixedLength);
    //    Array.Reverse(byteArray);
    //    return byteArray;
    //}

    //internal static (byte[] Data, byte[] UpdateSize) GeneratePartialUpdateFromBuffer(int height, int width, int x, int y, byte[] image, int channelCount = 4)
    //{
    //    var data = new List<byte>();

    //    for (var h = 0; h < height; h++)
    //    {
    //        data.AddRange(ConvertAndPad(((x + h) * 800) + y, 3));
    //        data.AddRange(ConvertAndPad(width, 2));
    //        for (var w = 0; w < width; w++)
    //        {
    //            var indexR = ((h * width) + w) * channelCount;
    //            data.Add(image[indexR]);
    //            var indexG = (((h * width) + w) * channelCount) + 1;
    //            data.Add(image[indexG]);
    //            var indexB = (((h * width) + w) * channelCount) + 2;
    //            data.Add(image[indexB]);
    //        }
    //    }
    //    var updSize = ConvertAndPad(data.Count + 2, 2);
    //    if (data.Count > 250)
    //    {
    //        var newMsg = new List<byte>();
    //        for (var i = 0; i <= data.Count; i++)
    //        {
    //            if (i % 249 == 0)
    //            {
    //                newMsg.AddRange(data.GetRange(i, Math.Min(249, data.Count - i)));
    //                newMsg.Add(0);
    //            }
    //        }
    //        // remove last padding 0
    //        newMsg.RemoveAt(newMsg.Count - 1);
    //        data = newMsg;
    //    }

    //    data.Add(0xef);
    //    data.Add(0x69);
    //    return (data.ToArray(), updSize);
    //}

    //private void DisplayPartialImage(int x, int y, int width, int height, TuringSmartScreenBufferC buffer)
    //{
    //    var (data, updSize) = GeneratePartialUpdateFromBuffer(height, width, x, y, buffer.ImgBuffer);
    //    var cmd = new List<byte>(UPDATE_BITMAP);
    //    cmd.AddRange(updSize);
    //    WriteCommand(cmd);
    //    WriteCommand(data);
    //}
}
