using System.Numerics;
using System.Runtime.InteropServices;

namespace ECTest; 

[StructLayout(LayoutKind.Sequential)]
public struct InstanceData {
	public Vector2 Position;
	public Vector2 Size;
	public Color   Color;
	public Vector2 TexturePostition;
	public Vector2 TextureSize;
	public float   Rotation;
	public int     TextureId;
}
