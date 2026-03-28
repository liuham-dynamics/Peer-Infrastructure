using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PeerTalk.Dns
{
    /// <summary>
    ///   Methods to write DNS wire formatted data items.
    /// </summary>
    public class DnsWireWriter
    {
        private const int _memberMaxPointer = 0x3FFF;
        private const ulong _memberUint48MaxValue = 0XFFFFFFFFFFFFul;
        private static readonly DateTime _memberUnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private Stream _memberStream = Stream.Null;
        private Dictionary<string, int> _memberPointers = [];
        private Stack<Stream> _memberScopes = new();

        /// <summary>
        ///   The writer relative position within the _memberStream.
        /// </summary>
        public int Position;

        /// <summary>
        ///   Creates a new instance of the <see cref="DnsWireWriter"/> on the
        ///   specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The destination for data items.
        /// </param>
        public DnsWireWriter(Stream stream)
        {
            this._memberStream = stream;
        }

        /// <summary>
        ///   Determines if canonical records are produced.
        /// </summary>
        /// <value>
        ///   <b>true</b> to produce canonical records; otherwise <b>false</b>.
        ///   Defaults to false.
        /// </value>
        /// <remarks>
        ///   When enabled, the following rules are applied
        ///   <list type="bullet">
        ///   <item><description>Domain _memberNames are uncompressed</description></item>
        ///   <item><description>Domain _memberNames are converted to US-ASCII lowercase</description></item>
        ///   </list>
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc4034#section-6.2"/>
        public bool CanonicalForm { get; set; }

        /// <summary>
        ///   Start a length prefixed _memberStream.
        /// </summary>
        /// <remarks>
        ///   A memory _memberStream is created for writing.  When it is popped,
        ///   the memory _memberStream's position is writen as an UInt16 and its
        ///   contents are copied to the current _memberStream.
        /// </remarks>
        public void PushLengthPrefixedScope()
        {
            _memberScopes.Push(_memberStream);
            _memberStream = new MemoryStream();
            Position += 2; // count the length prefix
        }

        /// <summary>
        ///   Start a length prefixed _memberStream.
        /// </summary>
        /// <remarks>
        ///   A memory _memberStream is created for writing.  When it is popped,
        ///   the memory _memberStream's position is writen as an UInt16 and its
        ///   contents are copied to the current _memberStream.
        /// </remarks>
        public ushort PopLengthPrefixedScope()
        {
            var lp = _memberStream;
            var length = (ushort)lp.Position;
            _memberStream = _memberScopes.Pop();
            WriteUInt16(length);
            Position -= 2;
            lp.Position = 0;
            lp.CopyTo(_memberStream);

            return length;
        }

        /// <summary>
        ///   Write a byte.
        /// </summary>
        public void WriteByte(byte value)
        {
            _memberStream.WriteByte(value);
            ++Position;
        }

        /// <summary>
        ///   Write a sequence of bytes.
        /// </summary>
        /// <param name="bytes">
        ///   A sequence of bytes to write.
        /// </param>
        public void WriteBytes(byte[] bytes)
        {
            if (bytes != null)
            {
                _memberStream.Write(bytes, 0, bytes.Length);
                Position += bytes.Length;
            }
        }

        /// <summary>
        ///   Write a sequence of bytes prefixed with the length as a byte.
        /// </summary>
        /// <param name="bytes">
        ///   A sequence of bytes to write.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   When the length is greater than <see cref="byte.MaxValue"/>.
        /// </exception>
        public void WriteByteLengthPrefixedBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            if (bytes.Length > byte.MaxValue)
            {
                throw new ArgumentException($"Length can not exceed {byte.MaxValue}.", nameof(bytes));
            }

            WriteByte((byte)bytes.Length);
            WriteBytes(bytes!);
        }

        /// <summary>
        ///   Write a sequence of bytes prefixed with the length as a unint16.
        /// </summary>
        /// <param name="bytes">
        ///   A sequence of bytes to write.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   When the length is greater than <see cref="ushort.MaxValue"/>.
        /// </exception>
        public void WriteUint16LengthPrefixedBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            if (bytes.Length > ushort.MaxValue)
            {
                throw new ArgumentException($"Bytes length can not exceed {ushort.MaxValue}.");
            }

            WriteUInt16((ushort)bytes.Length);
            WriteBytes(bytes);
        }

        /// <summary>
        ///   Write an unsigned short.
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            _memberStream.WriteByte((byte)(value >> 8));
            _memberStream.WriteByte((byte)value);
            Position += 2;
        }

        /// <summary>
        ///   Write an unsigned int.
        /// </summary>
        public void WriteUInt32(uint value)
        {
            _memberStream.WriteByte((byte)(value >> 24));
            _memberStream.WriteByte((byte)(value >> 16));
            _memberStream.WriteByte((byte)(value >> 8));
            _memberStream.WriteByte((byte)value);
            Position += 4;
        }

        /// <summary>
        ///   Write an unsigned long in 48 bits.
        /// </summary>
        public void WriteUInt48(ulong value)
        {
            if (value > _memberUint48MaxValue)
            {
                throw new ArgumentException("Value is greater than 48 bits.");
            }

            _memberStream.WriteByte((byte)(value >> 40));
            _memberStream.WriteByte((byte)(value >> 32));
            _memberStream.WriteByte((byte)(value >> 24));
            _memberStream.WriteByte((byte)(value >> 16));
            _memberStream.WriteByte((byte)(value >> 8));
            _memberStream.WriteByte((byte)value);
            Position += 6;
        }

        /// <summary>
        ///   Write a domain name.
        /// </summary>
        /// <param name="name">
        ///   The name to write.
        /// </param>
        /// <param name="uncompressed">
        ///   Determines if the <paramref name="name"/> must be uncompressed.  The
        ///   defaultl is false (allow compression).
        ///   <see cref="CanonicalForm"/> overrides this value.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   When a label length is greater than 63 octets.
        /// </exception>
        /// <remarks>
        ///   A domain name is represented as a sequence of _memberLabels, where
        ///   each label consists of a length octet followed by that
        ///   number of octets.The domain name terminates with the
        ///   zero length octet for the null label of the root. Note
        ///   that this field may be an odd number of octets; no
        ///   padding is used.
        /// </remarks>
        public void WriteDomainName(string name, bool uncompressed = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                _memberStream.WriteByte(0); // terminating byte
                ++Position;
                return;
            }
            WriteDomainName(new DomainName(name), uncompressed);
        }

        /// <summary>
        ///   Write a domain name.
        /// </summary>
        /// <param name="name">
        ///   The name to write.
        /// </param>
        /// <param name="uncompressed">
        ///   Determines if the <paramref name="name"/> must be uncompressed.  The
        ///   defaultl is false (allow compression).
        ///   <see cref="CanonicalForm"/> overrides this value.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   When a label length is greater than 63 octets.
        /// </exception>
        /// <remarks>
        ///   A domain name is represented as a sequence of _memberLabels, where
        ///   each label consists of a length octet followed by that
        ///   number of octets.The domain name terminates with the
        ///   zero length octet for the null label of the root. Note
        ///   that this field may be an odd number of octets; no
        ///   padding is used.
        /// </remarks>
        public void WriteDomainName(DomainName name, bool uncompressed = false)
        {
            if (name is null)
            {
                _memberStream.WriteByte(0); // terminating byte
                ++Position;
                return;
            }

            if (CanonicalForm)
            {
                uncompressed = true;
                name = name.ToCanonical();
            }

            var labels = name.Labels.ToArray();
            var n = labels.Length;
            for (var i = 0; i < n; ++i)
            {
                var label = labels[i];
                if (label.Length > 63)
                    throw new ArgumentException($"Label '{label}' cannot exceed 63 octets.");

                // Check for qualified name already used.
                var qn = string.Join(".", labels, i, labels.Length - i);
                if (!uncompressed && _memberPointers.TryGetValue(qn, out int pointer))
                {
                    WriteUInt16((ushort)(0xC000 | pointer));
                    return;
                }
                if (Position <= _memberMaxPointer)
                {
                    _memberPointers[qn] = Position;
                }

                // Add the label
                var bytes = Encoding.UTF8.GetBytes(label);
                WriteByteLengthPrefixedBytes(bytes);
            }

            _memberStream.WriteByte(0); // terminating byte
            ++Position;
        }

        /// <summary>
        ///   Write a string.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///   When the length is greater than <see cref="byte.MaxValue"/> or
        ///   the string is not ASCII.
        /// </exception>
        /// <remarks>
        ///   Strings are encoded with a length prefixed byte.  All strings must be
        ///   ASCII.
        /// </remarks>
        public void WriteString(string value)
        {
            if (value.Any(c => c > 0x7F))
            {
                throw new ArgumentException("Only ASCII characters are allowed.");
            }

            var bytes = Encoding.ASCII.GetBytes(value);
            WriteByteLengthPrefixedBytes(bytes);
        }

        /// <summary>
        ///   Write a time span with 16-bits.
        /// </summary>
        /// <param name="value">
        ///   The number of non-negative seconds.
        /// </param>
        /// <remarks>
        ///   The interval is represented as the number of seconds in two bytes.
        /// </remarks>
        public void WriteTimeSpan16(TimeSpan value)
        {
            WriteUInt16((ushort)value.TotalSeconds);
        }

        /// <summary>
        ///   Write a time span with 32-bits.
        /// </summary>
        /// <param name="value">
        ///   The number of non-negative seconds.
        /// </param>
        /// <remarks>
        ///   The interval is represented as the number of seconds in four bytes.
        /// </remarks>
        public void WriteTimeSpan32(TimeSpan value)
        {
            WriteUInt32((uint)value.TotalSeconds);
        }

        /// <summary>
        ///   Write a date/time.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="DateTime"/> in UTC to write.
        /// </param>
        /// <exception cref="OverflowException">
        ///   <paramref name="value"/> seconds cannot be represented
        ///   in 32 bits.
        /// </exception>
        /// <remarks>
        ///   Write the <paramref name="value"/> as the number seconds
        ///   since the Unix epoch.  The seconds is represented as 32-bit
        ///   unsigned int
        /// </remarks>
        public void WriteDateTime32(DateTime value)
        {
            var seconds = (value.ToUniversalTime() - _memberUnixEpoch).TotalSeconds;
            WriteUInt32(Convert.ToUInt32(seconds));
        }

        /// <summary>
        ///   Write a date/time.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="DateTime"/> in UTC to write.
        /// </param>
        /// <exception cref="OverflowException">
        ///   <paramref name="value"/> seconds cannot be represented
        ///   in 48 bits.
        /// </exception>
        /// <remarks>
        ///   Write the <paramref name="value"/> as the number seconds
        ///   since the Unix epoch.  The seconds is represented as 48-bit
        ///   unsigned int
        /// </remarks>
        public void WriteDateTime48(DateTime value)
        {
            var seconds = (value.ToUniversalTime() - _memberUnixEpoch).TotalSeconds;
            WriteUInt48(Convert.ToUInt64(seconds));
        }

        /// <summary>
        ///   Write an IP address.
        /// </summary>
        /// <param name="value"></param>
        public void WriteIPAddress(IPAddress value)
        {
            WriteBytes(value.GetAddressBytes());
        }

        /// <summary>
        ///   Write the bitmap(s) for the values.
        /// </summary>
        /// <param name="values">
        ///   The sequence of values to encode into a bitmap.
        /// </param>
        public void WriteBitmap(IEnumerable<ushort> values)
        {
            var windows = values
                // Convert values into Window and Mask
                .Select(v =>
                {
                    var w = new { Window = v / 256, Mask = new BitArray(256) };
                    w.Mask[v & 0xff] = true;
                    return w;
                })
                // Group by Window and merge the Masks
                .GroupBy(w => w.Window)
                .Select(g => new
                {
                    Window = g.Key,
                    Mask = g.Select(w => w.Mask).Aggregate((a, b) => a.Or(b))
                })
                .OrderBy(w => w.Window)
                .ToArray();

            foreach (var window in windows)
            {
                // BitArray to byte array and remove trailing zeros.
                var mask = ToBytes(window.Mask, true).ToList();
                for (int i = mask.Count - 1; i > 0; --i)
                {
                    if (mask[i] != 0)
                        break;
                    mask.RemoveAt(i);
                }

                _memberStream.WriteByte((byte)window.Window);
                _memberStream.WriteByte((byte)mask.Count);
                Position += 2;
                WriteBytes(mask.ToArray());
            }
        }

        private static IEnumerable<byte> ToBytes(BitArray bits, bool MSB = false)
        {
            int bitCount = 7;
            int outByte = 0;

            foreach (bool bitValue in bits)
            {
                if (bitValue)
                    outByte |= MSB ? 1 << bitCount : 1 << (7 - bitCount);
                if (bitCount == 0)
                {
                    yield return (byte)outByte;
                    bitCount = 8;
                    outByte = 0;
                }
                bitCount--;
            }

            // Last partially decoded byte
            if (bitCount < 7)
            {
                yield return (byte)outByte;
            }
        }
    }
}
