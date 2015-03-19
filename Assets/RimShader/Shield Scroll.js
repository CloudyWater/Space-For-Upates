var scrollX = 5.0*Time.time;
var scrollY = 5.0*Time.time;

function Update() {
    var offset : float = Time.time * scrollX;
    var offset2 : float = Time.time * scrollY;
    GetComponent.<Renderer>().material.SetTextureOffset ("_ShieldTexture", Vector2(offset, offset2));
    GetComponent.<Renderer>().material.SetTextureOffset ("_ShieldTex2", Vector2(offset, offset2)*2);
}