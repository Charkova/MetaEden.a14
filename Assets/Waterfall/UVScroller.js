// Scroll main texture based on time

//@script ExecuteInEditMode

var scrollSpeed = 0.1;
var yOffset = false;

function Update () 
{
	if(renderer.material.shader.isSupported)
		Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    var xoffset = Time.time * scrollSpeed;
   	if (yOffset) {yoffset=-1 * xoffset; xoffset=0;}    
    renderer.material.SetTextureOffset ("_MainTex", Vector2(yoffset, xoffset));
}