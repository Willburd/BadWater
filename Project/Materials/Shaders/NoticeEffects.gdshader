shader_type spatial;
render_mode depth_test_disabled,depth_draw_always,unshaded;

uniform sampler2D _MainTexture : filter_nearest; 
instance uniform vec2 _XY;
instance uniform vec2 _WH;
instance uniform float _AA;
instance uniform float _CF;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec3 col = texture(_MainTexture, _XY + (_WH * UV)).xyz;
	ALBEDO = mix(col,vec3(1.0,1.0,6.0),_CF * 0.3); 
	ALPHA = texture(_MainTexture, _XY + (_WH * UV)).a * _AA;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
