using ECTest;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;

namespace ECTest {
	public class Program {
		public static void CheckError(GL gl) {
			GLEnum error = gl.GetError();

			if (error != GLEnum.NoError) {
				throw new Exception($"GLES Error! {error}");
			}
		}

		public static void Main(string[] args) {
			GL gl;

			ShaderPair mainShaderPair;

			IWindow window = Window.Create(WindowOptions.Default with {
				API = new(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.Default, new(3, 0)),
				WindowBorder = WindowBorder.Fixed,
				Size = new(1024, 768)
			});

			window.Load += delegate {
				gl = window.CreateOpenGLES();

				mainShaderPair = new ShaderPair(gl, Shaders.Vertex, Shaders.Fragment);
				Console.WriteLine($"Shaders compiled! vtx:{mainShaderPair.Vertex} frg:{mainShaderPair.Fragment} prg: {mainShaderPair.Program}");
			};

			window.Update += delegate(double time) {
	
			};

			window.Render += delegate(double time) {
	
			};

			Console.WriteLine("Starting window!");
			window.Run();
			Console.WriteLine("Window ended!");
		}
	}
}