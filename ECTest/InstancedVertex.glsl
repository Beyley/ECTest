#version 300 es

//Vertex data
layout (location = 0) in vec2 VertexPosition;
layout (location = 1) in vec2 VertexTextureCoordinate;

//Instance data
layout (location = 2) in vec2  InstancePos;
layout (location = 3) in vec2  InstanceSize;
layout (location = 4) in vec4  InstanceColor;
layout (location = 5) in vec2  InstanceTexturePosition;
layout (location = 6) in vec2  InstanceTextureSize;
layout (location = 7) in float InstanceRotation;
layout (location = 8) in int   InstanceTextureId;


out      vec4 fs_in_col;
out      vec2 fs_in_tex;
flat out int  fs_in_texid;

uniform mat4 ProjectionMatrix;

void main() {
    mat2 rotation_matrix = mat2(cos(InstanceRotation),sin(InstanceRotation),
                               -sin(InstanceRotation),cos(InstanceRotation));

    //Scale up the vertex to the specified size
    vec2 _VertexPosition = vec2(mat4(rotation_matrix) * vec4(VertexPosition, 0, 0)) * InstanceSize;
    //Move the vertex by the offset of the instance
    _VertexPosition = _VertexPosition + InstancePos;

    //Apply our projection matrix
    gl_Position = ProjectionMatrix * vec4(_VertexPosition, 0, 1);
    
    fs_in_col = InstanceColor;
    fs_in_tex = (VertexTextureCoordinate * InstanceTextureSize) + InstanceTexturePosition;
    fs_in_texid = InstanceTextureId;
//    fs_in_texid = 0;
}