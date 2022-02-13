namespace ECTest; 

public static class Shaders {
	public static string Vertex = @"#version 300 es

layout (location = 0) in vec2 VertexPosition;
layout (location = 1) in vec4 VertexColor;
layout (location = 2) in vec2 VertexTextureCoordinate;

// uniform float RadianAngle;
out vec4 fs_in_col;
out vec2 fs_in_tex;
flat out int  fs_in_texid;

uniform mat4 ProjectionMatrix;
uniform vec2 InstanceData[128 * 6];
uniform int InstanceTexIds[128];

void main() {
	//mat2 rotation = mat2(cos(RadianAngle),sin(RadianAngle),
    //                -sin(RadianAngle),cos(RadianAngle));

	// gl_Position = mat4(rotation)*VertexPosition;

	vec2 i_position = InstanceData[gl_InstanceID * 6];
	vec2 i_size = InstanceData[(gl_InstanceID * 6) + 1];
	vec4 i_color = vec4(InstanceData[(gl_InstanceID * 6) + 2], InstanceData[(gl_InstanceID * 6) + 3]);
	vec2 i_tex_add = InstanceData[(gl_InstanceID * 6) + 4];
	vec2 i_tex_mult = InstanceData[(gl_InstanceID * 6) + 5];

	vec2 _VertexPosition = VertexPosition * i_size;
	_VertexPosition = _VertexPosition + i_position;

	gl_Position = ProjectionMatrix * vec4(_VertexPosition, 0, 1);
	fs_in_col = VertexColor * i_color;
	fs_in_tex = (VertexTextureCoordinate * i_tex_mult) + i_tex_add;
	fs_in_texid = InstanceTexIds[gl_InstanceID];
}
";
	public static string Fragment = @"#version 300 es
precision mediump float;

in  vec4 fs_in_col;
in  vec2 fs_in_tex;
flat in int  fs_in_texid;
out vec4 FragColor;

uniform sampler2D tex;
uniform sampler2D tex2;
uniform sampler2D tex3;
uniform sampler2D tex4;

void main() {
	sampler2D tex_to_use;

	//This makes sure we have a default in the case of an invalid texture id getting to the shader
	//This also fixes a compile warning
	tex_to_use = tex;

	if(fs_in_texid == 0)
		tex_to_use = tex;
	else if(fs_in_texid == 1)
		tex_to_use = tex2;
	else if(fs_in_texid == 2)
		tex_to_use = tex3;
	else if(fs_in_texid == 3)
		tex_to_use = tex4;

	FragColor = texture(tex_to_use, fs_in_tex) * fs_in_col;
};";
}
