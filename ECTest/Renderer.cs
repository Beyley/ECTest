using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Vulkan;
using Rectangle = System.Drawing.Rectangle;

namespace ECTest; 

public static class Renderer {
	private static VertexBufferObject  _VBO;
	private static VertexBufferObject  _InstanceVBO;
	private static VertexArrayObject   _VAO;
	// private static UniformBufferObject _UBO;

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
			// Color           = new(1, 1, 1, 1),
			TexturePosition = new(0, 1)
		},
		new() {
			Position        = new(1, 0),
			// Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 1)
		},
		new() {
			Position        = new(1, 1),
			// Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 0)
		},
		new() {
			Position        = new(0, 1),
			// Color           = new(1, 1, 1, 1),
			TexturePosition = new(0, 0)
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

	public static unsafe void Initialize(GL gl) {
		_VAO = new(gl);
		
		_VAO.Bind(gl);

		// _UBO = new(gl, _InstanceDataBufferSize);
		
		if (_VBO != null)
			_VBO.Dispose(gl);

		if (_InstanceVBO != null)
			_InstanceVBO.Dispose(gl);

		_VBO = new(gl);
		Program.CheckError(gl);
				
		_VBO.SetData<Vertex>(gl, _Vertices);
		Program.CheckError(gl);
			
		_VBO.Bind(gl);
			
		//Vertex Position
		gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
		//Texture position
		gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)sizeof(Vector2));
		
		gl.EnableVertexAttribArray(0);
		gl.EnableVertexAttribArray(1);
		// gl.EnableVertexAttribArray(2);
		Program.CheckError(gl);
		
		_InstanceVBO = new(gl);
		Program.CheckError(gl);
		
		_InstanceVBO.Bind(gl);

		int ptrPos = 0;
		//Position
		gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(2, 1);
		ptrPos += sizeof(Vector2);
		//Size
		gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(3, 1);
		ptrPos += sizeof(Vector2);
		//Color
		gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(4, 1);
		ptrPos += sizeof(Color);
		//Texture position
		gl.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(5, 1);
		ptrPos += sizeof(Vector2);
		//Texture size
		gl.VertexAttribPointer(6, 2, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(6, 1);
		ptrPos += sizeof(Vector2);
		//Rotation
		gl.VertexAttribPointer(7, 1, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(7, 1);
		ptrPos += sizeof(float);
		//Texture id
		gl.VertexAttribPointer(8, 1, VertexAttribPointerType.Int, false, (uint)sizeof(InstanceData), (void*)ptrPos);
		gl.VertexAttribDivisor(8, 1);
		ptrPos += sizeof(int);

		gl.EnableVertexAttribArray(2);
		gl.EnableVertexAttribArray(3);
		gl.EnableVertexAttribArray(4);
		gl.EnableVertexAttribArray(5);
		gl.EnableVertexAttribArray(6);
		gl.EnableVertexAttribArray(7);
		gl.EnableVertexAttribArray(8);
		
		VertexBufferObject.Unbind(gl);
		
		Program.CheckError(gl);

		// _UBO.Bind(gl);

		VertexArrayObject.Unbind(gl);
	}
	
	public static unsafe void Begin(GL gl, ShaderPair shader) {
		if (_Shader != shader) {
			shader.Use(gl);
			_NeedRebind = true;
		}
		else {
			_NeedRebind = false;
		}
		_Shader = shader;

		if(_NeedRebind)
			shader.SetUniform(gl, "ProjectionMatrix", Program.ProjectionMatrix);

		LastDrawAmount     = 0;
		LastInstanceAmount = 0;
	}

	public static void Clear(GL gl, Color color) {
		gl.Clear(ClearBufferMask.ColorBufferBit);
		gl.ClearColor(color.R, color.G, color.B, color.A);
	}

	public static void End(GL gl) {
		Flush(gl);
		
		// gl.DisableVertexAttribArray(0);
		// gl.DisableVertexAttribArray(1);
		// gl.DisableVertexAttribArray(2);
		// CheckError(gl);
	}

	// private static InstanceData _TempInstanceData = new();
	public static void DrawTexture(GL gl, Texture tex, Vector2 postition, Vector2 size, Vector2 textureRectAdd, Vector2 textureRectMult, Color color, float rotation = 0f) {
		if (_Instances >= NUM_INSTANCES || _UsedTextures == Texture.MaxTextureUnits) {
			Flush(gl);
		}

		_InstanceData[_Instances].Position         = postition;
		_InstanceData[_Instances].Size             = size;
		_InstanceData[_Instances].TextureId        = GetTextureId(tex);
		_InstanceData[_Instances].Color            = color;
		_InstanceData[_Instances].Rotation         = rotation;
		_InstanceData[_Instances].TexturePostition = textureRectAdd;
		_InstanceData[_Instances].TextureSize      = textureRectMult;

		_Instances++;
	}

	private static readonly Texture[] _BoundTextures = new Texture[Texture.MaxTextureUnits];
	private static          int       _UsedTextures  = 0;
	
	private static int GetTextureId(Texture tex) {
		if(_UsedTextures != 0)
			for (int i = 0; i < _UsedTextures; i++) {
				Texture tex2 = _BoundTextures[i];

				if (tex2 == null) break;
				if (tex == tex2) return i;
			}

		_BoundTextures[_UsedTextures] = tex;
		_UsedTextures++;
		
		return _UsedTextures - 1;
	}

	public const int NUM_INSTANCES = 1024;
	
	private static          uint           _Instances           = 0;
	public const            short          VECTORS_PER_INSTANCE = 6;
	private static readonly InstanceData[] _InstanceData           = new InstanceData[NUM_INSTANCES];
	// private static readonly Vector4[] _InstancePositionSizes = new Vector4[NUM_INSTANCES];
	//Colors are a vec4 anyway so this is just a normal array, one element per instance
	// private static readonly Color[]   _InstanceColors                = new Color[NUM_INSTANCES];
	// private static readonly Vector4[] _InstanceTexturePositionScales = new Vector4[NUM_INSTANCES];
	// private static readonly Vector4[] _InstanceRotations             = new Vector4[NUM_INSTANCES / 4];
	//We pack 4 tex ids into a single Vector4D<int>
	// private static readonly int[] _InstanceTexIds = new int[NUM_INSTANCES];

	// private static readonly unsafe nuint _InstanceDataBufferSize = (nuint)(sizeof(Vector4) * _InstancePositionSizes.Length + sizeof(Color) * _InstanceColors.Length + sizeof(Vector4) * _InstanceTexturePositionScales.Length + sizeof(Vector4) * _InstanceRotations.Length + sizeof(int) * _InstanceTexIds.Length);
	
	private static unsafe void Flush(GL gl) {
		if (_Instances == 0) return;
		
		for (int i = 0; i < _UsedTextures; i++) {
			Texture tex = _BoundTextures[i];
			
			tex.Bind(gl, TextureUnit.Texture0 + i);
		}
		
		_VAO.Bind(gl);
		Program.CheckError(gl);

		_InstanceVBO.SetData<InstanceData>(gl, _InstanceData);
		
		gl.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 6, _Instances);

		LastDrawAmount += _Instances;
		LastInstanceAmount++;
		
		_Instances    = 0;
		_UsedTextures = 0;
	}

	// private static Texture _LastTex;
}
