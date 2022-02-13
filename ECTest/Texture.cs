using Furball.Vixie.Graphics;
using Silk.NET.OpenGLES;

namespace ECTest;

public class Texture {
	public uint TextureID;

	public static unsafe Texture LoadQoi(GL gl, string filename) {
		Texture texture = new() {
			TextureID = gl.GenTexture()
		};

		(QoiLoader.Pixel[]? pixels, QoiLoader.QoiHeader? header) = QoiLoader.Load(File.ReadAllBytes(filename));

		texture.Bind(gl);
		
		gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
		gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
		gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
		gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
		
		fixed (void* ptr = pixels)
			gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, header.Width, header.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

		Unbind(gl);
		
		return texture;
	}

	public void Bind(GL gl) {
		gl.BindTexture(TextureTarget.Texture2D, this.TextureID);
	}

	public static void Unbind(GL gl) {
		//This unbinds the texture
		gl.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void Dispose(GL gl) {
		gl.DeleteTexture(this.TextureID);
	}
}
