shader_type canvas_item;
uniform sampler2D screen_tex : hint_screen_texture;
uniform int radius : hint_range(1, 5) = 2;
uniform float strength = 1.0;

void fragment() {
    vec2 ps = SCREEN_PIXEL_SIZE * strength;
    vec4 sum = vec4(0.0);
    int count = 0;
    
    for (int x = -radius; x <= radius; x++) {
        for (int y = -radius; y <= radius; y++) {
            vec2 offset = vec2(float(x), float(y)) * ps;
            sum += texture(screen_tex, SCREEN_UV + offset);
            count++;
        }
    }
    COLOR = sum / float(count);
}