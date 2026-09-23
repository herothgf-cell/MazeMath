using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeMath.Adventure
{
    /// <summary>Original, antialiased toy-robot illustrations. Generated once and cached; no external assets.</summary>
    public sealed class AdventureArt
    {
        private readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        private static readonly Color Outline=new Color32(94,125,138,255), Cream=new Color32(255,250,237,255),
            Mint=new Color32(157,218,198,255), Coral=new Color32(244,178,154,255), Blue=new Color32(173,216,238,255),
            Dark=new Color32(46,77,91,255), Gold=new Color32(244,207,129,255);
        public Material LineMaterial { get; private set; }
        public AdventureArt()
        {
            var shader=Shader.Find("Sprites/Default");
            if(shader!=null) { LineMaterial=new Material(shader); owned.Add(LineMaterial); }
        }
        public Sprite Get(string key)
        {
            if(cache.TryGetValue(key,out var sprite)) return sprite;
            bool tall=key=="player" || key=="golem";
            var p=new Painter(96,tall?136:96,tall?140:100);
            if(key=="solid") p.Rounded(0,0,100,100,0,Color.white);
            else if(key=="player") Player(p);
            else if(key=="robot") Robot(p);
            else if(key=="golem") Golem(p);
            else if(key=="heart")
            {
                p.Ellipse(32,62,23,23,new Color32(232,136,134,255)); p.Ellipse(68,62,23,23,new Color32(232,136,134,255));
                p.Polygon(new[]{new Vector2(10,56),new Vector2(90,56),new Vector2(50,12)},new Color32(232,136,134,255));
                p.Ellipse(27,69,8,4,new Color(1,1,1,.55f));
            }
            else if(key=="0")
            {
                p.Line(28,14,67,70,10,Outline); p.Line(28,14,67,70,6,Coral);
                p.Polygon(new[]{new Vector2(13,70),new Vector2(36,87),new Vector2(66,84),new Vector2(86,64),new Vector2(79,50),new Vector2( sixty(),66),new Vector2(34,72)},Outline);
                p.Line(23,75,44,81,9,Blue); p.Line(44,81,66,76,9,Blue); p.Line(66,76,80,62,9,Blue);
            }
            else if(key=="1")
            {
                p.Line(33,19,64,69,15,Outline); p.Line(33,19,64,69,10,Blue);
                p.Ellipse(33,19,10,10,Outline); p.Ellipse(33,19,5,5,Cream);
                p.Polygon(new[]{new Vector2(44,68),new Vector2(45,89),new Vector2(57,94),new Vector2(58,78),new Vector2(71,70),new Vector2(86,78),new Vector2(88,64),new Vector2(70,54)},Outline);
                p.Line(50,77,64,66,13,Mint); p.Line(64,66,78,68,13,Mint);
            }
            else if(key=="2")
            {
                p.Outlined(10,22,33,58,9,Outline,Mint); p.Outlined(56,22,33,58,9,Outline,Mint);
                p.Outlined(9,15,38,23,8,Outline,Cream); p.Outlined(53,15,38,23,8,Outline,Cream);
                p.Line(18,59,34,59,5,Blue); p.Line(64,59,80,59,5,Blue);
            }
            else if(key=="3")
            {
                p.Polygon(new[]{new Vector2(14,83),new Vector2(50,93),new Vector2(86,83),new Vector2(82,45),new Vector2(68,22),new Vector2(50,10),new Vector2(32,22),new Vector2(18,45)},Outline);
                p.Polygon(new[]{new Vector2(21,77),new Vector2(50,85),new Vector2(79,77),new Vector2(75,47),new Vector2(62,28),new Vector2(50,19),new Vector2(38,28),new Vector2(25,47)},Mint);
                p.Ellipse(50,57,18,18,Cream); p.Line(50,47,50,67,5,Blue); p.Line(40,57,60,57,5,Blue);
            }
            else if(key=="4")
            {
                p.Line(50,76,50,94,5,Outline); p.Ellipse(50,92,6,6,Coral);
                p.Outlined(15,17,70,65,18,Outline,Cream); p.Outlined(24,30,52,40,12,Outline,Blue);
                p.Ellipse(50,50,13,13,Mint); p.Ellipse(50,50,5,5,Dark); p.Ellipse(45,56,3,3,Color.white);
                p.Line(39,23,61,23,4,Coral);
            }
            else if(key=="5")
            {
                p.Outlined(32,68,36,23,9,Outline,Cream); p.Outlined(15,13,70,69,16,Outline,Coral);
                p.Outlined(25,23,50,29,9,Outline,Cream); p.Line(50,42,50,32,5,Gold);
                p.Line(24,66,76,66,4,new Color32(221,142,123,255));
            }
            else
            {
                // Generic world tile: crisp block face with a bright top edge and dark lower seam.
                p.Rounded(0,0,100,100,1,new Color32(89,96,77,255));
                p.Rounded(4,5,92,91,1,new Color32(225,221,199,255));
                p.Rounded(5,78,90,17,0,new Color32(240,236,214,255));
                p.Rounded(6,6,88,8,0,new Color32(179,174,151,255));
                p.Rounded(13,31,10,8,0,new Color32(201,196,173,255));
                p.Rounded(68,54,13,9,0,new Color32(197,192,169,255));
                p.Rounded(38,18,7,7,0,new Color32(236,231,208,255));
            }
            var texture=new Texture2D(p.Width,p.Height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="Momo illustration / "+key};
            texture.SetPixels(p.Pixels); texture.Apply(false,true);
            sprite=Sprite.Create(texture,new Rect(0,0,p.Width,p.Height),new Vector2(.5f,.5f),p.Width);
            owned.Add(texture); owned.Add(sprite); cache[key]=sprite; return sprite;
        }
        private static float sixty() => 60;
        private static void Player(Painter p)
        {
            p.Ellipse(50,8,33,5,new Color(.25f,.37f,.4f,.15f));
            p.Outlined(22,10,24,24,8,Outline,Cream); p.Outlined(54,10,24,24,8,Outline,Cream);
            p.Outlined(22,30,56,57,17,Outline,Mint);
            p.Ellipse(15,61,11,17,Cream); p.Ellipse(85,61,11,17,Cream);
            p.Outlined(5,76,90,60,22,Outline,Cream); p.Outlined(14,89,72,34,13,Outline,Blue);
            p.Ellipse(34,105,4,6,Dark); p.Ellipse(66,105,4,6,Dark);
            p.Ellipse(23,98,5,3,Coral); p.Ellipse(77,98,5,3,Coral);
            p.Line(45,96,50,94,2,Dark); p.Line(50,94,55,96,2,Dark);
            p.Rounded(24,73,52,10,4,Coral); p.Rounded(62,62,11,18,4,Coral);
            p.Outlined(35,42,30,23,7,Outline,Cream); p.Ellipse(50,54,6,6,Gold);
            p.Line(20,128,40,130,3,Color.white);
        }
        private static void Robot(Painter p)
        {
            p.Ellipse(50,9,33,4,new Color(.25f,.37f,.4f,.12f));
            p.Line(50,76,50,94,4,Outline); p.Ellipse(50,94,5,5,Coral);
            p.Ellipse(13,50,10,17,Mint); p.Ellipse(87,50,10,17,Mint);
            p.Outlined(14,20,72,59,19,Outline,Cream); p.Outlined(22,36,56,32,12,Outline,Mint);
            p.Ellipse(36,53,4,6,Dark); p.Ellipse(64,53,4,6,Dark);
            p.Ellipse(28,44,5,3,Coral); p.Ellipse(72,44,5,3,Coral);
            p.Line(46,43,50,41,2,Dark); p.Line(50,41,54,43,2,Dark);
            p.Ellipse(50,27,5,5,Gold); p.Rounded(26,10,17,15,5,Mint); p.Rounded(57,10,17,15,5,Mint);
        }
        private static void Golem(Painter p)
        {
            Color stone=new Color32(171,198,208,255);
            p.Outlined(19,8,26,30,9,Outline,stone); p.Outlined(55,8,26,30,9,Outline,stone);
            p.Outlined(18,36,64,58,17,Outline,stone);
            p.Outlined(3,39,21,49,10,Outline,stone); p.Outlined(76,39,21,49,10,Outline,stone);
            p.Outlined(12,86,76,46,17,Outline,Cream);
            p.Ellipse(33,109,5,6,Dark); p.Ellipse(67,109,5,6,Dark);
            p.Ellipse(22,99,6,3,Coral); p.Ellipse(78,99,6,3,Coral);
            p.Line(43,99,50,96,3,Dark); p.Line(50,96,57,99,3,Dark);
            p.Rounded(22,124,58,12,6,Mint); p.Ellipse(43,134,16,5,Mint);
            p.Outlined(34,48,32,33,9,Outline,Mint); p.Ellipse(50,66,9,11,Gold);
            p.Line(27,81,38,81,3,Cream);
        }
        private sealed class Painter
        {
            public readonly int Width,Height;
            public readonly Color[] Pixels;
            private readonly float logicalHeight;
            public Painter(int w,int h,float extent) { Width=w; Height=h; logicalHeight=extent; Pixels=new Color[w*h]; }
            private void Paint(Func<float,float,float> distance,Color c)
            {
                for(int y=0;y<Height;y++)for(int x=0;x<Width;x++)
                {
                    float a=Mathf.Clamp01(.65f-distance((x+.5f)*100/Width,(y+.5f)*logicalHeight/Height))*c.a;
                    if(a<=0)continue;
                    int i=y*Width+x; var d=Pixels[i]; float alpha=a+d.a*(1-a);
                    Pixels[i]=new Color((c.r*a+d.r*d.a*(1-a))/alpha,(c.g*a+d.g*d.a*(1-a))/alpha,(c.b*a+d.b*d.a*(1-a))/alpha,alpha);
                }
            }
            public void Rounded(float x,float y,float w,float h,float radius,Color c)
            {
                Paint((px,py)=>{float dx=Mathf.Abs(px-x-w/2)-(w/2-radius),dy=Mathf.Abs(py-y-h/2)-(h/2-radius);return Mathf.Sqrt(Mathf.Max(dx,0)*Mathf.Max(dx,0)+Mathf.Max(dy,0)*Mathf.Max(dy,0))+Mathf.Min(Mathf.Max(dx,dy),0)-radius;},c);
            }
            public void Outlined(float x,float y,float w,float h,float radius,Color edge,Color fill)
            {
                Rounded(x,y,w,h,radius,edge); Rounded(x+2.5f,y+2.5f,w-5,h-5,Mathf.Max(0,radius-2.5f),fill);
            }
            public void Ellipse(float x,float y,float rx,float ry,Color c)
            {
                Paint((px,py)=>(Mathf.Sqrt((px-x)*(px-x)/(rx*rx)+(py-y)*(py-y)/(ry*ry))-1)*Mathf.Min(rx,ry),c);
            }
            public void Line(float x0,float y0,float x1,float y1,float thickness,Color c)
            {
                var a=new Vector2(x0,y0); var b=new Vector2(x1,y1); var ab=b-a;
                Paint((x,y)=>{var p=new Vector2(x,y);float f=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.001f,ab.sqrMagnitude));return Vector2.Distance(p,a+ab*f)-thickness/2;},c);
            }
            public void Polygon(Vector2[] v,Color c)
            {
                Paint((x,y)=>{bool inside=false;float distance=float.MaxValue;var p=new Vector2(x,y);
                    for(int i=0,j=v.Length-1;i<v.Length;j=i++)
                    {
                        var a=v[i];var b=v[j];var ab=b-a;
                        if((a.y>y)!=(b.y>y) && x<(b.x-a.x)*(y-a.y)/(b.y-a.y)+a.x)inside=!inside;
                        float f=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.001f,ab.sqrMagnitude));distance=Mathf.Min(distance,Vector2.Distance(p,a+ab*f));
                    }
                    return inside?-distance:distance;
                },c);
            }
        }
        public void Dispose() { foreach(var o in owned)if(o!=null)UnityEngine.Object.Destroy(o); owned.Clear(); cache.Clear(); }
    }
}
