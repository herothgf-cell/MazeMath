using System.Collections.Generic;
using UnityEngine;

namespace MazeMath.Adventure
{
    // Original procedural pixel art: no borrowed Minecraft assets or external font dependency.
    public sealed class AdventureArt
    {
        private readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        private readonly List<Object> owned = new List<Object>();
        public Material LineMaterial { get; private set; }
        public AdventureArt() { var shader = Shader.Find("Sprites/Default"); if (shader != null) { LineMaterial = new Material(shader); owned.Add(LineMaterial); } }
        public Sprite Get(string key)
        {
            if (cache.TryGetValue(key, out var s)) return s;
            string[] rows;
            switch (key)
            {
                case "player": rows = new[]{"...bbbb...","..bbbbbb..","..bttttb..","..tttkt...","...ttt....","..ggggg...",".tgggggt..",".tgggggt..","..ggggg...","..ddd d...".Replace(" ","d"),"..dd.dd...","..kk.kk..."}; break;
                case "robot": rows = new[]{"..g..g..",".gggggg.",".gkkgkg.",".gggggg.","..dddd..",".gdwwdg.","..dddd..","..g..g.."}; break;
                case "heart": rows = new[]{".rr.rr.","rrrrrrr","rrrrrrr",".rrrrr.","..rrr..","...r..."}; break;
                case "0": rows = new[]{".www....","ww.ww...","....ww..",".....ww.","....ww..","...ww...","..ww....","..w....."}; break;
                case "1": rows = new[]{".w..w...",".wwww...","..ww....","..ww....","..ww....","..ww....",".www....","..w....."}; break;
                case "2": rows = new[]{".gg.gg..",".gg.gg..",".gg.gg..",".gg.gg..",".gggggg.",".ww.ww..","........","........"}; break;
                case "3": rows = new[]{".wwwww..",".wgggw..",".wgggw..",".wgggw..","..wgw...","...w....","........","........"}; break;
                case "4": rows = new[]{"...c....","..ccc...",".ccwcc..","ccwwwcc.",".ccwcc..","..ccc...","...c....","........"}; break;
                case "5": rows = new[]{"..bbb...",".bwwwb..","bbbbbbb.","btttttb.","btwwwtb.","btttttb.","bbbbbbb.","........"}; break;
                case "golem": rows = new[]{"..dddddd..",".dwwwwwwd.",".dwkwwkwd.",".dwwwwwwd.","..dddddd..","ddggggggdd","ddggccggdd","ddggggggdd","..dddddd..","..dd..dd..","..dd..dd..",".ddd..ddd."}; break;
                default: rows = new[]{"wwwwwwww","wggggggd","wggdgggd","wggggggd","wggggdgd","wgdggggd","wggggggd","dddddddd"}; break;
            }
            int h = rows.Length, w = rows[0].Length;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "MazeMath " + key };
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) texture.SetPixel(x, h - y - 1, ColorFor(x < rows[y].Length ? rows[y][x] : '.'));
            texture.Apply(); s = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(.5f, .5f), w);
            owned.Add(texture); owned.Add(s); cache[key] = s; return s;
        }
        private static Color ColorFor(char c)
        {
            switch (c) { case 'w': return new Color32(207,220,172,255); case 'g': return new Color32(115,148,75,255); case 'd': return new Color32(54,73,44,255); case 'b': return new Color32(83,60,36,255); case 't': return new Color32(213,183,121,255); case 'k': return new Color32(20,30,17,255); case 'r': return new Color32(218,83,70,255); case 'c': return new Color32(79,218,188,255); default: return Color.clear; }
        }
        public void Dispose() { foreach (var o in owned) if (o != null) Object.Destroy(o); owned.Clear(); cache.Clear(); }
    }
}
