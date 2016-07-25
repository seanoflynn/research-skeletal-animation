#version 150 core

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Model;

uniform mat4 Bones[50];

in vec3 Position;
in vec2 TextureCoordinates;
in vec4 Index;
in vec4 Weight;

out vec2 TextureCoordinates0;

void main()
{
  vec4 newPosition = vec4(0.0);
  int index = 0;

  for(int i=0; i<4; i++)
  {		
    index = int(Index[i]);	
    newPosition += (Bones[index] * vec4(Position,1.0)) * Weight[i];		
  }

  gl_Position = Projection * View * Model * vec4(newPosition.xyz, 1.0);
  TextureCoordinates0 = TextureCoordinates;
}