#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec3 fragPosition;
out vec3 worldNormal;
out vec2 texCoords;

uniform mat4 modelMatrix;
uniform mat4 transInvModelMatrix;
uniform mat4 mvpMatrix;

void main()
{
	gl_Position = mvpMatrix * vec4(aPosition, 1.0);
	fragPosition = vec3(modelMatrix * vec4(aPosition, 1.0));
	worldNormal = normalize(mat3(transInvModelMatrix) * aNormal);
	texCoords = aTexCoords;
}