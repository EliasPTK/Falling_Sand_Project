shader_type canvas_item;
uniform vec4 values[1024]: source_color;
varying vec2 local_uv;
void vertex() {


}

void fragment() {
	vec2 new_local_uv = UV.xy * vec2(31.0,31.0);
	local_uv = floor(new_local_uv)/vec2(31.0,31.0);
	// Called for every pixel the material is visible on.
	int location = int(local_uv.x * 32.0) + int((local_uv.y * 992.0));
	//COLOR = vec4(float(location)/1024.0,float(location)/1024.0,float(location)/1024.0,1);
	//COLOR = vec4(local_uv.y,local_uv.y,local_uv.y,1);
	vec4 val = values[location];
	//if(local_uv.x * 31.0 <= 15.5){
	//	COLOR = sandCol;
	//}
	COLOR = val;
	//COLOR = vec4(local_uv.x,local_uv.x,local_uv.x,1);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
