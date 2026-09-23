using System;
using MazeMath.Questions.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MazeMath.Adventure
{
    [DefaultExecutionOrder(500)]
    public sealed partial class AdventureHud : MonoBehaviour
    {
        private AdventureGame game;
        private AdventureTheme t;
        private AdventureArt art;
        private Canvas canvas;
        private RectTransform safe, header, dock, modal, panel, body;
        private RectTransform topRail, bottomRail, leftRail, rightRail;
        private Text quest, floor, stats, tip, toast, boss, eyebrow;
        private Image xpFill;
        private readonly Image[] hearts=new Image[5];
        private readonly Image[] hotbar=new Image[6];
        private readonly RectTransform[] hotSlots=new RectTransform[6];
        private readonly RectTransform[] moves=new RectTransform[4];
        private RectTransform jumpButton, useButton, portrait, mapButton, pauseButton, xpTrack;
        private readonly QuestionFeedbackFlow questionFlow=new QuestionFeedbackFlow();
        private AdventurePresentation presentation;
        private string kind;
        private float toastUntil;
        private bool showTouch;
        private Vector2 lastSize;
        private Rect lastSafe;
        private AdventureHudLayout layout;
        private NumericInputBuffer input;
        private Text answerText, feedback;
        private int hintLevel;
        public bool Paused => modal!=null && modal.gameObject.activeSelf;
        public bool QuestionFeedbackPending => kind=="question" && questionFlow.IsPending;
        public bool IsQuestionOpen => kind=="question" && Paused;
        public Rect PlayfieldViewport { get; private set; }

        public void Initialize(AdventureGame owner,AdventureTheme theme,AdventureArt artwork)
        {
            game=owner; t=theme; art=artwork;
            canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=50;
            var scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            topRail=Rail("HeaderRail"); bottomRail=Rail("DockRail"); leftRail=Rail("LeftSafeMargin"); rightRail=Rail("RightSafeMargin");
            safe=t.Rect(transform,"SafeArea",0,0,1,1); safe.gameObject.AddComponent<global::MazeMath.UI.SafeAreaFitter>();
            header=t.Rect(safe,"CompactHeader",0,0,1,1); dock=t.Rect(safe,"ControlDock",0,0,1,1);
            portrait=t.Rect(header,"MomoPortrait",0,0,1,1); var p=t.Fill(portrait,Color.white); p.sprite=art.Get("robot"); p.preserveAspect=true;
            eyebrow=t.Label(header,"MOMO / CHAPTER 01",11,TextAnchor.MiddleLeft,0,0,1,1); eyebrow.color=t.Muted;
            quest=t.Label(header,"",17,TextAnchor.MiddleLeft,0,0,1,1);
            toast=t.Label(header,"",16,TextAnchor.MiddleLeft,0,0,1,1); toast.color=t.Green;
            floor=t.Label(header,"1F",14,TextAnchor.MiddleCenter,0,0,1,1); floor.color=t.Muted;
            mapButton=t.Button(header,t.T("지도","Map"),Map,0,0,1,1).GetComponent<RectTransform>();
            pauseButton=t.Button(header,"II",Pause,0,0,1,1).GetComponent<RectTransform>();
            boss=t.Label(header,"",12,TextAnchor.MiddleLeft,0,0,1,1); boss.color=t.Gold;
            for(int i=0;i<6;i++)
            {
                int index=i; var b=t.Button(dock,"",()=>Hotbar(index),0,0,1,1);
                hotSlots[i]=b.GetComponent<RectTransform>();
                var icon=t.Rect(b.transform,"ToolIcon",.16f,.16f,.84f,.84f); hotbar[i]=t.Fill(icon,Color.white); hotbar[i].sprite=art.Get(i.ToString()); hotbar[i].preserveAspect=true;
            }
            for(int i=0;i<5;i++)
            {
                var r=t.Rect(dock,"Heart",0,0,1,1); hearts[i]=t.Fill(r,Color.white); hearts[i].sprite=art.Get("heart"); hearts[i].preserveAspect=true;
            }
            xpTrack=t.Rect(dock,"Experience",0,0,1,1); t.Fill(xpTrack,t.Edge);
            xpFill=t.Fill(t.Rect(xpTrack,"XP",0,0,0,1),t.Green);
            stats=t.Label(dock,"",12,TextAnchor.MiddleRight,0,0,1,1); stats.color=t.Muted;
            tip=t.Label(dock,"",14,TextAnchor.MiddleCenter,0,0,1,1); tip.color=t.Ink;
            string[] arrows={"<",">","^","v"};
            for(int i=0;i<4;i++)
            {
                var b=t.Button(dock,arrows[i],()=>{},0,0,1,1);
                b.gameObject.AddComponent<AdventureHoldButton>().Bind(game.Controls,i); moves[i]=b.GetComponent<RectTransform>();
            }
            jumpButton=t.Button(dock,t.T("점프","Jump"),()=>game.Controls.Jump(),0,0,1,1).GetComponent<RectTransform>();
            useButton=t.Button(dock,t.T("조사","Use"),()=>game.Controls.Interact(),0,0,1,1,true).GetComponent<RectTransform>();
            showTouch=Application.isMobilePlatform || Touchscreen.current!=null;
            presentation=new AdventurePresentation(game,theme,artwork);
            Canvas.ForceUpdateCanvases(); Reflow();
        }
        private RectTransform Rail(string name) { var r=t.Rect(transform,name,0,0,1,1); t.Fill(r,t.Rail); return r; }
        private static void Place(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=Vector2.zero; r.pivot=Vector2.zero; r.anchoredPosition=new Vector2(x,y); r.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h));
        }
        private static void Anchors(RectTransform r,float x0,float y0,float x1,float y1)
        {
            r.anchorMin=new Vector2(x0,y0); r.anchorMax=new Vector2(x1,y1); r.offsetMin=r.offsetMax=Vector2.zero;
        }
        public void SetTouchControls(bool visible) { showTouch=visible; game.Controls.Clear(); Reflow(); }
        private void Reflow()
        {
            if(safe==null) return;
            float w=safe.rect.width,h=safe.rect.height; if(w<1 || h<1) return;
            layout=new AdventureHudLayout(w,h,showTouch); lastSize=new Vector2(w,h); lastSafe=Screen.safeArea;
            float head=(float)layout.HeaderHeight, d=(float)layout.DockHeight, slot=(float)layout.SlotSize;
            Place(header,0,h-head,w,head); Place(dock,0,0,w,d);
            bool small=layout.Compact;
            float avatar=small?0:42, start=small?14:68, right=small?134:156;
            portrait.gameObject.SetActive(!small); Place(portrait,14,(head-42)/2,avatar,42);
            Place(eyebrow.rectTransform,start,head-22,w-start-right,14);
            Place(boss.rectTransform,start,head-22,w-start-right,14);
            Place(quest.rectTransform,start,7,w-start-right,Mathf.Max(24,head-29));
            Place(toast.rectTransform,start,7,w-start-right,Mathf.Max(24,head-29));
            float bsize=Mathf.Min(40,head-18), by=(head-bsize)/2;
            Place(floor.rectTransform,w-right+2,by,small?32:46,bsize);
            Place(mapButton,w-94,by,40,bsize); Place(pauseButton,w-46,by,34,bsize);
            bool stacked=small && showTouch;
            float gap=4, hotW=6*slot+5*gap;
            float hotY=stacked?d*.47f:10;
            for(int i=0;i<6;i++) Place(hotSlots[i],(w-hotW)/2+i*(slot+gap),hotY,slot,slot);
            for(int i=0;i<4;i++) moves[i].gameObject.SetActive(showTouch);
            jumpButton.gameObject.SetActive(showTouch); useButton.gameObject.SetActive(showTouch);
            float control=stacked?Mathf.Min(42,(w-160)/4):44;
            for(int i=0;i<4;i++) Place(moves[i],12+i*(control+4),10,control,Mathf.Min(48,d*.39f));
            Place(jumpButton,w-132,10,56,Mathf.Min(48,d*.39f)); Place(useButton,w-68,10,56,Mathf.Min(48,d*.39f));
            float infoY=showTouch?d-21:24;
            for(int i=0;i<5;i++) Place(hearts[i].rectTransform,14+i*18,infoY,15,15);
            Place(xpTrack,14,infoY-7,88,3);
            Place(stats.rectTransform,w-130,infoY-2,116,18);
            Place(tip.rectTransform,stacked?108:showTouch?116:135,d-24,w-(stacked?240:showTouch?250:270),20);
            if(stacked) { tip.gameObject.SetActive(false); } else { tip.gameObject.SetActive(true); }
            // Rendering is cropped, not merely covered. No world object can be hidden behind these controls.
            Rect sr=Screen.safeArea; if(sr.width<1 || sr.height<1) sr=new Rect(0,0,Screen.width,Screen.height);
            float sw=Mathf.Max(1,Screen.width),sh=Mathf.Max(1,Screen.height);
            PlayfieldViewport=new Rect(sr.x/sw,(sr.y+sr.height*(float)layout.WorldBottom)/sh,sr.width/sw,sr.height*(float)(layout.WorldTop-layout.WorldBottom)/sh);
            Anchors(topRail,0,PlayfieldViewport.yMax,1,1); Anchors(bottomRail,0,0,1,PlayfieldViewport.yMin);
            Anchors(leftRail,0,PlayfieldViewport.yMin,PlayfieldViewport.xMin,PlayfieldViewport.yMax);
            Anchors(rightRail,PlayfieldViewport.xMax,PlayfieldViewport.yMin,1,PlayfieldViewport.yMax);
            presentation.SetViewport(PlayfieldViewport);
            ResizeDialog();
        }
        private void ResizeDialog()
        {
            if(panel==null || safe==null) return;
            panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f); panel.pivot=new Vector2(.5f,.5f); panel.anchoredPosition=Vector2.zero;
            panel.sizeDelta=new Vector2(Mathf.Min(kind=="question"?560:680,safe.rect.width-24),Mathf.Min(540,safe.rect.height-24));
        }
        private void LateUpdate()
        {
            if(game==null || safe==null) return;
            if(lastSize!=safe.rect.size || lastSafe!=Screen.safeArea) Reflow();
            presentation.Update();
            if(kind=="question" && questionFlow.ShouldClose(Time.unscaledTime))
            {
                // The answer was already committed by AdventureGame.Answer. Do not grant or submit twice.
                ForceClose(); Toast(t.T("정답! 다시 모험을 떠나요.","Great job! Back to exploring."));
            }
        }
        private void OnDestroy() { questionFlow.Cancel(); presentation?.Dispose(); }
        public void Refresh(AdventureState s,string nearby,bool guide)
        {
            if(s==null || quest==null) return;
            AdventureWorld.Objective(s,out var ox,out var oy,out var key);
            string objective=Objective(key);
            if(guide || AdventureRules.HasEnchant(s,4,1)) objective+="  "+(ox<s.x?"←":"→")+" "+((int)(oy/8)+1)+"F";
            bool notifying=Time.unscaledTime<toastUntil;
            quest.text=objective; quest.gameObject.SetActive(!notifying); toast.gameObject.SetActive(notifying);
            floor.text=((int)(s.y/8)+1)+"F";
            for(int i=0;i<5;i++)
            {
                hearts[i].color=i<s.health?Color.white:new Color(.65f,.70f,.72f,.45f);
                hotbar[i].color=s.owned[i]?(s.equipped[i]?Color.white:new Color(1,1,1,.55f)):new Color(.65f,.72f,.75f,.3f);
                hotSlots[i].GetComponent<Image>().color=s.owned[i] && s.equipped[i]?t.Mint:t.Sky;
            }
            xpFill.rectTransform.anchorMax=new Vector2((s.xp%50)/50f,1);
            stats.text="XP "+s.xp;
            tip.text=nearby==null || nearby==t.T("탐험 중","Exploring")?(showTouch?"":t.T("방향키 이동 · Space 점프 · E 조사","Arrows move · Space jump · E use")):nearby+(showTouch?"":" [E]");
            bool arena=s.y>=15 && s.x>=36;
            boss.gameObject.SetActive(arena); eyebrow.gameObject.SetActive(!arena);
            boss.text=arena?t.T("수호 골렘 · 보호막 ","Guardian · shields ")+AdventureRules.Shields(s)+" / 3":"";
        }
        private string Objective(string key)
        {
            switch(key)
            {
                case "math": return t.T("숫자 장치를 찾아볼까요?","Find the number terminal");
                case "craft": return t.T("작업대로 돌아가 곡괭이 팔 만들기","Return to the workbench; craft a mining arm");
                case "mine": return t.T("곡괭이 팔로 균열벽 열기","Open the cracked wall");
                case "bridge": return t.T("저울의 무게를 8로 맞추기","Place 8kg on the scale");
                case "laser": return t.T("위층 거울로 전원 연결하기","Climb and align the mirrors");
                case "sequence": return t.T("발판을 2 → 4 → 6 순서로 밟기","Step on 2 → 4 → 6");
                case "boss.math": return t.T("골렘의 숫자 보호막 풀기","Solve the guardian's number lock");
                case "boss.sequence": return t.T("골렘 앞 발판: 2 → 4 → 6","Guardian plates: 2 → 4 → 6");
                case "boss.laser": return t.T("골렘의 거울 보호막 풀기","Align the guardian's mirror");
                case "finish": return t.T("수호 골렘에게 말 걸기","Talk to the guardian");
                default: return t.T("모모와 함께 탐험 성공!","You and Momo did it!");
            }
        }
        public void Toast(string text) { if(toast!=null) { toast.text=text; toastUntil=Time.unscaledTime+3; } }
        private void Modal(string title,string type)
        {
            ForceClose(); kind=type; game.Controls.Clear();
            modal=t.Rect(safe,"ModalOverlay",0,0,1,1); t.Fill(modal,new Color(.12f,.20f,.25f,.45f),true);
            panel=t.Box(modal,"Dialog",0,0,1,1); ResizeDialog();
            t.Label(panel,title,22,TextAnchor.MiddleLeft,.05f,.88f,.85f,.98f);
            if(type!="title") t.Button(panel,"×",Close,.88f,.89f,.97f,.98f);
            body=t.Rect(panel,"DialogBody",.045f,.035f,.955f,.85f);
        }
        public void ForceClose()
        {
            questionFlow.Cancel(); if(game!=null) game.Controls.Clear();
            if(EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(null);
            if(modal!=null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); }
            modal=panel=body=null; kind=null; answerText=feedback=null; input=null;
        }
        public void Close() { if(kind=="title") return; if(!game.Running) { Title(); return; } game.Commit(); ForceClose(); }
        public void Question()
        {
            if(game.State==null || game.State.question==null) return;
            Modal(t.T("숫자 장치","Number terminal"),"question"); input=new NumericInputBuffer(3); hintLevel=0; questionFlow.Begin();
            var q=game.State.question;
            var mascot=t.Rect(body,"MomoHelper",.02f,.82f,.16f,.98f); var mi=t.Fill(mascot,Color.white); mi.sprite=art.Get("robot"); mi.preserveAspect=true;
            t.Label(body,q.prompt,28,TextAnchor.MiddleCenter,.18f,.82f,.98f,.99f);
            feedback=t.Label(body,"",16,TextAnchor.MiddleCenter,.02f,.68f,.98f,.81f);
            if(q.multipleChoice)
            {
                for(int i=0;i<q.choices.Length;i++)
                {
                    int answer=q.choices[i]; float y=.48f-i*.145f;
                    var button=t.Button(body,answer.ToString(),()=>SubmitAnswer(answer.ToString()),.18f,y,.82f,y+.125f);
                    button.GetComponentInChildren<Text>().fontSize=22;
                }
            }
            else
            {
                var box=t.Box(body,"NumberInput",.21f,.54f,.79f,.67f); box.GetComponent<Image>().color=t.Sky;
                answerText=t.Label(box,"",26,TextAnchor.MiddleCenter,.04f,.05f,.96f,.95f);
                for(int n=0;n<12;n++)
                {
                    int index=n,row=n/3,col=n%3;
                    string label=n<9?(n+1).ToString():n==9?"←":n==10?"0":t.T("확인","OK");
                    var b=t.Button(body,label,()=>Key(index),.21f+col*.20f,.39f-row*.125f,.39f+col*.20f,.50f-row*.125f,n==11);
                    var text=b.GetComponentInChildren<Text>(); text.fontSize=n==11?16:22; text.resizeTextMaxSize=text.fontSize;
                }
            }
            t.Button(body,t.T("힌트","Hint"),Hint,.83f,.06f,.99f,.17f);
            if(q.solved) { questionFlow.RecordAnswer(true,Time.unscaledTime); feedback.text=t.T("이미 해결했어요!","Already solved!"); LockQuestionButtons(); }
        }
        private void Key(int n)
        {
            if(input==null || !questionFlow.CanSubmit) return;
            if(n<9)input.AppendDigit(n+1); else if(n==9)input.Backspace(); else if(n==10)input.AppendDigit(0); else SubmitAnswer(input.Text);
            if(answerText!=null && input!=null)answerText.text=input.Text;
        }
        public void SubmitAnswer(string text)
        {
            if(kind!="question" || !questionFlow.CanSubmit || game.State.question==null || game.State.question.solved) return;
            bool ok=game.Answer(text);
            feedback.text=ok?t.T("정답이에요! 참 잘했어요.","Correct! Nicely done."):t.T("괜찮아요. 한 번 더 생각해 볼까요?","That's okay. Let's try again.");
            feedback.color=ok?t.Green:t.Gold;
            if(questionFlow.RecordAnswer(ok,Time.unscaledTime)) LockQuestionButtons();
        }
        private void LockQuestionButtons() { foreach(var b in body.GetComponentsInChildren<Button>()) b.interactable=false; }
        private void Hint()
        {
            if(!questionFlow.CanSubmit) return; hintLevel++;
            feedback.text=hintLevel<3?t.T("작은 묶음으로 나눠 보세요. 10부터 만들어 봐요.","Try small groups. Make a ten first."):t.T("함께 확인해요: ","Let's check: ")+game.State.question.answer;
        }
        public void ReadKeyboard(Keyboard k)
        {
            if(k==null) return;
            if(k.escapeKey.wasPressedThisFrame) { Close(); return; }
            if(kind!="question" || input==null || !questionFlow.CanSubmit || game.State.question.multipleChoice) return;
            var keys=new[]{k.digit0Key,k.digit1Key,k.digit2Key,k.digit3Key,k.digit4Key,k.digit5Key,k.digit6Key,k.digit7Key,k.digit8Key,k.digit9Key};
            var pads=new[]{k.numpad0Key,k.numpad1Key,k.numpad2Key,k.numpad3Key,k.numpad4Key,k.numpad5Key,k.numpad6Key,k.numpad7Key,k.numpad8Key,k.numpad9Key};
            for(int i=0;i<10;i++)if(keys[i].wasPressedThisFrame||pads[i].wasPressedThisFrame)input.AppendDigit(i);
            if(k.backspaceKey.wasPressedThisFrame)input.Backspace();
            if(k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame)SubmitAnswer(input.Text);
            if(answerText!=null && input!=null)answerText.text=input.Text;
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        public void ReadLegacyKeyboard()
        {
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            if(kind!="question" || input==null || !questionFlow.CanSubmit || game.State.question.multipleChoice) return;
            foreach(char c in global::UnityEngine.Input.inputString)if(c>='0' && c<='9')input.AppendDigit(c-'0');
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Backspace))input.Backspace();
            if(global::UnityEngine.Input.GetKeyDown(KeyCode.Return)||global::UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))SubmitAnswer(input.Text);
            if(answerText!=null && input!=null)answerText.text=input.Text;
        }
#endif
    }
}
