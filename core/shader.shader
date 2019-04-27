shader_type canvas_item;

void fragment () {
    float speed = 0.4;
    float mult = 0.2;

    float time=TIME*speed;
    vec2 resolution = vec2(1024, 600);
    vec2 uv = (FRAGCOORD.xy / resolution.xx-0.5)*8.0;
    vec2 uv0=uv;
    float i0=1.0;
    float i1=1.0;
    float i2=1.0;
    float i4=0.0;
    for(int s=0;s<2;s++)
    {
      vec2 r;
      r=vec2(cos(uv.y*i0-i4+time/i1),sin(uv.x*i0-i4+time/i1))/i2;
      r+=vec2(-r.y,r.x)*0.3;
      uv.xy+=r;
          
      i0*=1.93;
      i1*=1.15;
      i2*=1.7;
      i4+=0.05+0.1*time*i1;
    }
    float r=sin(uv.x-time)*0.5+0.5;
    float b=sin(uv.y+time)*0.5+0.5;
    float g=sin((uv.x+uv.y+sin(time*0.5))*0.5)*0.5+0.5;
    COLOR = vec4(r * mult,g * mult,b * mult,1.0);
}
