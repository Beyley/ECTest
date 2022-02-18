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
	private static VertexArrayObject   _VAO;
	private static UniformBufferObject _UBO;

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
			TexturePosition = new(0, 1)
		},
		new() {
			Position        = new(1, 0),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 1)
		},
		new() {
			Position        = new(1, 1),
			Color           = new(1, 1, 1, 1),
			TexturePosition = new(1, 0)
		},
		new() {
			Position        = new(0, 1),
			Color           = new(1, 1, 1, 1),
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

		_UBO = new(gl, _InstanceDataBufferSize);
		
		if (_VBO != null)
			_VBO.Dispose(gl);
			
		_VBO = new(gl);
		Program.CheckError(gl);
				
		_VBO.SetData<Vertex>(gl, _Vertices);
		Program.CheckError(gl);
			
		_VBO.Bind(gl);
			
		gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
		gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)sizeof(Vector2));
		gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(sizeof(Vector2) + sizeof(Color)));
		Program.CheckError(gl);
		
		gl.EnableVertexAttribArray(0);
		gl.EnableVertexAttribArray(1);
		gl.EnableVertexAttribArray(2);
		Program.CheckError(gl);
		
		_UBO.Bind(gl);
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

	private static Vector4 tempPositionSize;
	private static Vector4 color;

	private static Vector4 tempTextureData;

	public static void DrawTexture(GL gl, Texture tex, Vector2 postition, Vector2 size, Vector2 textureRectAdd, Vector2 textureRectMult, Color color, float rotation = 0f) {
		if (_Instances >= NUM_INSTANCES || _UsedTextures == Texture.MaxTextureUnits) {
			Flush(gl);
		}

		// _templateInstanceData.Position        = postition;
		// _templateInstanceData.Size            = size;
		// _templateInstanceData.Color           = color;
		// _templateInstanceData.TextureRectAdd  = textureRectAdd;
		// _templateInstanceData.TextureRectMult = textureRectMult;
		//
		// _InstanceData[_Instances] = _templateInstanceData;

		tempPositionSize.X = postition.X;
		tempPositionSize.Y = postition.Y;
		tempPositionSize.Z = size.X;
		tempPositionSize.W = size.Y;
		
		_InstancePositionSizes[_Instances] = tempPositionSize;

		_InstanceColors[_Instances] = color;

		tempTextureData.X = textureRectAdd.X;
		tempTextureData.Y = textureRectAdd.Y;
		tempTextureData.Z = textureRectMult.X;
		tempTextureData.W = textureRectMult.Y;

		_InstanceTexturePositionScales[_Instances] = tempTextureData;

		int instanceOver4 = (int)Math.Floor(_Instances / 4f);

		switch (_Instances % 4) {
			case 0: {
				_InstanceRotations[instanceOver4].X = rotation;
				break;
			}
			case 1: {
				_InstanceRotations[instanceOver4].Y = rotation;
				break;
			}
			case 2: {
				_InstanceRotations[instanceOver4].Z = rotation;
				break;
			}
			case 3: {
				_InstanceRotations[instanceOver4].W = rotation;
				break;
			}
		}

		int texId         = GetTextureId(tex);
		_InstanceTexIds[_Instances] = texId;

		_Instances++;
	}

	private static readonly Texture[] _BoundTextures = new Texture[Texture.MaxTextureUnits];
	private static          int       _UsedTextures  = 0;
	
	private static int GetTextureId(Texture tex) {
		if(_UsedTextures != 0)
			for (int i = 0; i < _UsedTextures; i++) {
				Texture tex2 = _BoundTextures[i];

				if (tex == tex2) return i;
			}

		_BoundTextures[_UsedTextures] = tex;
		_UsedTextures++;
		
		return _UsedTextures - 1;
	}

	public const short NUM_INSTANCES = 128;
	
	private static          uint           _Instances             = 0;
	public const            short          VECTORS_PER_INSTANCE   = 6;
	private static readonly Vector4[] _InstancePositionSizes = new Vector4[NUM_INSTANCES];
	//Colors are a vec4 anyway so this is just a normal array, one element per instance
	private static readonly Color[]   _InstanceColors                = new Color[NUM_INSTANCES];
	private static readonly Vector4[] _InstanceTexturePositionScales = new Vector4[NUM_INSTANCES];
	private static readonly Vector4[] _InstanceRotations             = new Vector4[NUM_INSTANCES / 4];
	//We pack 4 tex ids into a single Vector4D<int>
	private static readonly int[] _InstanceTexIds = new int[NUM_INSTANCES];

	private static readonly unsafe nuint _InstanceDataBufferSize = (nuint)(sizeof(Vector4) * _InstancePositionSizes.Length + sizeof(Color) * _InstanceColors.Length + sizeof(Vector4) * _InstanceTexturePositionScales.Length + sizeof(Vector4) * _InstanceRotations.Length + sizeof(int) * _InstanceTexIds.Length);
	
	private static unsafe void Flush(GL gl) {
		if (_Instances == 0) return;
		
		for (int i = 0; i < _UsedTextures; i++) {
			Texture tex = _BoundTextures[i];
			
			tex.Bind(gl, TextureUnit.Texture0 + i);
		}
		
		_VAO.Bind(gl);
		Program.CheckError(gl);
		
		// _UBO.Bind(gl);
		// Program.CheckError(gl);
		
		_UBO.SetDataSub<Vector4>(gl, _InstancePositionSizes, 0);
		Program.CheckError(gl);
		int offset = sizeof(Vector4) * _InstancePositionSizes.Length;
		_UBO.SetDataSub<Color>(gl, _InstanceColors, offset);
		Program.CheckError(gl);
		offset += sizeof(Color) * _InstanceColors.Length;
		_UBO.SetDataSub<Vector4>(gl, _InstanceTexturePositionScales, offset);
		Program.CheckError(gl);
		offset += sizeof(Vector4) * _InstanceTexturePositionScales.Length;
		_UBO.SetDataSub<Vector4>(gl, _InstanceRotations, offset);
		Program.CheckError(gl);
		offset += sizeof(Vector4) * _InstanceRotations.Length;
		_UBO.SetDataSub<int>(gl, _InstanceTexIds, offset);
		Program.CheckError(gl);

		fixed (void* ptr = _Indicies)
			gl.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, ptr, _Instances);

		LastDrawAmount += _Instances;
		LastInstanceAmount++;
		
		_Instances    = 0;
		_UsedTextures = 0;
	}

	// private static Texture _LastTex;
}
