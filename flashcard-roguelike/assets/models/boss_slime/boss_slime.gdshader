shader_type spatial;
render_mode depth_draw_always, cull_disabled;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D tex;

uniform float Specular : hint_range(0.0, 1.0) = 0.5;
uniform float Hardness : hint_range(1.0, 511.0, 1.0) = 50.0;
uniform vec3 BaseColor : source_color = vec3(1.0, 0.5, 0.0);
uniform vec3 HighlightColor : source_color = vec3(1.0, 0.9, 0.4);
uniform float Opacity : hint_range(0.0, 1.0) = 0.2;

uniform float refraction : hint_range(10000, 100000) = 60000.0;
uniform float time_scale = 1.0;
uniform float motion_strength = 0.08;

mat3 rotation_from_cam_translation(mat4 view_matrix, vec3 camera_position_world, vec3 node_position_world) {

	vec3 cam_offset_view_dir =
		(view_matrix * vec4(normalize(camera_position_world - node_position_world), 0.0)).xyz;

	vec3 dir1 = vec3(0, 0, 1);
	vec3 dir2 = cam_offset_view_dir;

	vec3 axis = normalize(cross(dir1, dir2));
	float cos_angle = dot(dir1, dir2);
	float angle = acos(cos_angle);
	float sin_angle = sin(angle);

	mat3 rot_matrix = mat3(
		vec3(
			cos_angle + axis.x * axis.x * (1.0 - cos_angle),
			axis.x * axis.y * (1.0 - cos_angle) - axis.z * sin_angle,
			axis.x * axis.z * (1.0 - cos_angle) + axis.y * sin_angle
		),
		vec3(
			axis.y * axis.x * (1.0 - cos_angle) + axis.z * sin_angle,
			cos_angle + axis.y * axis.y * (1.0 - cos_angle),
			axis.y * axis.z * (1.0 - cos_angle) - axis.x * sin_angle
		),
		vec3(
			axis.z * axis.x * (1.0 - cos_angle) - axis.y * sin_angle,
			axis.z * axis.y * (1.0 - cos_angle) + axis.x * sin_angle,
			cos_angle + axis.z * axis.z * (1.0 - cos_angle)
		)
	);

	return rot_matrix;
}

void fragment() {

	float t = TIME * time_scale;

	// Camera-facing normal correction (your original trick)
	NORMAL = rotation_from_cam_translation(
		VIEW_MATRIX,
		CAMERA_POSITION_WORLD,
		NODE_POSITION_WORLD
	) * NORMAL;

	// -------------------------------
	// SLIME SURFACE MOTION
	// -------------------------------

	// slow organic wobble field
	vec3 wobble = vec3(
		sin(UV.x * 6.0 + t) * cos(UV.y * 4.0 + t * 0.7),
		cos(UV.y * 6.0 + t) * sin(UV.x * 4.0 + t * 0.6),
		0.0
	) * motion_strength;

	NORMAL = normalize(NORMAL + wobble);

	// extra "goo drift" for screen-space distortion
	vec2 flow = vec2(
		sin(UV.y * 3.0 + t),
		cos(UV.x * 3.0 + t)
	) * 0.01;

	vec2 distorted_uv =
		SCREEN_UV + flow +
		vec2(-NORMAL.x * VIEWPORT_SIZE.y,
			  NORMAL.y * VIEWPORT_SIZE.x) / refraction;

	// -------------------------------
	// TEXTURE / REFRACTION
	// -------------------------------

	vec3 sample =
		texture(screen_texture, distorted_uv).rgb +
		texture(tex, UV + flow * 0.5).rgb;

	// density for slime “bulging”
	float density = 1.0 - dot(NORMAL, vec3(0.0, 0.0, 1.0));

	ALBEDO =
		mix(
			mix(sample * BaseColor, BaseColor, Opacity),
			HighlightColor,
			smoothstep(0.35, 0.75, density)
		);
}

void light() {

	vec3 L = normalize(LIGHT);
	vec3 N = normalize(NORMAL);

	float NdotL = dot(N, L) * ATTENUATION;

	vec3 V = normalize(VIEW);
	vec3 H = normalize(L + V);

	float specular = max(0.0, pow(dot(H, NORMAL), Hardness)) * Specular;

	vec3 finalSpecular =
		(vec3(specular) * LIGHT_COLOR / PI) *
		max(0.0, 1.0 - pow(1.0 - NdotL, 20));

	SPECULAR_LIGHT += finalSpecular;
	DIFFUSE_LIGHT += ATTENUATION * LIGHT_COLOR / PI;
}