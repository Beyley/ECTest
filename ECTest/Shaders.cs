using System.Text;
using Silk.NET.OpenGLES;

namespace ECTest; 

public static class Shaders {
	public static string Vertex;
	public static string Fragment;

	public static void GenerateShaders(GL gl) {
		Vertex   = Program.LoadStringFromEmbedded("InstancedVertex.glsl");
		Fragment = Program.LoadStringFromEmbedded("InstancedFragment.glsl");
		
		Texture.MaxTextureUnits = gl.GetInteger(GLEnum.MaxTextureImageUnits);

		StringBuilder uniforms = new();
		StringBuilder select   = new();
		for (int i = 0; i < Texture.MaxTextureUnits; i++) {
			uniforms.Append($"uniform sampler2D tex_{i};\n");
			select.Append("    ");
			if (i != 0)
				select.Append("else ");
			select.Append($"if(fs_in_texid == {i})\n");
			select.Append($"        tex_to_use = tex_{i};\n");
		}

		Fragment = Fragment.Replace("${UNIFORMS}", uniforms.ToString());
		Fragment = Fragment.Replace("${SELECT}", select.ToString());
	}
}
