using System.Security.Cryptography.X509Certificates;
using Silk.NET.OpenGLES;

namespace ECTest; 

public class VertexBufferObject {
	public uint BufferID;

	public VertexBufferObject(GL gl) {
		this.BufferID = gl.GenBuffer();
	}

	public void Bind(GL gl) {
		gl.BindBuffer(GLEnum.ArrayBuffer, this.BufferID);
	}

	public static void Unbind(GL gl) {
		gl.BindBuffer(GLEnum.ArrayBuffer, 0);
	}

	public void SetData<T>(GL gl, ReadOnlySpan<T> data) where T : unmanaged {
		this.Bind(gl);
		gl.BufferData(GLEnum.ArrayBuffer, data, GLEnum.StaticDraw);
	}
	
	public void Dispose(GL gl) {
		gl.DeleteBuffer(this.BufferID);
	}
}
