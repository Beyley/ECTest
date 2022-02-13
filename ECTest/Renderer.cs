using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Rectangle = System.Drawing.Rectangle;

namespace ECTest; 

public static class Renderer {
	private static VertexBuffer _Buffer;
	public static uint LastDrawAmount {
		get;
		private set;
	}
	public static uint LastInstanceAmount {
    	get;
    	private set;
    }

	private static Vertex[] _Vertices = {
		new() {
			Position        = new(0, 0),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(0, 0)
		},
		new() {
			Position        = new(1, 0),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 0)
		},
		new() {
			Position        = new(1, 1),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 1)
		},
		new() {
			Position        = new(0, 1),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(0, 1)
		}
	};
	private static short[] _Indicies = {
		//Tri 1
		0, 1, 2, 
		//Tri 2
		2, 3, 0
	};

	private static ShaderPair _Shader     = null;
	private static bool       _NeedRebind = false;
	
	public static unsafe void Begin(GL gl, ShaderPair shader) {
		if (_Shader != shader) {
			shader.Use(gl);
			_NeedRebind = true;
		}
		else {
			_NeedRebind = false;
		}
		_Shader = shader;

		if (_NeedRebind) {
			if (_Buffer != null)
				_Buffer.Dispose(gl);
			
			_Buffer = new(gl);
			Program.CheckError(gl);
				
			_Buffer.SetData<Vertex>(gl, _Vertices);
			Program.CheckError(gl);
			
			_Buffer.Bind(gl);
			
			gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
			gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)sizeof(Vector2));
			gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(sizeof(Vector2) + sizeof(Color)));
			Program.CheckError(gl);
		}

		if(_NeedRebind)
			shader.SetUniform(gl, "ProjectionMatrix", Program.ProjectionMatrix);
		
		gl.EnableVertexAttribArray(0);
		gl.EnableVertexAttribArray(1);
		gl.EnableVertexAttribArray(2);
		Program.CheckError(gl);

		LastDrawAmount     = 0;
		LastInstanceAmount = 0;
	}

	public static void Clear(GL gl, Color color) {
		gl.Clear(ClearBufferMask.ColorBufferBit);
		gl.ClearColor(color.R, color.G, color.B, color.A);
	}

	public static void End(GL gl) {
		Flush(gl);
		
		gl.DisableVertexAttribArray(0);
		gl.DisableVertexAttribArray(1);
		gl.DisableVertexAttribArray(2);
		// CheckError(gl);
	}


	private static InstanceData _templateInstanceData;
	public static void DrawTexture(GL gl, Texture tex, Vector2 postition, Vector2 size, Vector2 textureRectMult, Color color) {
		if (_LastTex != null && tex != _LastTex || _Instances >= _InstanceData.Length) {
			Flush(gl);
		}
		_LastTex = tex;

		_templateInstanceData.Position        = postition;
		_templateInstanceData.Size            = size;
		_templateInstanceData.Color           = color;
		_templateInstanceData.TextureRectMult = textureRectMult;
		
		_InstanceData[_Instances] = _templateInstanceData;
		
		_Instances++;
	}

	private static          uint           _Instances           = 0;
	public const            short          VECTORS_PER_INSTANCE = 5;
	private static readonly InstanceData[] _InstanceData        = new InstanceData[128];
	
	private static unsafe void Flush(GL gl) {
		if (_Instances == 0) return;
		
		_LastTex.Bind(gl);

		_Shader.SetUniform(gl, "InstanceData", _InstanceData);
		
		fixed (void* ptr = _Indicies)
			gl.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, ptr, _Instances);

		LastDrawAmount += _Instances;
		LastInstanceAmount++;
		
		_Instances = 0;
	}

	private static Texture _LastTex;
}
