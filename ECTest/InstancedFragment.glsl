#version 300 es
precision mediump float;

in      vec4 fs_in_col;
in      vec2 fs_in_tex;
//The texture id 
flat in int  fs_in_texid;

//The final color of the pixel
out vec4 OutputColor;

//These are the bound textures
${UNIFORMS}
 
void main() {
    //This is the texture to be used in the sampler
    sampler2D tex_to_use;

    //This makes sure we have a default in the case of an invalid texture id getting to the shader
    //This also fixes a compile warning
    tex_to_use = tex_1;

${SELECT} 

    OutputColor = texture(tex_to_use, fs_in_tex) * fs_in_col;
};