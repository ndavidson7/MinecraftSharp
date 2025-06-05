#version 410 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

out vec3 fragPosition;
out vec3 worldNormal;

uniform mat4 modelMatrix;
uniform mat4 transInvModelMatrix;
uniform mat4 mvpMatrix;

void main()
{
	gl_Position = mvpMatrix * vec4(position, 1.0);
	fragPosition = vec3(modelMatrix * vec4(position, 1.0));
	worldNormal = normalize(mat3(transInvModelMatrix) * normal);
}