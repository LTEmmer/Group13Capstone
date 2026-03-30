shader_type canvas_item;
uniform float scroll_speed : hint_range(0.0, 0.5) = 0.005;

void fragment() {
	vec2 uv = UV;
	uv.x = fract(uv.x + TIME * scroll_speed);
	COLOR = texture(TEXTURE, uv);
}
