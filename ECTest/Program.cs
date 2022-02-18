using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using ECTest;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;

namespace ECTest {
	public class Program {
		public static string LoadStringFromEmbedded(string name) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			
			name = $"ECTest.{name}";
			
			using (Stream stream = assembly.GetManifestResourceStream(name))
				using (StreamReader reader = new(stream))
					return reader.ReadToEnd();
		}
		
		private static Texture   _Tex1;
		private static Texture   _Tex2;
		public static  Matrix4x4 ProjectionMatrix;

		[Conditional("DEBUG")]
		public static void CheckError(GL gl) {
			GLEnum error = gl.GetError();

			if (error != GLEnum.NoError) {
				throw new Exception($"GLES Error! {error}");
			}
		}

#if DEBUG
		public const bool IS_DEBUG = true;
#else
		public const bool IS_DEBUG = false;
#endif
		
		public static unsafe void Main(string[] args) {
			GL gl = null;

			ShaderPair mainShaderPair = null;

			IWindow window = Window.Create(WindowOptions.Default with {
				API = new(ContextAPI.OpenGLES, ContextProfile.Core, IS_DEBUG ? ContextFlags.Debug : ContextFlags.Default, new(3, 0)),
				WindowBorder = WindowBorder.Fixed,
				Size = new(1024, 768),
				VSync = false,
				Samples = 4
				// FramesPerSecond = 10000
			});

			window.Load += delegate {
				gl = window.CreateOpenGLES();
				
				gl.Enable(EnableCap.Blend);
				gl.BlendFunc(GLEnum.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				gl.Viewport(new Vector2D<int>(0, 0), window.Size);
				CheckError(gl);

				Shaders.GenerateShaders(gl);
				mainShaderPair = new ShaderPair(gl, Shaders.Vertex, Shaders.Fragment);
				CheckError(gl);
				Console.WriteLine($"Shaders compiled! vtx:{mainShaderPair.Vertex} frg:{mainShaderPair.Fragment} prg: {mainShaderPair.Program}");

				mainShaderPair.Use(gl);

				for (int i = 0; i < Texture.MaxTextureUnits; i++) {
					mainShaderPair.BindUniformToTexUnit(gl, $"tex_{i}", i);
				}
				
				mainShaderPair.SetUniformBlockBinding(gl, "InstanceData", 0);

				_Tex1 = Texture.LoadQoi(gl, "images/test.qoi");
				_Tex2 = Texture.LoadQoi(gl, "images/test2.qoi");
				CheckError(gl);
				
				ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, window.Size.X, 0, window.Size.Y, 1f, 0f);
				
				if(IS_DEBUG) {
					gl.Enable(GLEnum.DebugOutputSynchronous);
					gl.DebugMessageCallback(Callback, null);
				}
				
				gl.Enable(EnableCap.Multisample);
				
				Renderer.Initialize(gl);
			};

			window.Update += delegate(double time) {
	
			};

			// Vector2[] positions        = { new(-0.5f, 0.5f), new(0.5f, 0.5f), new(0.5f, -0.5f), new(-0.5f, -0.5f) };
			// Vector2[] texturePositions = { new(0, 1f), new(1, 1), new(1, 0), new(0, 0) };
			// Vector4   color     = new(255, 255, 255, 255);

			double a = 0;
			window.Render += delegate(double time) {
				if (a > 1) {
					Console.WriteLine(1d / time);
					Console.WriteLine(Renderer.LastDrawAmount);
					Console.WriteLine(Renderer.LastInstanceAmount);
					a = 0;
				}

				a += time;
				Renderer.Begin(gl, mainShaderPair);
				CheckError(gl);

				Renderer.Clear(gl, new(0, 0, 0, 0));
				CheckError(gl);
				
				// for (int x = 0; x <= 1000; x += 10) {
				// 	for (int y = 0; y <= 740; y += 10) {
				// 		Renderer.DrawTexture(
				// 			gl, 
				// 			x % 40 != 0 ? _Tex2 : _Tex1, 
				// 			new(x, y), 
				// 			new(20f),
				// 			new(0f),
				// 			new(1f),
				// 			x % 40 != 0 ? new(1, 0, 1, 1) : new(0, 1, 1, 1),
				// 			x % 40 != 0 ? 1f : 0.5f
				// 		);
				// 	}
				// }

				for (int x = 0; x < 2000000; x++) {
					Renderer.DrawTexture(
						gl, 
						x % 40 != 0 ? _Tex2 : _Tex1, 
						new(x % 1000, 10), 
						new(20f),
						new(0f),
						new(1f),
						x % 40 != 0 ? new(1, 0, 1, 1) : new(0, 1, 1, 1),
						x % 40 != 0 ? 1f : 0.5f
					);
				}

				CheckError(gl);

				Renderer.End(gl);
				CheckError(gl);
			};
			
			window.Closing += delegate {
				_Tex1.Dispose(gl);
			};

			Console.WriteLine("Starting window!");
			window.Run();
			Console.WriteLine("Window ended!");
		}
		
		private static void Callback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userparam) {
			Console.WriteLine(SilkMarshal.PtrToString(message));
		}
	}
}