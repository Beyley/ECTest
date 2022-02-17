using Silk.NET.OpenGLES;

namespace ECTest; 

public class UniformBufferObject {
	public          uint  BufferId;
	public readonly nuint Size;

	public UniformBufferObject(GL gl, nuint size) {
		this.BufferId = gl.GenBuffer();
		gl.BindBuffer(BufferTargetARB.UniformBuffer, this.BufferId);
		gl.BufferData(BufferTargetARB.UniformBuffer, size, 0, BufferUsageARB.DynamicDraw);
		gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
		Program.CheckError(gl);

		this.Size = size;
	}

	public void Bind(GL gl) {
		gl.BindBufferRange(BufferTargetARB.UniformBuffer, 0, this.BufferId, 0, this.Size);
		Program.CheckError(gl); 
		// Program.CheckError(gl);
	}

	public unsafe void SetDataSub<T>(GL gl, ReadOnlySpan<T> data, nint offset) where T : unmanaged {
		gl.BufferSubData(BufferTargetARB.UniformBuffer, offset, data);
		// Program.CheckError(gl);
		// gl.BufferSubData(BufferTargetARB.UniformBuffer, )
	}
}
