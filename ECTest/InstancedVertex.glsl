#version 300 es

layout (location = 0) in vec2 VertexPosition;
//I dont think we need to actually upload VertexColor here, we could save on some bytes
layout (location = 1) in vec4 VertexColor;
layout (location = 2) in vec2 VertexTextureCoordinate;

out      vec4 fs_in_col;
out      vec2 fs_in_tex;
flat out int  fs_in_texid;

const int NumInstances = 128;

//The reason we use vec4 everywhere is due to it being padded up to that size anyway
layout(std140) uniform InstanceData {
    //We pack both the position (vec2) and size (vec2) into a vec4
    vec4 InstancePositionSizes[NumInstances];
    //Color is a vec4 anyway, so nothing special happens here
    vec4 InstanceColors[NumInstances];
    //We pack the position (vec2) and the scale (vec2) of the texture crop into a vec4
    vec4 InstanceTexturePositionScales[NumInstances];
    //Rotations are a single float, so we pack 4 rotations into a single vec4
    vec4 InstanceRotations[NumInstances / 4];
    //Texture IDs are a single integer, so we pack 4 IDs into a single ivec4
    ivec4 InstanceTextureIds[NumInstances / 4];
};

uniform mat4 ProjectionMatrix;

void main() {
    //The quad's offset from 0,0 (bottom left)
    vec2 i_position = vec2(InstancePositionSizes[gl_InstanceID].xy);
    //The size of the total quad
    vec2 i_size = vec2(InstancePositionSizes[gl_InstanceID].zw);
    //The colour of the quad
    vec4 i_color = InstanceColors[gl_InstanceID];
    //The bottom left position of the texture crop (for full, you wanna do (0, 0))
    vec2 i_tex_add = vec2(InstanceTexturePositionScales[gl_InstanceID].xy);
    //The size of the texture crop (for full, you wanna do (1, 1))
    vec2 i_tex_mult = vec2(InstanceTexturePositionScales[gl_InstanceID].zw);
    //The rotation of the quad, in radians
    float i_rotation = InstanceRotations[gl_InstanceID / 4][gl_InstanceID % 4];

    mat2 rotation_matrix = mat2(cos(i_rotation),sin(i_rotation),
                               -sin(i_rotation),cos(i_rotation));

    //Scale up the vertex to the specified size
    vec2 _VertexPosition = vec2(mat4(rotation_matrix) * vec4(VertexPosition, 0, 0)) * i_size;
    //Move the vertex by the offset of the instance
    _VertexPosition = _VertexPosition + i_position;

    //Apply our projection matrix
    gl_Position = ProjectionMatrix * vec4(_VertexPosition, 0, 1);
    
    fs_in_col = VertexColor * i_color;
    fs_in_tex = (VertexTextureCoordinate * i_tex_mult) + i_tex_add;
    fs_in_texid = InstanceTextureIds[gl_InstanceID / 4][gl_InstanceID % 4];
//    fs_in_texid = 0;
}