namespace ECTest; 

public static class Shaders {
	public static string Vertex = @"#version 300 es

layout (location = 0) in vec2 VertexPosition;
layout (location = 1) in vec4 VertexColor;
layout (location = 2) in vec2 VertexTextureCoordinate;

// uniform float RadianAngle;
out vec4 fs_in_col;
out vec2 fs_in_tex;

uniform mat4 ProjectionMatrix;
uniform vec2 InstanceData[128 * 5];

void main() {
	//mat2 rotation = mat2(cos(RadianAngle),sin(RadianAngle),
    //                -sin(RadianAngle),cos(RadianAngle));

	// gl_Position = mat4(rotation)*VertexPosition;

	vec2 i_position = InstanceData[gl_InstanceID * 5];
	vec2 i_size = InstanceData[(gl_InstanceID * 5) + 1];
	vec4 i_color = vec4(InstanceData[(gl_InstanceID * 5) + 2], InstanceData[(gl_InstanceID * 5) + 3]);
	vec2 i_tex_mult = InstanceData[(gl_InstanceID * 5) + 4];

	vec2 _VertexPosition = VertexPosition * i_size;
	_VertexPosition = _VertexPosition + i_position;

	gl_Position = ProjectionMatrix * vec4(_VertexPosition, 0, 1);
	fs_in_col = VertexColor * i_color;
	fs_in_tex = VertexTextureCoordinate * i_tex_mult;
}
";
	public static string Fragment = @"#version 300 es
precision mediump float;

in  vec4 fs_in_col;
in  vec2 fs_in_tex;
out vec4 FragColor;

uniform sampler2D tex;

void main() {
	FragColor = texture(tex, fs_in_tex) * fs_in_col;
};";
}
