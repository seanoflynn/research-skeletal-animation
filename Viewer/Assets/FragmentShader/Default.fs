#version 150 core

uniform vec4 Color;
uniform sampler2D TextureId;

in vec2 TextureCoordinates0;

out vec4 Color0;

void main(void)
{
	Color0 = texture(TextureId, TextureCoordinates0) * Color;
}
