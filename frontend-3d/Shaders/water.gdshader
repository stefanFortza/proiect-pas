shader_type spatial;
render_mode cull_disabled;

uniform vec4 albedo : source_color = vec4(0.1, 0.3, 0.5, 1.0);
uniform vec4 albedo2 : source_color = vec4(0.0, 0.1, 0.2, 1.0);
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float roughness : hint_range(0.0, 1.0) = 0.02;

uniform sampler2D wave_noise;
uniform float wave_time = 0.0;

// Gerstner Wave Parameters: Vector4(DirectionX, DirectionZ, Steepness, Wavelength)
uniform vec4 wave_a = vec4(1.0, 1.0, 0.15, 10.0);
uniform vec4 wave_b = vec4(1.0, 0.6, 0.10, 5.0);
uniform vec4 wave_c = vec4(1.0, 1.3, 0.10, 3.0);

uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

uniform float beer_law_factor = 2.0;
uniform float foam_distance = 0.1;
uniform vec4 foam_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

varying float height;
varying vec3 world_pos;

vec3 gerstner_wave(vec4 wave, vec3 p, inout vec3 tangent, inout vec3 binormal) {
    float steepness = wave.z;
    float wavelength = wave.w;
    float k = 2.0 * PI / wavelength;
    float c = sqrt(9.8 / k);
    vec2 d = normalize(wave.xy);
    float f = k * (dot(d, p.xz) - c * TIME);
    float a = steepness / k;

    tangent += vec3(
        -d.x * d.x * (steepness * sin(f)),
        d.x * (steepness * cos(f)),
        -d.x * d.y * (steepness * sin(f))
    );
    binormal += vec3(
        -d.x * d.y * (steepness * sin(f)),
        d.y * (steepness * cos(f)),
        -d.y * d.y * (steepness * sin(f))
    );

    return vec3(
        d.x * (a * cos(f)),
        a * sin(f),
        d.y * (a * cos(f))
    );
}

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    vec3 p = world_pos;
    
    vec3 tangent = vec3(1.0, 0.0, 0.0);
    vec3 binormal = vec3(0.0, 0.0, 1.0);
    vec3 offset = vec3(0.0);
    
    offset += gerstner_wave(wave_a, p, tangent, binormal);
    offset += gerstner_wave(wave_b, p, tangent, binormal);
    offset += gerstner_wave(wave_c, p, tangent, binormal);
    
    vec3 normal = normalize(cross(binormal, tangent));
    NORMAL = (VIEW_MATRIX * vec4(normal, 0.0)).xyz;
    
    VERTEX += offset;
    height = offset.y;
}

void fragment() {
    float depth_raw = texture(depth_texture, SCREEN_UV).r;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw);
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float linear_depth = -view.z;
    float screen_depth = linear_depth;
    
    float water_depth = -(INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, FRAGCOORD.z, 1.0)).z / view.w;
    float depth_diff = screen_depth - FRAGCOORD.z; // Simplified depth diff
    
    // Better depth calculation
    vec4 world = INV_VIEW_MATRIX * view;
    float depth = (world.y - world_pos.y);
    
    // Beer's Law for water transparency/color
    float beer_law = exp(-depth * beer_law_factor);
    vec3 water_color = mix(albedo2.rgb, albedo.rgb, beer_law);
    
    // Foam
    float foam_line = smoothstep(foam_distance, 0.0, depth);
    vec3 final_color = mix(water_color, foam_color.rgb, foam_line);
    
    ALBEDO = final_color;
    METALLIC = metallic;
    ROUGHNESS = roughness;
    SPECULAR = 0.5;
    ALPHA = mix(1.0, albedo.a, beer_law);
}
