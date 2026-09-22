using System;
using System.Collections.Generic;
using System.Globalization;
using MazeMath.Questions.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MazeMath.Adventure
{
    public sealed class AdventureHud : MonoBehaviour
    {
        private AdventureGame game;
        private AdventureTheme t;
        private AdventureArt art;
        private RectTransform safe, modal, body;
        private Text quest, floor, stats, tip, toast, boss;
        private Image xpFill;
        private readonly Image[] hearts = new Image[5];
        private readonly Image[] hotbar = new Image[6];
        private string kind;
        private float toastUntil;
        private int selectedTool, selectedMaterial=2;
        private int[] patternGrid={-1,-1,-1,-1,-1,-1,-1,-1,-1};
        private NumericInputBuffer input;
        private Text answerText, feedback;
        private int hintLevel;
        public bool Paused { get { return modal!=null && modal.gameObject.activeSelf; } }

        public void Initialize(AdventureGame game,AdventureTheme theme,AdventureArt art)
        {
            this.game=game; t=theme; this.art=art;
            var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=50;
            var scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safe=t.Rect(transform,"SafeArea",0,0,1,1); safe.gameObject.AddComponent<global::MazeMath.UI.SafeAreaFitter>();
            var card=t.Box(safe,"Objective",.018f,.82f,.40f,.977f);
            t.Label(card,"CHAPTER 01 / MOMO'S MINE",14,TextAnchor.UpperLeft,.05f,.68f,.95f,.91f).color=t.Green;
            quest=t.Label(card,"",23,TextAnchor.MiddleLeft,.05f,.10f,.95f,.69f);
            var floorBox=t.Box(safe,"Floor",.79f,.915f,.982f,.977f);
            floor=t.Label(floorBox,"1F",20,TextAnchor.MiddleCenter,.03f,.07f,.97f,.93f);
            t.Button(safe,t.T("지도","Map"),Map,.79f,.823f,.88f,.905f);
            t.Button(safe,"II",Pause,.89f,.823f,.982f,.905f);
            boss=t.Label(safe,"",22,TextAnchor.MiddleCenter,.42f,.91f,.76f,.98f); boss.color=t.Gold;
            var hot=t.Box(safe,"Hotbar",.305f,.145f,.695f,.26f);
            for(int i=0;i<6;i++)
            {
                int index=i; var b=t.Button(hot,"",()=>Hotbar(index),i/6f+.012f,.07f,(i+1)/6f-.012f,.93f);
                var icon=t.Rect(b.transform,"Icon",.20f,.22f,.80f,.84f); hotbar[i]=t.Fill(icon,Color.white); hotbar[i].sprite=art.Get(i.ToString()); hotbar[i].preserveAspect=true;
                t.Label(b.transform,(i+1).ToString(),11,TextAnchor.UpperLeft,.07f,.73f,.30f,.97f).color=t.Gold;
            }
            for(int i=0;i<5;i++)
            {
                var r=t.Rect(safe,"Heart",.309f+i*.032f,.280f,.335f+i*.032f,.326f); hearts[i]=t.Fill(r,Color.white); hearts[i].sprite=art.Get("heart"); hearts[i].preserveAspect=true;
            }
            var bar=t.Box(safe,"Experience",.305f,.264f,.695f,.28f);
            var fill=t.Rect(bar,"XP",.006f,.18f,.01f,.82f); xpFill=t.Fill(fill,t.Green);
            stats=t.Label(safe,"",16,TextAnchor.MiddleRight,.48f,.285f,.695f,.325f);
            tip=t.Label(safe,"",18,TextAnchor.MiddleCenter,.28f,.085f,.73f,.142f);
            toast=t.Label(safe,"",22,TextAnchor.MiddleCenter,.18f,.66f,.82f,.80f); toast.color=t.Gold;
            HoldButton("◀",0,.018f,.04f,.082f,.14f);
            HoldButton("▶",1,.091f,.04f,.155f,.14f);
            HoldButton("▲",2,.164f,.04f,.228f,.14f);
            HoldButton("▼",3,.237f,.04f,.301f,.14f);
            t.Button(safe,t.T("점프","Jump"),()=>game.Controls.Jump(),.768f,.04f,.86f,.14f);
            t.Button(safe,t.T("조사","Use"),()=>game.Controls.Interact(),.875f,.04f,.982f,.14f,true);
        }
        private void HoldButton(string text,int direction,float x0,float y0,float x1,float y1)
        {
            var b=t.Button(safe,text,()=>{},x0,y0,x1,y1);
            var hold=b.gameObject.AddComponent<AdventureHoldButton>(); hold.Bind(game.Controls,direction);
        }
        public void Refresh(AdventureState s,string nearby,bool guide)
        {
            if(s==null || quest==null) return;
            AdventureWorld.Objective(s,out var ox,out var oy,out var key);
            string objective=Objective(key);
            if(guide || AdventureRules.HasEnchant(s,4,1)) objective+="  "+(ox<s.x?"←":"→")+" "+((int)(oy/8)+1)+"F";
            quest.text=objective; floor.text=((int)(s.y/8)+1)+"F  ·  "+t.T("모모의 폐광","Momo's Mine");
            for(int i=0;i<5;i++) { hearts[i].color=i<s.health?Color.white:new Color(.2f,.2f,.2f,1); hotbar[i].color=s.owned[i]?s.equipped[i]?Color.white:t.Muted:new Color(.25f,.29f,.23f,1); }
            xpFill.rectTransform.anchorMax=new Vector2(.01f+.98f*((s.xp%50)/50f),.82f);
            stats.text="XP "+s.xp+"  |  "+t.Material(0)+" "+s.materials[0];
            tip.text=nearby==null?"":nearby+(nearby==t.T("탐험 중","Exploring")?"":" [E]");
            boss.text=s.y>=15 && s.x>=36 ? t.T("수호 골렘  ","GUARDIAN  ")+new string('■',AdventureRules.Shields(s))+new string('□',3-AdventureRules.Shields(s)) : "";
            if(Time.unscaledTime>toastUntil) toast.text="";
        }
        private string Objective(string key)
        {
            switch(key)
            {
                case "math": return t.T("숫자 장치를 찾아 재료를 얻으세요","Find the number terminal");
                case "craft": return t.T("아까 본 작업대에서 곡괭이 팔 제작","Return to the workbench; craft Mining Arm");
                case "mine": return t.T("곡괭이 팔로 균열벽을 여세요","Use Mining Arm at the cracked wall");
                case "bridge": return t.T("상자의 무게를 8로 맞추세요","Place 8kg on the scale");
                case "laser": return t.T("위층의 거울로 전원을 연결하세요","Climb and align the mirrors");
                case "sequence": return t.T("2 → 4 → 6 발판 순서를 찾으세요","Step on 2 → 4 → 6");
                case "boss.math": return t.T("골렘의 숫자 보호막 해제","Solve the guardian's number lock");
                case "boss.sequence": return t.T("골렘 앞 발판: 2 → 4 → 6","Guardian plates: 2 → 4 → 6");
                case "boss.laser": return t.T("골렘의 거울 보호막 해제","Align the guardian's mirror");
                case "finish": return t.T("수호 골렘에게 말을 걸어 주세요","Talk to the guardian");
                default: return t.T("챕터 1 완료! 모모와 탐험 성공","Chapter 1 complete! Great exploring!");
            }
        }
        public void Toast(string text) { if(toast!=null) { toast.text=text; toastUntil=Time.unscaledTime+5; } }
        private void Modal(string title,string type)
        {
            ForceClose(); kind=type; game.Controls.Clear();
            modal=t.Rect(safe,"ModalOverlay",0,0,1,1); t.Fill(modal,new Color(0,0,0,.68f),true);
            bool narrow=Screen.width<(Screen.height*1.2f);
            var panel=t.Box(modal,"Dialog",narrow?.025f:.20f,.12f,narrow?.975f:.80f,.88f);
            t.Label(panel,title,28,TextAnchor.MiddleLeft,.05f,.87f,.83f,.97f);
            if(type!="title") t.Button(panel,"X",Close,.87f,.87f,.97f,.97f);
            body=t.Rect(panel,"DialogBody",.045f,.04f,.955f,.84f);
        }
        public void ForceClose()
        {
            game.Controls.Clear();
            if(modal!=null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); }
            modal=body=null; kind=null; answerText=feedback=null; input=null;
        }
        public void Close() { if(kind=="title") return; if(!game.Running) { Title(); return; } game.Commit(); ForceClose(); }
        public void Title()
        {
            Modal(t.T("연준이의 숫자 로봇 탐험대","YEONJUN'S NUMBER ROBOT ADVENTURE"),"title");
            t.Label(body,t.T("직접 길을 찾고, 로봇 장비를 만들고, 수호 골렘을 도와주세요.","Explore the mine, craft robot tools, and help the guardian."),24,TextAnchor.MiddleCenter,.05f,.65f,.95f,.94f);
            var b=t.Button(body,t.T("이어서 탐험","Continue"),()=>game.Continue(),.18f,.46f,.82f,.60f,true); b.interactable=game.CanContinue;
            t.Button(body,t.T("새 탐험 시작","New adventure"),ConfirmNew,.18f,.27f,.82f,.41f);
            t.Label(body,t.T("PC: 방향키 / Space / E  ·  모바일: 화면 버튼\n작업대에서 장비를 만들고 아까 본 길로 돌아가 보세요.","PC: arrows / Space / E  ·  Touch: screen buttons\nCraft tools at the workbench and return to the locked path."),18,TextAnchor.MiddleCenter,.03f,.02f,.97f,.21f);
        }
        private void ConfirmNew()
        {
            if(!game.CanContinue) { game.BeginNew(); return; }
            Modal(t.T("새 탐험을 시작할까요?","Start a new adventure?"),"confirm");
            t.Label(body,t.T("현재 이어하기 진행이 새 탐험으로 바뀝니다.","Your current continue slot will be replaced."),23,TextAnchor.MiddleCenter,.05f,.5f,.95f,.9f);
            t.Button(body,t.T("취소","Cancel"),Title,.07f,.19f,.47f,.38f);
            t.Button(body,t.T("새로 시작","Start new"),()=>game.BeginNew(),.53f,.19f,.93f,.38f,true);
        }
        public void Pause()
        {
            if(!game.Running) return; game.Commit(); Modal(t.T("잠시 쉬어가기","Pause"),"pause");
            t.Button(body,t.T("계속 탐험","Resume"),Close,.15f,.81f,.85f,.96f,true);
            t.Button(body,t.T("길 도우미 켜기","Show route hint"),()=>{ game.ShowGuide=true; Close(); },.15f,.62f,.85f,.77f);
            t.Button(body,t.T("체크포인트로 돌아가기","Return to checkpoint"),()=>game.ReturnCheckpoint(),.15f,.43f,.85f,.58f);
            t.Button(body,t.T("현재 퍼즐 다시 시작","Reset local puzzle"),()=>{game.ResetLocalPuzzle();Close();},.15f,.24f,.85f,.39f);
            t.Button(body,t.T("저장하고 제목 화면","Save and title"),()=>game.Title(),.15f,.05f,.85f,.20f);
        }
        public void Map()
        {
            if(!game.Running) return; Modal(t.T("지나온 길","Explored map"),"map");
            int current=AdventureWorld.RoomAt(game.State.x,game.State.y);
            for(int f=0;f<3;f++) for(int r=0;r<5;r++)
            {
                int id=f*5+r; float x=.06f+r*.18f,y=.13f+f*.23f; var cell=t.Box(body,"MapRoom",x,y,x+.16f,y+.19f);
                t.Label(cell,game.State.visited[id]?id==current?"●":RoomName(id):"?",18,TextAnchor.MiddleCenter,.05f,.06f,.95f,.94f).color=id==current?t.Green:t.Ink;
            }
            t.Label(body,t.T("방문한 방만 표시합니다. 지도 버튼으로 이동할 수는 없어요.","Only explored rooms are shown. The map does not teleport you."),18,TextAnchor.MiddleCenter,.02f,.83f,.98f,.97f);
            t.Button(body,t.T("목표 방향 안내","Show objective direction"),()=>{game.ShowGuide=true;Close();},.20f,.0f,.80f,.10f);
        }
        private string RoomName(int id)
        {
            string[] ko={"입구","작업대","숫자","저울","사다리","보물","발판","허브","거울","상자","높은 보물","통로","쉼터","골렘","코어"};
            string[] en={"Start","Bench","Math","Scale","Ladder","Loot","Plates","Hub","Mirrors","Chest","High loot","Hall","Camp","Golem","Core"};
            return t.Korean?ko[id]:en[id];
        }
        private void Hotbar(int index) { if(!game.Running) return; selectedTool=Math.Min(index,4); Bag(); }
        public void Bag()
        {
            if(!game.Running) return; Modal(t.T("로봇 장비와 재료","Equipment and materials"),"bag");
            ResourceLine(.80f,.98f);
            for(int i=0;i<6;i++)
            {
                int k=i; float x=.035f+(i%3)*.325f,y=.44f-(i/3)*.30f;
                var b=t.Button(body,"",()=>{if(k<5){game.ToggleEquipment(k);Bag();}},x,y,x+.29f,y+.27f);
                var rect=t.Rect(b.transform,"ItemIcon",.31f,.34f,.69f,.92f); var icon=t.Fill(rect,Color.white);
                icon.sprite=art.Get(i<5?i.ToString():"robot"); icon.preserveAspect=true;
                string label=i<5?t.Tool(i)+"\n"+(game.State.owned[i]?game.State.equipped[i]?t.T("장착 중","Equipped"):t.T("미장착","Unequipped"):t.T("아직 없음","Not crafted")):"MOMO";
                t.Label(b.transform,label,14,TextAnchor.MiddleCenter,.03f,.02f,.97f,.35f);
                b.interactable=i<5 && game.State.owned[i];
                if(i<5 && !game.State.owned[i]) icon.color=t.Muted;
            }
            t.Label(body,t.T("장비를 눌러 장착/해제 · 제작과 인챈트는 작업대에서","Tap equipment to equip/unequip. Craft and enchant at the workbench."),17,TextAnchor.MiddleCenter,0,0,1,.12f);
        }
        public void Workshop() { if(!game.AtWorkshop) return; Modal(t.T("모모의 작업대","Momo's workbench"),"workshop"); WorkshopTabs(); ResourceLine(.70f,.85f);
            for(int i=0;i<5;i++)
            {
                int k=i; float y=.56f-i*.115f; var cost=AdventureRules.Cost(i); string line=t.Tool(i)+"\n";
                for(int m=0;m<5;m++) if(cost[m]>0) line+=t.Material(m)+" "+game.State.materials[m]+"/"+cost[m]+"  ";
                t.Label(body,line,18,TextAnchor.MiddleLeft,.025f,y,.72f,y+.105f);
                var b=t.Button(body,game.State.owned[i]?t.T("완료","Owned"):t.T("제작","Craft"),()=>{bool ok=game.Craft(k);Workshop();if(!ok)Toast(t.T("재료를 확인해 주세요. 첫 장비는 곡괭이 팔입니다.","Check materials. Craft Mining Arm first."));},.74f,y,.98f,y+.105f,true);
                b.interactable=!game.State.owned[i];
            }
        }
        private void WorkshopTabs()
        {
            t.Button(body,t.T("제작","Craft"),Workshop,0,.88f,.32f,1);
            t.Button(body,"3×3",()=>{selectedTool=4;Pattern();},.34f,.88f,.66f,1);
            t.Button(body,t.T("인챈트","Enchant"),EnchantMenu,.68f,.88f,1,1);
        }
        private void ResourceLine(float y0,float y1)
        {
            string text="XP "+game.State.xp+"   "; for(int i=0;i<5;i++) text+=t.Material(i)+" "+game.State.materials[i]+(game.State.pending[i]>0?" (+"+game.State.pending[i]+")":"")+"   ";
            t.Label(body,text,18,TextAnchor.MiddleLeft,.02f,y0,.98f,y1).color=t.Gold;
        }
        private void Pattern()
        {
            if(!game.AtWorkshop) return; if(selectedTool!=0 && selectedTool!=4) selectedTool=4;
            Modal(t.T("3×3 조합 제작","3×3 crafting"),"pattern"); WorkshopTabs();
            t.Button(body,t.Tool(4),()=>{selectedTool=4;ResetGrid();Pattern();},0,.73f,.48f,.85f);
            t.Button(body,t.Tool(0),()=>{selectedTool=0;ResetGrid();Pattern();},.52f,.73f,1,.85f);
            var recipe=AdventureRules.Pattern(selectedTool);
            t.Label(body,t.T("설계도","Blueprint"),17,TextAnchor.MiddleCenter,0,.63f,.34f,.72f);
            for(int r=0;r<3;r++) for(int c=0;c<3;c++)
            {
                int n=r*3+c; float x=c*.11f,y=.33f+(2-r)*.09f;
                var slot=t.Box(body,"RecipeCell",x,y,x+.105f,y+.085f); t.Label(slot,recipe[n]<0?"":t.Material(recipe[n]),12,TextAnchor.MiddleCenter,.02f,.02f,.98f,.98f);
                float gx=.40f+c*.195f,gy=.25f+(2-r)*.135f; int index=n;
                t.Button(body,patternGrid[n]<0?"-":t.Material(patternGrid[n]),()=>{patternGrid[index]=selectedMaterial;Pattern();},gx,gy,gx+.18f,gy+.125f);
            }
            for(int i=-1;i<4;i++)
            {
                int m=i; float x=(i+1)*.20f;
                t.Button(body,i<0?t.T("지우기","Clear"):t.Material(i),()=>{selectedMaterial=m;Pattern();},x,.10f,x+.19f,.21f,i==selectedMaterial);
            }
            t.Button(body,t.T("조합하기","Assemble"),()=>{bool ok=game.Craft(selectedTool,patternGrid);Toast(ok?t.T("조립 완료!","Assembled!"):t.T("배치와 재료를 확인해 주세요. 재료는 소모되지 않았어요.","Check pattern and materials. Nothing was consumed."));if(ok){ResetGrid();Workshop();}},.39f,0,.99f,.087f,true);
            t.Label(body,t.T("재료 선택 → 칸 터치","Select material → tap cell"),15,TextAnchor.MiddleCenter,0,0,.37f,.25f);
        }
        private void ResetGrid() { patternGrid=new[]{-1,-1,-1,-1,-1,-1,-1,-1,-1}; }
        private void EnchantMenu()
        {
            if(!game.AtWorkshop) return; Modal(t.T("장비 인챈트","Equipment enchantments"),"enchant"); WorkshopTabs(); ResourceLine(.69f,.84f);
            for(int i=0;i<5;i++) { int index=i; var b=t.Button(body,t.Tool(i),()=>{selectedTool=index;EnchantMenu();},i*.20f,.49f,(i+1)*.20f-.01f,.67f,i==selectedTool); b.interactable=game.State.owned[i]; }
            for(int v=0;v<2;v++)
            {
                int variant=v; float y=.23f-v*.20f;
                var b=t.Button(body,t.Effect(selectedTool,v)+"\n"+(game.State.unlockedEnchants[selectedTool*2+v]?t.T("무료로 변경","Switch free"):"10 XP"),()=>{if(!game.Enchant(selectedTool,variant))Toast(t.T("장비와 XP를 확인해 주세요.","Check owned equipment and XP."));EnchantMenu();},.02f,y,.98f,y+.18f,true);
                b.interactable=game.State.owned[selectedTool];
            }
        }
        public void Question()
        {
            Modal(t.T("숫자 장치","Number terminal"),"question"); input=new NumericInputBuffer(3); hintLevel=0;
            var q=game.State.question;
            t.Label(body,q.prompt,38,TextAnchor.MiddleCenter,.02f,.78f,.98f,.99f);
            feedback=t.Label(body,"",19,TextAnchor.MiddleCenter,.02f,.64f,.98f,.78f); feedback.color=t.Gold;
            if(q.multipleChoice)
            {
                for(int i=0;i<4;i++) { int value=q.choices[i]; float y=.45f-i*.13f; t.Button(body,value.ToString(),()=>Submit(value.ToString()),.20f,y,.80f,y+.12f); }
            }
            else
            {
                var box=t.Box(body,"NumberInput",.20f,.53f,.80f,.65f); answerText=t.Label(box,"",30,TextAnchor.MiddleCenter,.04f,.05f,.96f,.95f);
                for(int n=0;n<12;n++)
                {
                    int index=n; int row=n/3,col=n%3; string label=n<9?(n+1).ToString():n==9?"←":n==10?"0":t.T("확인","OK");
                    t.Button(body,label,()=>Key(index),.20f+col*.20f,.38f-row*.12f,.39f+col*.20f,.49f-row*.12f,n==11);
                }
            }
            t.Button(body,t.T("힌트","Hint"),Hint,.82f,.08f,.99f,.23f);
        }
        private void Key(int n)
        {
            if(input==null || game.State.question.solved) return;
            if(n<9)input.AppendDigit(n+1); else if(n==9)input.Backspace(); else if(n==10)input.AppendDigit(0); else Submit(input.Text);
            if(answerText!=null)answerText.text=input.Text;
        }
        private void Submit(string text)
        {
            if(game.State.question.solved) return;
            bool ok=game.Answer(text);
            feedback.text=ok?t.T("정답이에요! 문을 닫고 탐험을 계속하세요.","Correct! Close the panel and keep exploring."):t.T("한 번 더 생각해 볼까요? 힌트도 사용할 수 있어요.","Try again. Hints are always available.");
            if(ok)feedback.color=t.Green;
        }
        private void Hint()
        {
            hintLevel++;
            feedback.text=hintLevel<3?t.T("수를 작은 묶음으로 나누어 계산해 보세요. 10부터 만들어 봐요.","Break numbers into small groups. Try making a ten first."):t.T("함께 확인해요: ","Let's check together: ")+game.State.question.answer;
        }
        public void ReadKeyboard(Keyboard k)
        {
            if(k.escapeKey.wasPressedThisFrame) { Close(); return; }
            if(kind!="question" || input==null || game.State.question.multipleChoice) return;
            var keys=new[]{k.digit0Key,k.digit1Key,k.digit2Key,k.digit3Key,k.digit4Key,k.digit5Key,k.digit6Key,k.digit7Key,k.digit8Key,k.digit9Key};
            var pads=new[]{k.numpad0Key,k.numpad1Key,k.numpad2Key,k.numpad3Key,k.numpad4Key,k.numpad5Key,k.numpad6Key,k.numpad7Key,k.numpad8Key,k.numpad9Key};
            for(int i=0;i<10;i++)if(keys[i].wasPressedThisFrame||pads[i].wasPressedThisFrame)input.AppendDigit(i);
            if(k.backspaceKey.wasPressedThisFrame)input.Backspace();
            if(k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame)Submit(input.Text);
            if(answerText!=null)answerText.text=input.Text;
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        public void ReadLegacyKeyboard()
        {
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            if(kind!="question" || input==null || game.State.question.multipleChoice) return;
            foreach(char c in global::UnityEngine.Input.inputString) if(c>='0' && c<='9') input.AppendDigit(c-'0');
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Backspace)) input.Backspace();
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Return)||global::UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter)) Submit(input.Text);
            if(answerText!=null)answerText.text=input.Text;
        }
#endif
        public void ClearScreen()
        {
            Modal(t.T("챕터 1 완료!","CHAPTER 1 COMPLETE!"),"clear");
            t.Label(body,t.T("모모의 계산 코어를 되찾았어요.\n길을 기억하고, 장비를 만들고, 골렘을 도왔습니다!","You recovered Momo's calculation core.\nYou remembered paths, crafted tools and helped the guardian!"),26,TextAnchor.MiddleCenter,.05f,.4f,.95f,.95f);
            t.Button(body,t.T("남은 보물 탐험","Explore remaining treasures"),Close,.1f,.18f,.9f,.36f,true);
        }
    }
}
