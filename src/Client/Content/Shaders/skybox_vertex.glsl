#version 410 core

layout (location = 0) in vec3 aPosition;

out vec3 texCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    // Per https://disqus.com/by/disqus_7NSOyKz2j7/ in the comments of https://learnopengl.com/Advanced-OpenGL/Cubemaps,
    // the Z coordinate needs to be flipped because OpenGL cubemaps use a left-handed coordinate system, which
    // differs from the right-handed coordinate system used by the camera in only the Z axis.
    texCoords = vec3(aPosition.xy, -aPosition.z);
    vec4 pos = projection * view * vec4(aPosition, 1.0);
    gl_Position = pos.xyww;
}  