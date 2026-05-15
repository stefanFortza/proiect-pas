shader_type spatial;
render_mode blend_add, depth_draw_never, unshaded, cull_disabled;

uniform vec4 base_color : source_color = vec4(0.2, 0.5, 1.0, 1.0);
uniform float glow_intensity : hint_range(0.0, 10.0) = 2.0;

void fragment() {
    float dist = distance(UV, vec2(0.5));
    float alpha = clamp(1.0 - dist * 2.0, 0.0, 1.0);
    alpha = pow(alpha, 3.0); // Sharpen the edges a bit
    
    vec4 color = base_color * COLOR; // Multiply by particle color (from Gradient)
    ALBEDO = color.rgb * glow_intensity;
    ALPHA = alpha * color.a;
}
