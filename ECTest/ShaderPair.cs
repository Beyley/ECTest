using System.Numerics;
using Silk.NET.OpenGLES;

namespace ECTest; 

public class ShaderPair {
	public uint Vertex;
	public uint Fragment;

	public uint Program;
	
	public ShaderPair(GL gl, string vertex, string fragment) {
		this.Vertex   = CompileShader(gl, ShaderType.VertexShader, vertex);
		ECTest.Program.CheckError(gl);
		this.Fragment = CompileShader(gl, ShaderType.FragmentShader, fragment);
		ECTest.Program.CheckError(gl);
		
		this.Program = LinkShader(gl, this.Vertex, this.Fragment);
		ECTest.Program.CheckError(gl);
	}

	private static uint LinkShader(GL gl, uint vertex, uint fragment) {
		uint program = gl.CreateProgram();

		if (program == 0)
			throw new Exception("Unable to create shader program!");
		
		//Attatch the shaders
		gl.AttachShader(program, vertex);
		gl.AttachShader(program, fragment);
		
		gl.LinkProgram(program);

		Console.WriteLine($"SHADER LINK LOG {gl.GetProgramInfoLog(program)}");
		
		if (program == 0)
			throw new Exception("Unable to link shaders!");

		return program;
	}
	
	private Dictionary<string, int> CachedUniformLocations = new();

	public unsafe void SetUniform(GL gl, string name, Matrix4x4 matrix) {
		if (!this.CachedUniformLocations.TryGetValue(name, out int location)) {
			location = gl.GetUniformLocation(this.Program, name);
			
			this.CachedUniformLocations[name] = location;
		}
		
		gl.UniformMatrix4(location, 1, false, (float*) &matrix);
		ECTest.Program.CheckError(gl);
	}
	
	// public unsafe void SetUniform(GL gl, string name, ReadOnlySpan<InstanceData> vec2arr) {
	// 	if (!this.CachedUniformLocations.TryGetValue(name, out int location)) {
	// 		location = gl.GetUniformLocation(this.Program, name);
	// 		
	// 		this.CachedUniformLocations[name] = location;
	// 	}
	//
	// 	fixed(InstanceData* ptr = vec2arr)
	// 		gl.Uniform2(location, (uint)(vec2arr.Length * Renderer.VECTORS_PER_INSTANCE), (float*)ptr);
	// }
	
	public void SetUniform(GL gl, string name, ReadOnlySpan<int> intarr) {
		if (!this.CachedUniformLocations.TryGetValue(name, out int location)) {
			location = gl.GetUniformLocation(this.Program, name);
			
			this.CachedUniformLocations[name] = location;
		}

		gl.Uniform1(location, (uint)intarr.Length, intarr);
	}

	public void SetUniformBlockBinding(GL gl, string name, uint binding) {
		gl.UniformBlockBinding(this.Program, gl.GetUniformBlockIndex(this.Program, name), binding);
	}

	public unsafe void BindUniformToTexUnit(GL gl, string name, int unit) {
		int location = gl.GetUniformLocation(this.Program, name);

		gl.Uniform1(location, unit);
	}
	
	public void Use(GL gl) {
		gl.UseProgram(this.Program);
		ECTest.Program.CheckError(gl);
	}

	private static uint CompileShader(GL gl, ShaderType type, string source) {
		uint shader = gl.CreateShader(type);
		gl.ShaderSource(shader, source);
		gl.CompileShader(shader);
	
		Console.WriteLine($"SHADER COMPILE LOG {type} {gl.GetShaderInfoLog(shader)}");

		if (shader != 0)
			return shader;
		
		gl.DeleteShader(shader);
		throw new Exception($"Shader compilation failed!: {gl.GetShaderInfoLog(shader)}");
	}
	
}
