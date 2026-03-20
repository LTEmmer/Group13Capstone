shader_type spatial;
render_mode blend_mix, depth_draw_always, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 shallow_color : source_color = vec4(0.1, 0.5, 0.8, 0.85);
uniform vec4 deep_color    : source_color = vec4(0.02, 0.15, 0.4, 0.95);
uniform float wave_speed   = 0.6;
uniform float wave_scale   = 3.0;
uniform float fresnel_power = 3.0;
uniform float emission_strength = 0.2;

void fragment() {
	vec2 uv1 = UV * wave_scale + vec2(TIME * wave_speed * 0.4, TIME * wave_speed * 0.2);
	vec2 uv2 = UV * wave_scale * 1.3 - vec2(TIME * wave_speed * 0.3, TIME * wave_speed * 0.5);

	float nx = sin(uv1.x * 6.2832) * 0.08 + sin(uv2.x * 6.2832) * 0.05;
	float ny = cos(uv1.y * 6.2832) * 0.08 + cos(uv2.y * 6.2832) * 0.05;

	vec3 n = normalize(vec3(nx, ny, 1.0));
	NORMAL_MAP = n * 0.5 + 0.5;

	float fresnel = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), fresnel_power);
	vec4 water_color = mix(deep_color, shallow_color, fresnel);

	ALBEDO = water_color.rgb;
	ALPHA = water_color.a;
	EMISSION = water_color.rgb * emission_strength;
	ROUGHNESS = 0.05;
	METALLIC = 0.3;
	SPECULAR = 0.9;
}
