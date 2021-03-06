using System.Text;

namespace ECTest; 

public class BigEndianBinaryReader : BinaryReader {
	public BigEndianBinaryReader(Stream input) : base(input) {}
	public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding) {}
	public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) {}

	public override uint ReadUInt32() {
		byte[] data = this.ReadBytes(4);

		Array.Reverse(data);

		return BitConverter.ToUInt32(data);
	}
}