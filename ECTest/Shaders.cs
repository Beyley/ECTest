namespace ECTest; 

public static class Shaders {
	public static string Vertex = @"
#version 300 es            

in vec4 VertexPosition, VertexColor;      
uniform float RadianAngle;
out vec4     TriangleColor;    

mat2 rotation = mat2(cos(RadianAngle),sin(RadianAngle),
                   -sin(RadianAngle),cos(RadianAngle));

void main() {
	gl_Position = mat4(rotation)*VertexPosition;
	TriangleColor = VertexColor;
}
";
	public static string Fragment = @"
#version 300 es        
precision mediump float;

in vec4   TriangleColor;
out vec4 FragColor;  
  
void main() {          
	FragColor = TriangleColor;
};";
}
