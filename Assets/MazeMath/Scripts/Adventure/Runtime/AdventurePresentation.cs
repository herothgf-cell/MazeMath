using System;
using System.Globalization;
using UnityEngine;

namespace MazeMath.Adventure
{
    /// <summary>Presentation only: changes no collision, interaction, reward or save state.</summary>
    public sealed class AdventurePresentation : IDisposable
    {
        private readonly AdventureGame game;
        private readonly AdventureTheme theme;
        private readonly AdventureArt art;
        private Camera camera;
        private Rect oldRect;
        private Color oldBackground;
        private bool cameraCaptured;
        private Rect viewport=new Rect(0,0,1,1);
        private Transform currentRoot;
        private TextMesh[] labels=Array.Empty<TextMesh>();
        private SpriteRenderer[] owners=Array.Empty<SpriteRenderer>();
        public AdventurePresentation(AdventureGame game,AdventureTheme theme,AdventureArt art) { this.game=game; this.theme=theme; this.art=art; }
        public void SetViewport(Rect value)
        {
            viewport=value;
            if(camera==null) camera=Camera.main;
            if(camera==null) return;
            if(!cameraCaptured) { oldRect=camera.rect; oldBackground=camera.backgroundColor; cameraCaptured=true; }
            camera.rect=viewport;
            camera.backgroundColor=new Color32(214,232,240,255);
        }
        public void Update()
        {
            if(game==null || game.Motor==null) return;
            if(camera==null) SetViewport(viewport);
            var root=game.transform.Find("MineWorld");
            if(root!=currentRoot) { currentRoot=root; if(root!=null) StyleWorld(root); }
            TextMesh nearest=null; float nearestDistance=2.3f;
            for(int i=0;i<labels.Length;i++)
            {
                var label=labels[i]; if(label==null || IsPlate(label)) continue;
                if(owners[i]!=null && !owners[i].gameObject.activeInHierarchy) continue;
                float dx=Mathf.Abs(label.transform.position.x-game.Motor.X);
                float dy=Mathf.Abs(label.transform.position.y-(game.Motor.Y+1.5f));
                if(dy<3 && dx<nearestDistance) { nearest=label; nearestDistance=dx; }
            }
            for(int i=0;i<labels.Length;i++)
            {
                var label=labels[i]; if(label==null) continue;
                bool number=IsPlate(label);
                bool visible=number?Mathf.Abs(label.transform.position.x-game.Motor.X)<14 && Mathf.Abs(label.transform.position.y-game.Motor.Y)<5:label==nearest;
                visible=visible && (owners[i]==null || owners[i].gameObject.activeInHierarchy);
                label.gameObject.SetActive(visible);
                if(visible)
                {
                    label.color=number?theme.Gold:theme.Ink;
                    var renderer=label.GetComponent<MeshRenderer>();
                    if(renderer!=null)
                    {
                        // Actual mesh bounds prevent verbose object names from becoming giant billboards.
                        var bounds=renderer.bounds.size;
                        float factor=Mathf.Min(1,4.0f/Mathf.Max(.01f,bounds.x),.60f/Mathf.Max(.01f,bounds.y));
                        if(factor<.999f) label.transform.localScale*=factor;
                    }
                }
            }
        }
        private static bool IsPlate(TextMesh label) => label.text=="2" || label.text=="4" || label.text=="6";
        private void StyleWorld(Transform root)
        {
            var sprites=root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach(var r in sprites)
            {
                switch(r.name)
                {
                    case "Backdrop":
                        r.sprite=art.Get("solid"); int floor=Mathf.Clamp((int)(r.transform.position.y/8),0,2);
                        r.color=floor==0?new Color32(217,235,240,255):floor==1?new Color32(226,237,224,255):new Color32(239,231,218,255); break;
                    case "Pillar": r.sprite=art.Get("solid"); r.color=new Color32(197,216,217,255); break;
                    case "Moss": r.color=new Color32(165,207,177,255); break;
                    case "Stone": r.color=new Color32(177,204,197,255); break;
                    case "Soil": r.color=new Color32(171,189,180,255); break;
                    case "mined": r.color=new Color32(159,181,194,255); break;
                    case "bridge": case "repaired": case "repaired-top": r.color=new Color32(213,174,143,255); break;
                    case "laser": r.color=new Color32(145,198,197,255); break;
                    case "LadderRail": case "LadderRung": case "BridgePlanks": r.color=new Color32(210,169,129,255); break;
                    case "Memory": r.sprite=art.Get("solid"); r.color=new Color32(79,169,151,150); break;
                }
            }
            labels=root.GetComponentsInChildren<TextMesh>(true); owners=new SpriteRenderer[labels.Length];
            for(int i=0;i<labels.Length;i++)
            {
                var label=labels[i]; label.fontSize=40; label.characterSize=IsPlate(label)?.15f:.085f;
                label.color=theme.Ink;
                if(!IsPlate(label) && !theme.Korean) label.text=CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label.text.ToLowerInvariant());
                float best=float.MaxValue;
                foreach(var r in sprites)
                {
                    if(r.name=="Backdrop" || r.name=="Stone" || r.name=="Soil") continue;
                    float distance=Mathf.Abs(r.transform.position.x-label.transform.position.x)*4+Mathf.Abs(r.transform.position.y+1.2f-label.transform.position.y);
                    if(distance<best) { best=distance; owners[i]=r; }
                }
            }
        }
        public void Dispose()
        {
            if(cameraCaptured && camera!=null) { camera.rect=oldRect; camera.backgroundColor=oldBackground; }
        }
    }
}
