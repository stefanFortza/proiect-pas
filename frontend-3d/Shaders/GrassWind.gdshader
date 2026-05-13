shader_type spatial;
render_mode cull_disabled;

uniform vec3 base_color : source_color = vec3(0.1, 0.4, 0.1);
uniform vec3 tip_color : source_color = vec3(0.3, 0.7, 0.2);

uniform float wind_speed = 1.0;
uniform float wind_strength = 0.2;
uniform float wind_scale = 0.5;

varying float v_height;
varying float v_variation;

void vertex() {
    v_height = VERTEX.y;
    v_variation = INSTANCE_CUSTOM.r;
    
    // Calculate world position for noise continuity
    vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    
    // Use INSTANCE_CUSTOM.g for per-instance wind phase offset
    float phase = world_pos.x * wind_scale + world_pos.z * wind_scale + INSTANCE_CUSTOM.g * 10.0;
    
    // Simple wind using sine waves
    float wind = sin(TIME * wind_speed + phase);
    wind += sin(TIME * wind_speed * 1.5 + phase * 0.5);
    
    // Only sway the top part of the grass
    float sway = v_height * wind * wind_strength;
    
    VERTEX.x += sway;
    VERTEX.z += sway;
}

void fragment() {
    // Apply variation to colors
    vec3 local_base = mix(base_color, base_color * 0.8, v_variation);
    vec3 local_tip = mix(tip_color, tip_color * 1.2, v_variation);
    
    ALBEDO = mix(local_base, local_tip, clamp(v_height, 0.0, 1.0));
    ROUGHNESS = 0.8;
}
