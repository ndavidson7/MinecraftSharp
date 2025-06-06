#version 410 core

in vec3 fragPosition;
in vec3 worldNormal;
in vec2 texCoords;

out vec4 fragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};

struct DirectionalLightSource {
    vec3 direction;

    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
};

struct PointLightSource {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
};

uniform vec3 viewPosition;
uniform Material material;
uniform DirectionalLightSource sun;

vec4 calculateDirectionalLighting(DirectionalLightSource light, vec3 normal, vec3 viewDir, vec4 texDiffuse)
{
    vec3 lightDir = normalize(-light.direction);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    vec4 ambient = light.ambient * texDiffuse;
    vec4 diffuse = light.diffuse * diff * texDiffuse;
    vec4 specular = light.specular * spec * texture(material.specular, texCoords);

    return (ambient + diffuse + specular);
}

vec4 calculatePointLighting(PointLightSource light, vec3 normal, vec3 fragPos, vec3 viewDir, vec4 texDiffuse)
{
    vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    
    // combine results
    vec4 ambient = light.ambient * texDiffuse * attenuation;
    vec4 diffuse = light.diffuse * diff * texDiffuse * attenuation;
    vec4 specular = light.specular * spec * texture(material.specular, texCoords) * attenuation;
    
    return (ambient + diffuse + specular);
}

void main()
{
    vec3 viewDir = normalize(viewPosition - fragPosition);
    vec4 texDiffuse = texture(material.diffuse, texCoords);

    // directional lighting
    vec4 result = calculateDirectionalLighting(sun, worldNormal, viewDir, texDiffuse);

    // point lighting
    //for(int i = 0; i < numPointLights; i++)
    //  result += calculatePointLighting(pointLights[i], worldNormal, fragPosition, viewDir, texDiffuse);

    fragColor = result;
}