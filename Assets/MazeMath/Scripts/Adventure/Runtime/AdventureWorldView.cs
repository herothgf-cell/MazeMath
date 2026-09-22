using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeMath.Adventure
{
    public sealed class AdventureThing
    {
        public string id, ko, en, icon;
        public float x, y;
        public SpriteRenderer sprite;
        public TextMesh label;
        public AdventureThing(string id, float x, float y, string ko, string en, string icon = "tile")
        { this.id = id; this.x = x; this.y = y; this.ko = ko; this.en = en; this.icon = icon; }
    }

    public sealed class AdventureWorldView
    {
        public readonly List<AdventureThing> Things = new List<AdventureThing>();
        private readonly AdventureTheme theme;
        private readonly AdventureArt art;
        private readonly Transform root;
        private readonly Dictionary<string, GameObject> gates = new Dictionary<string, GameObject>();
        private Transform player, momo, carry;
        private readonly Dictionary<int, GameObject> ladderViews = new Dictionary<int, GameObject>();
        private SpriteRenderer playerSprite;
        private LineRenderer laser, bossLaser;
        private TextMesh bossLabel;
        private readonly List<TextMesh> plateLabels = new List<TextMesh>();
        private readonly List<TextMesh> bossPlateLabels = new List<TextMesh>();
        private readonly List<SpriteRenderer> memoryTrail = new List<SpriteRenderer>();
        private float lastX;
        public AdventureWorldView(Transform root, AdventureTheme theme, AdventureArt art) { this.root = root; this.theme = theme; this.art = art; }

        public void Build(AdventureState state)
        {
            for (int f = 0; f < 3; f++)
            {
                Quad("Backdrop",30,f * 8 + 3,60,8,new Color(.12f + f*.015f,.20f + f*.01f,.13f),-30);
                for (int x = 0; x < 60; x++)
                {
                    if (f == 0 && x >= 47 && x < 50) continue;
                    Sprite("Stone",new Vector3(x+.5f,f*8-.5f),"tile",Vector2.one,new Color(.63f,.71f,.52f),-1);
                    if (f == 0) for (int y = 1; y < 4; y++) Sprite("Soil",new Vector3(x+.5f,-y-.5f),"tile",Vector2.one,new Color(.3f,.36f,.27f),-2);
                    if (x % 4 == 1) { Quad("Pillar",x,f*8+3, .35f,6,new Color(.16f,.25f,.17f),-20); Quad("Moss",x+.4f,f*8+4,.9f,.18f,new Color(.28f,.37f,.21f),-19); }
                }
                for (int room = 0; room < 5; room++)
                {
                    var r = Sprite("Memory",new Vector3(room*12+6, f*8+.08f),"tile",new Vector2(9,.1f),new Color(.6f,.9f,.3f,.35f),0);
                    memoryTrail.Add(r); r.enabled = false;
                }
            }
            Gate("mined",36,0,new Color(.46f,.48f,.36f));
            Gate("bridge",46.2f,0,new Color(.64f,.46f,.24f));
            Gate("laser",36,8,new Color(.32f,.57f,.52f));
            Gate("repaired",12,8,new Color(.45f,.38f,.30f));
            Gate("repaired-top",12,16,new Color(.45f,.38f,.30f));
            gates["bridge-floor"] = Quad("BridgePlanks",48.5f,-.12f,3,.25f,new Color(.73f,.59f,.34f),1).gameObject;
            Ladder(54,0,8); Ladder(30,8,16); Ladder(18,0,8);
            Quad("HighPlatform",6,18.85f,6,.3f,theme.Stone,1);
            Things.Add(new AdventureThing("entrance",6,0,"폐광 입구 · 이동하고 조사해 보세요","ENTRANCE · Move and interact","robot"));
            Things.Add(new AdventureThing("workshop",18,0,"모모의 작업대","WORKBENCH","1"));
            Things.Add(new AdventureThing("math",30,0,"숫자 제어기","NUMBER TERMINAL","4"));
            Things.Add(new AdventureThing("mine",34,0,"균열벽 · 곡괭이 팔","CRACKED WALL","0"));
            Things.Add(new AdventureThing("box0",38,0,"3kg 상자","3kg CRATE","5"));
            Things.Add(new AdventureThing("box1",40.5f,0,"5kg 상자","5kg CRATE","5"));
            Things.Add(new AdventureThing("box2",43,0,"2kg 상자","2kg CRATE","5"));
            Things.Add(new AdventureThing("scale",44.7f,0,"저울 · 목표 8kg","SCALE · TARGET 8kg","3"));
            Things.Add(new AdventureThing("ladderA",54,0,"사다리 · 위/아래","LADDER · UP / DOWN","tile"));
            Things.Add(new AdventureThing("cache-a",57,8,"재료 상자","SUPPLY CHEST","5"));
            Things.Add(new AdventureThing("mirrorA",38,8,"거울 A 회전","ROTATE MIRROR A","4"));
            Things.Add(new AdventureThing("mirrorB",45,8,"거울 B 회전","ROTATE MIRROR B","4"));
            Things.Add(new AdventureThing("checkpoint",30,8,"체크포인트 / 위층 사다리","CHECKPOINT / UPPER LADDER","robot"));
            Things.Add(new AdventureThing("sequence",25,8,"단서: 2 → 4 → 6","CLUE: 2 → 4 → 6","4"));
            Things.Add(new AdventureThing("repair",13.5f,8,"고장 난 장치 · 파워 렌치","BROKEN MACHINE · WRENCH","1"));
            Things.Add(new AdventureThing("cache-b",6,8,"탐험 보물","EXPLORER CHEST","5"));
            Things.Add(new AdventureThing("high-cache",6,19,"높은 보물 · 점프 부스터","HIGH CHEST · JUMP BOOTS","5"));
            Things.Add(new AdventureThing("checkpoint-top",30,16,"보스 전 체크포인트","BOSS CHECKPOINT","robot"));
            Things.Add(new AdventureThing("boss.math",38,16,"골렘의 숫자 장치","GOLEM NUMBER LOCK","4"));
            Things.Add(new AdventureThing("boss.sequence",40,16,"발판: 2 → 4 → 6","PLATES: 2 → 4 → 6","tile"));
            Things.Add(new AdventureThing("boss.mirror",54,16,"골렘의 거울","GOLEM MIRROR","4"));
            Things.Add(new AdventureThing("boss",57,16,"수호 골렘 · 조사","GUARDIAN GOLEM · INTERACT","golem"));
            var order = AdventureQuestions.PlateOrder(state.seed);
            for (int i = 0; i < 3; i++)
            {
                plateLabels.Add(Plate(15+i*3,8,order[i]));
                bossPlateLabels.Add(Plate(43+i*3,16,order[2-i]));
            }
            foreach (var t in Things)
            {
                float h = t.icon == "golem" ? 3 : 1.15f;
                t.sprite = Sprite(t.id,new Vector3(t.x,t.y+h/2),t.icon,new Vector2(h,h),Color.white,2);
                t.label = Text(t.sprite.transform,theme.T(t.ko,t.en),new Vector3(0,1.2f),.16f);
                if (t.id == "boss") bossLabel = t.label;
            }
            playerSprite = Sprite("Player",new Vector3(state.x,state.y+.85f),"player",new Vector2(.92f,1.65f),Color.white,10); player = playerSprite.transform;
            momo = Sprite("Momo",new Vector3(state.x-1.2f,state.y+.65f),"robot",new Vector2(.7f,.7f),Color.white,9).transform;
            carry = Sprite("CarriedCrate",new Vector3(state.x,state.y+2.1f),"5",new Vector2(.6f,.6f),Color.white,11).transform;
            laser = Beam("Laser"); bossLaser = Beam("BossLaser");
            Sprite("MirrorA",new Vector3(38,12),"4",Vector2.one*.7f,Color.white,4);
            Sprite("MirrorB",new Vector3(45,12),"4",Vector2.one*.7f,Color.white,4);
            Sprite("Receiver",new Vector3(45,9),"4",Vector2.one*.5f,theme.Gold,4);
            Sprite("BossMirror",new Vector3(54,20),"4",Vector2.one*.7f,Color.white,4);
            Sprite("BossReceiver",new Vector3(54,22),"4",Vector2.one*.5f,theme.Gold,4);
            lastX = state.x; Refresh(state, -1, new bool[3], false, true, true);
        }
        public void Refresh(AdventureState state, int held, bool[] weights, bool mirrorA, bool mirrorB, bool bossMirror)
        {
            gates["mined"].SetActive(!state.Has("mined")); gates["bridge"].SetActive(!state.Has("bridge"));
            gates["bridge-floor"].SetActive(state.Has("bridge")); gates["laser"].SetActive(!state.Has("laser"));
            gates["repaired"].SetActive(!state.Has("repaired")); gates["repaired-top"].SetActive(!state.Has("repaired"));
            carry.gameObject.SetActive(held >= 0);
            ladderViews[30].SetActive(state.Has("sequence")); ladderViews[18].SetActive(state.Has("sequence"));
            foreach (var t in Things)
            {
                if (t.id.StartsWith("box",StringComparison.Ordinal)) { int i = int.Parse(t.id.Substring(3)); t.sprite.gameObject.SetActive(held != i && !weights[i]); }
                t.label.gameObject.SetActive(t.sprite.gameObject.activeSelf);
                if (t.id.StartsWith("cache",StringComparison.Ordinal) || t.id == "high-cache") t.sprite.color = state.Has("reward/"+t.id) ? Color.gray : Color.white;
            }
            for (int i = 0; i < 15; i++) memoryTrail[i].enabled = state.visited[i] && AdventureRules.HasEnchant(state,4,0);
            bossLabel.text = theme.T("수호 골렘 · 보호막 ","GUARDIAN · SHIELDS ") + AdventureRules.Shields(state) + " / 3";
            if (state.Has("clear")) bossLabel.text = theme.T("훌륭하십니다! 미로를 지켜 주셨군요.","Well done! You restored the mine.");
            laser.positionCount = mirrorA ? 4 : 3;
            laser.SetPosition(0,new Vector3(38,9,-.2f)); laser.SetPosition(1,new Vector3(38,12,-.2f));
            laser.SetPosition(2,new Vector3(mirrorA?45:36.6f,12,-.2f));
            if (mirrorA) laser.SetPosition(3,new Vector3(45,mirrorB?9:14.6f,-.2f));
            bossLaser.positionCount=3; bossLaser.SetPosition(0,new Vector3(51,20,-.2f)); bossLaser.SetPosition(1,new Vector3(54,20,-.2f)); bossLaser.SetPosition(2,new Vector3(54,bossMirror?18:22,-.2f));
        }
        public void Animate(AdventureMotor motor, AdventureThing near, AdventureState state, float time, float dt)
        {
            float dx = motor.X-lastX; if (Math.Abs(dx) > .001f) playerSprite.flipX = dx < 0;
            player.position = new Vector3(motor.X, motor.Y+.85f+(Math.Abs(dx)>.001f && motor.Grounded ? Mathf.Sin(time*15)*.035f : 0),0);
            lastX = motor.X; momo.position = Vector3.Lerp(momo.position,new Vector3(motor.X-1.25f,motor.Y+.65f,0),dt*6);
            carry.position = new Vector3(motor.X,motor.Y+2.05f,0);
            foreach (var t in Things)
            {
                bool marked = (AdventureRules.HasTool(state,4) && Math.Abs(t.x-motor.X)<5 && Math.Abs(t.y-motor.Y)<3)
                    || (AdventureRules.HasEnchant(state,0,0) && t.id.StartsWith("cache",StringComparison.Ordinal))
                    || (AdventureRules.HasEnchant(state,1,1) && t.id=="repair")
                    || (AdventureRules.HasEnchant(state,2,1) && t.id=="high-cache");
                t.label.color = t == near ? theme.Green : marked ? theme.Gold : theme.Ink;
            }
        }
        public void PlateFeedback(bool boss, int index, bool correct)
        {
            var list = boss ? bossPlateLabels : plateLabels;
            foreach (var label in list) label.color = theme.Ink;
            if (index >= 0 && index < list.Count) list[index].color = correct ? theme.Green : theme.Gold;
        }
        private void Gate(string id,float x,float y,Color c) { gates[id] = Sprite(id,new Vector3(x,y+3.4f),"tile",new Vector2(.8f,6.8f),c,3).gameObject; }
        private void Ladder(float x,float bottom,float top)
        {
            int first = root.childCount;
            Quad("LadderRail",x-.4f,(top+bottom)/2,.1f,top-bottom,new Color(.6f,.5f,.3f),1);
            Quad("LadderRail",x+.4f,(top+bottom)/2,.1f,top-bottom,new Color(.6f,.5f,.3f),1);
            for(float y=bottom+.3f;y<top;y+=.6f) Quad("LadderRung",x,y,.8f,.12f,new Color(.7f,.6f,.38f),1);
            var group = new GameObject("Ladder_" + x); group.transform.SetParent(root,false);
            int count = root.childCount - first - 1;
            for(int i=0;i<count;i++) root.GetChild(first).SetParent(group.transform,true);
            ladderViews[(int)x] = group;
        }
        private TextMesh Plate(float x,float y,int n) { var r = Quad("Plate",x,y+.1f,1.1f,.2f,theme.Gold,2); return Text(r.transform,n.ToString(),new Vector3(0,1.1f),.3f); }
        private SpriteRenderer Quad(string name,float x,float y,float w,float h,Color c,int order) { return Sprite(name,new Vector3(x,y),"tile",new Vector2(w,h),c,order); }
        private SpriteRenderer Sprite(string name,Vector3 pos,string icon,Vector2 size,Color c,int order)
        {
            var go = new GameObject(name); go.transform.SetParent(root,false); go.transform.position=pos; go.transform.localScale=new Vector3(size.x,size.y,1);
            var r=go.AddComponent<SpriteRenderer>(); r.sprite=art.Get(icon);
            go.transform.localScale=new Vector3(size.x/r.sprite.bounds.size.x,size.y/r.sprite.bounds.size.y,1);
            r.color=c; r.sortingOrder=order; return r;
        }
        private TextMesh Text(Transform parent,string text,Vector3 offset,float size)
        {
            var go = new GameObject("WorldLabel"); go.transform.SetParent(root,false); go.transform.position=parent.position+offset;
            var t=go.AddComponent<TextMesh>(); t.font=theme.Font; t.fontSize=48; t.characterSize=size; t.anchor=TextAnchor.MiddleCenter; t.alignment=TextAlignment.Center; t.text=text; t.color=theme.Ink;
            go.GetComponent<MeshRenderer>().sharedMaterial=theme.Font.material; go.GetComponent<MeshRenderer>().sortingOrder=8;
            return t;
        }
        private LineRenderer Beam(string name)
        {
            var go=new GameObject(name); go.transform.SetParent(root,false); var l=go.AddComponent<LineRenderer>(); l.sharedMaterial=art.LineMaterial; l.startColor=l.endColor=new Color(.58f,1,.68f); l.startWidth=l.endWidth=.06f; l.sortingOrder=5; l.useWorldSpace=true; return l;
        }
    }
}
