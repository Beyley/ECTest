using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using ECTest;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;

namespace ECTest {
	public class Program {
		private static Texture      _Tex;
		public static Matrix4x4    ProjectionMatrix;

		[Conditional("DEBUG")]
		public static void CheckError(GL gl) {
			GLEnum error = gl.GetError();

			if (error != GLEnum.NoError) {
				throw new Exception($"GLES Error! {error}");
			}
		}

		public static unsafe void Main(string[] args) {
			GL gl = null;

			ShaderPair mainShaderPair = null;

			IWindow window = Window.Create(WindowOptions.Default with {
				API = new(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.Default, new(3, 0)),
				WindowBorder = WindowBorder.Fixed,
				Size = new(1024, 768),
				VSync = false,
				// FramesPerSecond = 10000
			});
			
			

			window.Load += delegate {
				gl = window.CreateOpenGLES();
				
				gl.Enable(EnableCap.Blend);
				gl.BlendFunc(GLEnum.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				gl.Viewport(new Vector2D<int>(0, 0), window.Size);
				CheckError(gl);

				mainShaderPair = new ShaderPair(gl, Shaders.Vertex, Shaders.Fragment);
				CheckError(gl);
				Console.WriteLine($"Shaders compiled! vtx:{mainShaderPair.Vertex} frg:{mainShaderPair.Fragment} prg: {mainShaderPair.Program}");

				_Tex = Texture.LoadQoi(gl, "images/test.qoi");
				CheckError(gl);
				
				ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, window.Size.X, 0, window.Size.Y, 1f, 0f);
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
				
				Renderer.Clear(gl, new(0, 0, 0, 0));
				CheckError(gl);
				
				for (int x = 0; x <= 1000; x += 40) {
					for (int y = 0; y <= 740; y += 40) {
						Renderer.DrawTexture(
							gl, 
							_Tex, 
							new(x, y), 
							new(40f),
							new(1f),
							x % 80 != 0 ? new(1, 0, 1, 1) : new(0, 1, 1, 1)
						);
					}
				}

				CheckError(gl);

				Renderer.End(gl);
			};
			
			window.Closing += delegate {
				_Tex.Dispose(gl);
			};

			Console.WriteLine("Starting window!");
			window.Run();
			Console.WriteLine("Window ended!");
		}
	}
}