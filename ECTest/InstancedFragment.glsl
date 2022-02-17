#version 300 es
precision mediump float;

in      vec4 fs_in_col;
in      vec2 fs_in_tex;
//The texture id 
flat in int  fs_in_texid;

//The final color of the pixel
out vec4 OutputColor;

//These are the bound textures
uniform sampler2D tex_1;
uniform sampler2D tex_2;
uniform sampler2D tex_3;
uniform sampler2D tex_4;
 
void main() {
    //This is the texture to be used in the sampler
    sampler2D tex_to_use;

    //This makes sure we have a default in the case of an invalid texture id getting to the shader
    //This also fixes a compile warning
    tex_to_use = tex_1;

    if(fs_in_texid == 0) {
        tex_to_use = tex_1;
//        OutputColor = vec4(1,0,0,1);
    }
    else if(fs_in_texid == 1) { 
        tex_to_use = tex_2;
//        OutputColor = vec4(0,1,0,1);
    }
    else if(fs_in_texid == 2) { 
        tex_to_use = tex_3;
//        OutputColor = vec4(0,0,1,1);
    }
    else if(fs_in_texid == 3) { 
        tex_to_use = tex_4;
//        OutputColor = vec4(1,1,1,1);
    }

    OutputColor = texture(tex_to_use, fs_in_tex) * fs_in_col;
};