using System.Numerics;
using System.Runtime.InteropServices;

namespace ECTest; 

[StructLayout(LayoutKind.Sequential)]
public struct Vertex {
	public Vector2 Position;
	public Color   Color;
	public Vector2 TexturePosition;
}
