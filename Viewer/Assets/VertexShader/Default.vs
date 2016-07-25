#version 150 core

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Model;

in vec3 Position;
in vec2 TextureCoordinates;

out vec2 TextureCoordinates0;

void main()
{		
    gl_Position = Projection * View * Model * vec4(Position, 1.0);   
    TextureCoordinates0 = TextureCoordinates;
}