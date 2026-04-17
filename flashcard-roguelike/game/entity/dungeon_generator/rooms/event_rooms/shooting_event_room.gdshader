shader_type spatial;
render_mode unshaded;

uniform sampler2D albedo_texture : source_color;
uniform vec2 scroll_speed = vec2(0.0, 0.5);

void fragment() {
    vec2 uv = UV + scroll_speed * TIME;
    ALBEDO = texture(albedo_texture, uv).rgb;
}