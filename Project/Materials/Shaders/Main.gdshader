shader_type spatial;
render_mode depth_prepass_alpha;

uniform sampler2D _MainTexture : filter_nearest; 
instance uniform vec2 _XY;
instance uniform vec2 _WH;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALBEDO = texture(_MainTexture, _XY + (_WH * UV)).xyz;
	ALPHA = texture(_MainTexture, _XY + (_WH * UV)).a;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
