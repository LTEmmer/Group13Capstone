shader_type spatial;
render_mode unshaded;

uniform vec4 color_a : source_color = vec4(1.0, 0.2, 0.0, 1.0);
uniform vec4 color_b : source_color = vec4(0.0, 0.2, 1.0, 1.0);
uniform vec4 color_c : source_color = vec4(0.0, 0.8, 0.2, 1.0);

void fragment() {
    vec2 uv = UV;

    float mask = uv.y;

    // Slowly cycle through colors over time
    float t = sin(TIME * 0.5) * 0.5 + 0.5;
    float t2 = sin(TIME * 0.3 + 2.0) * 0.5 + 0.5;

    vec4 color = mix(color_a, color_b, t);
    color = mix(color, color_c, t2 * mask);

    ALBEDO = color.rgb;
}