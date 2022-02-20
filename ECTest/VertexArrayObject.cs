using Silk.NET.OpenGLES;

namespace ECTest; 

public class VertexArrayObject {
	public uint VaoId;

	public VertexArrayObject(GL gl) {
		this.VaoId = gl.GenVertexArray();
		Program.CheckError(gl);
	}

	public void Bind(GL gl) {
		gl.BindVertexArray(this.VaoId);
		Program.CheckError(gl);
	}

	public static void Unbind(GL gl) {
		gl.BindVertexArray(0);
	}

	public void Dispose(GL gl) {
		gl.DeleteVertexArray(this.VaoId);
		Program.CheckError(gl);
	}
}
