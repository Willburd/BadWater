shader_type canvas_item;

uniform sampler2D _MainTexture : filter_nearest; 

uniform sampler2D _NoiseA;
uniform sampler2D _NoiseB;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	float xx = mod(UV.x,1.0);
	float yy = mod(UV.y,1.0);
	vec4 col = texture(_MainTexture, vec2( xx, yy)).rgba;
	if(col.g >= 0.999)
	{
		float val = texture(_NoiseA, vec2( mod((UV.x + (TIME * 0.02)) * 0.022, 1.0), mod((UV.y - (TIME * 0.0145)) * 0.01, 1.0))).r + texture(_NoiseB, vec2( mod((UV.x - (TIME * 0.03)) * 0.25, 1.0), mod((UV.y + (TIME * 0.087)) * 0.21, 1.0))).r;
		col.g = val * 0.9;
	}
	COLOR = col;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
