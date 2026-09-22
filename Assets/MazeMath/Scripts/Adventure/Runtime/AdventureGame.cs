using System;
using System.Collections.Generic;
using MazeMath.Puzzles.Types;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace MazeMath.Adventure
{
    public sealed class AdventureGame : MonoBehaviour
    {
        [SerializeField] private Font koreanFont;
        public AdventureState State { get; private set; }
        public global::MazeMath.Core.Save.ISaveStore Persistence { get; set; }
        public AdventureControls Controls { get; } = new AdventureControls();
        public bool Running { get; private set; }
        public bool ShowGuide { get; set; }
        public AdventureTheme Theme { get; private set; }
        public AdventureMotor Motor { get; private set; }
        public AdventureHud Hud { get; private set; }
        public string NearestName { get; private set; }
        public bool CanContinue { get; private set; }
        public bool BackupRecovered { get; private set; }
        private AdventureSave save;
        private AdventureArt art;
        private AdventureWorldView world;
        private Transform worldRoot;
        private Camera gameCamera;
        private AdventureState resume;
        private AdventureThing nearest;
        private WeightBridgePuzzle weight;
        private LaserMirrorPuzzle laser, bossLaser;
        private SequencePlatePuzzle sequence, bossSequence;
        private bool[] weights = new bool[3];
        private int carried = -1;
        private bool mirrorA, mirrorB, bossMirror;
        private string lastPlate;
        private int repairAttempts;
        private bool suspended;
        private float lastSave, noProgress, bossClock;
        private int lastRoom = -1;
        private readonly int[] kg = { 3, 5, 2 };

        private void Start()
        {
            Theme = new AdventureTheme(koreanFont); art = new AdventureArt(); save = new AdventureSave(Persistence);
            CanContinue = save.TryLoad(out resume, out var backup); BackupRecovered = backup;
            gameCamera = Camera.main;
            if (gameCamera == null) { var c = new GameObject("AdventureCamera",typeof(Camera)); c.transform.SetParent(transform,false); c.tag="MainCamera"; gameCamera=c.GetComponent<Camera>(); }
            gameCamera.orthographic=true; gameCamera.orthographicSize=5.8f; gameCamera.clearFlags=CameraClearFlags.SolidColor; gameCamera.backgroundColor=new Color(.12f,.20f,.13f);
            if (EventSystem.current == null)
            {
                var e = new GameObject("AdventureEventSystem",typeof(EventSystem)); e.transform.SetParent(transform,false);
#if ENABLE_INPUT_SYSTEM
                e.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
                e.AddComponent<StandaloneInputModule>();
#endif
            }
            Hud = new GameObject("BlockHUD",typeof(RectTransform)).AddComponent<AdventureHud>(); Hud.transform.SetParent(transform,false); Hud.Initialize(this,Theme,art);
            Install(AdventureState.NewRun(100)); Running=false; Hud.Title();
        }
        public void BeginNew()
        {
            int seed = MazeMath.Maze.Generation.MazeSeedService.StableHash(Guid.NewGuid().ToString("N"));
            Install(AdventureState.NewRun(seed)); Running=true; Hud.ForceClose(); Commit();
            Hud.Toast(Theme.T("모모: 오른쪽 숫자 장치부터 살펴봐요!","Momo: explore the number terminal to the right!"));
        }
        public void Continue()
        {
            if (!save.TryLoad(out var loaded,out var backup)) { Hud.Toast(Theme.T("저장 파일을 복원할 수 없습니다. 원본은 유지합니다.","Save could not be restored. Original data was kept.")); return; }
            Install(loaded); Running=true; Hud.ForceClose();
            if (backup) Hud.Toast(Theme.T("백업 체크포인트를 복구했습니다.","Recovered the backup checkpoint."));
        }
        private void Install(AdventureState state)
        {
            Controls.Clear(); nearest=null; ShowGuide=false; State=state; Motor=new AdventureMotor(state.x,state.y); noProgress=0; bossClock=0; lastRoom=-1;
            weights=new bool[3]; carried=-1; repairAttempts=0; lastPlate=null;
            weight=new WeightBridgePuzzle("bridge",8); weight.StartPuzzle();
            sequence=new SequencePlatePuzzle("sequence",new[]{2,4,6}); sequence.StartPuzzle();
            bossSequence=new SequencePlatePuzzle("boss.sequence",new[]{2,4,6}); bossSequence.StartPuzzle();
            mirrorA=state.Has("laser"); mirrorB=state.Has("laser"); bossMirror=!state.Has("boss.laser");
            laser=new LaserMirrorPuzzle("laser",12,7,new GridPosition(2,1),GridDirection.Up,new GridPosition(9,1),null,
                new Dictionary<GridPosition,MirrorOrientation>{ {new GridPosition(2,4),mirrorA?MirrorOrientation.Slash:MirrorOrientation.Backslash}, {new GridPosition(9,4),mirrorB?MirrorOrientation.Backslash:MirrorOrientation.Slash} }); laser.StartPuzzle();
            bossLaser=new LaserMirrorPuzzle("boss.laser",7,7,new GridPosition(0,4),GridDirection.Right,new GridPosition(3,6),null,
                new Dictionary<GridPosition,MirrorOrientation>{ {new GridPosition(3,4),bossMirror?MirrorOrientation.Backslash:MirrorOrientation.Slash} }); bossLaser.StartPuzzle();
            if (worldRoot != null) { worldRoot.gameObject.SetActive(false); Destroy(worldRoot.gameObject); }
            worldRoot=new GameObject("MineWorld").transform; worldRoot.SetParent(transform,false);
            world=new AdventureWorldView(worldRoot,Theme,art); world.Build(state); RefreshWorld();
            gameCamera.transform.position=new Vector3(Mathf.Clamp(state.x,7,53),state.y+3.5f,-10);
        }
        private void Update()
        {
            if (Hud == null) return;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && !suspended)
            {
                var k=Keyboard.current;
                if (Hud.Paused) Hud.ReadKeyboard(k);
                else if (Running)
                {
                    if (k.spaceKey.wasPressedThisFrame) Controls.Jump();
                    if (k.eKey.wasPressedThisFrame) Controls.Interact();
                    if (k.mKey.wasPressedThisFrame) Hud.Map();
                    if (k.bKey.wasPressedThisFrame) Hud.Bag();
                    if (k.escapeKey.wasPressedThisFrame) Hud.Pause();
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (!suspended)
            {
                if (Hud.Paused) Hud.ReadLegacyKeyboard();
                else if (Running)
                {
                    if (global::UnityEngine.Input.GetKeyDown(KeyCode.Space)) Controls.Jump();
                    if (global::UnityEngine.Input.GetKeyDown(KeyCode.E)) Controls.Interact();
                    if (global::UnityEngine.Input.GetKeyDown(KeyCode.M)) Hud.Map();
                    if (global::UnityEngine.Input.GetKeyDown(KeyCode.B)) Hud.Bag();
                    if (global::UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Hud.Pause();
                }
            }
#endif
            if (!Running || Hud.Paused || suspended) { Controls.ConsumeInteract(); return; }
            if (Controls.ConsumeInteract()) Interact();
            noProgress+=Time.unscaledDeltaTime;
            if (noProgress>90) { noProgress=0; ShowGuide=true; Hud.Toast(Theme.T("모모: 지도와 목표 방향을 함께 살펴볼까요?","Momo: try the map and objective direction.")); }
            if (Time.unscaledTime-lastSave>10) Commit();
            TickBoss();
        }
        private void FixedUpdate()
        {
            if (!Running || Hud == null || Hud.Paused || suspended) return;
            float x=Controls.Horizontal,y=Controls.Vertical;
#if ENABLE_INPUT_SYSTEM
            var k=Keyboard.current;
            if (k != null)
            {
                x+=((k.dKey.isPressed||k.rightArrowKey.isPressed)?1:0)-((k.aKey.isPressed||k.leftArrowKey.isPressed)?1:0);
                y+=((k.wKey.isPressed||k.upArrowKey.isPressed)?1:0)-((k.sKey.isPressed||k.downArrowKey.isPressed)?1:0);
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            x+=((global::UnityEngine.Input.GetKey(KeyCode.D)||global::UnityEngine.Input.GetKey(KeyCode.RightArrow))?1:0)-((global::UnityEngine.Input.GetKey(KeyCode.A)||global::UnityEngine.Input.GetKey(KeyCode.LeftArrow))?1:0);
            y+=((global::UnityEngine.Input.GetKey(KeyCode.W)||global::UnityEngine.Input.GetKey(KeyCode.UpArrow))?1:0)-((global::UnityEngine.Input.GetKey(KeyCode.S)||global::UnityEngine.Input.GetKey(KeyCode.DownArrow))?1:0);
#endif
            Motor.Tick(x,y,Controls.ConsumeJump(),Mathf.Min(Time.fixedDeltaTime,.05f),State);
            State.x=Motor.X; State.y=Motor.Y;
            int room=AdventureWorld.RoomAt(State.x,State.y);
            if (room!=lastRoom)
            {
                if (!State.visited[room]) { State.visited[room]=true; noProgress=0; }
                lastRoom=room; RefreshWorld();
            }
            nearest=null; float distance=1.7f;
            foreach (var t in world.Things)
            {
                if (!t.sprite.gameObject.activeSelf || Mathf.Abs(t.y-Motor.Y)>1.8f) continue;
                float d=Mathf.Abs(t.x-Motor.X);
                if (d<distance) { distance=d; nearest=t; }
            }
            NearestName=nearest==null?Theme.T("탐험 중","Exploring"):Theme.T(nearest.ko,nearest.en);
            CheckPlates();
            if (Motor.Recovered) { Commit(); Hud.Toast(Theme.T("체크포인트로 돌아왔어요. 장비는 그대로입니다.","Back at the checkpoint. Your equipment is safe.")); }
        }
        private void LateUpdate()
        {
            if (world == null || Motor == null) return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            world.Animate(Motor,nearest,State,Time.unscaledTime,dt);
            float half=gameCamera.orthographicSize*gameCamera.aspect;
            var target=new Vector3(Mathf.Clamp(Motor.X,Mathf.Min(half,30),Mathf.Max(60-half,30)),Motor.Y+3.5f,-10);
            gameCamera.transform.position=Vector3.Lerp(gameCamera.transform.position,target,1-Mathf.Exp(-dt*7));
            Hud.Refresh(State,NearestName,ShowGuide);
        }
        public void Interact()
        {
            if (!Running || Hud.Paused || nearest==null) return;
            string id=nearest.id;
            switch(id)
            {
                case "entrance": Hud.Toast(Theme.T("좌우 이동 · 점프 · 사다리 위/아래 · 조사 E","Move · Jump · Climb up/down · Interact E")); break;
                case "workshop": Hud.Workshop(); break;
                case "math": case "boss.math": AdventureQuestions.Ensure(State,id); Commit(); Hud.Question(); break;
                case "mine": if (AdventureRules.OpenMiningGate(State)) Changed(Theme.T("균열벽을 열었습니다!","The cracked wall is open!")); else Hud.Toast(Theme.T("작업대에서 곡괭이 팔을 제작하고 장착해 주세요.","Craft and equip the Mining Arm at the workbench.")); break;
                case "box0": case "box1": case "box2":
                    if (carried>=0) { Hud.Toast(Theme.T("먼저 저울에 상자를 올려 주세요.","Place the held crate on the scale first.")); break; }
                    carried=int.Parse(id.Substring(3)); RefreshWorld(); break;
                case "scale": Scale(); break;
                case "mirrorA": case "mirrorB":
                    if (State.Has("laser")) break;
                    if (id=="mirrorA") { mirrorA=!mirrorA; laser.RotateMirror(new GridPosition(2,4)); }
                    else { mirrorB=!mirrorB; laser.RotateMirror(new GridPosition(9,4)); }
                    if (laser.IsSolved()) { AdventureRules.Complete(State,"laser"); Changed(Theme.T("전원이 연결되어 문이 열렸어요!","Power restored. The door is open!")); }
                    RefreshWorld(); break;
                case "boss.mirror":
                    if (State.Has("boss.laser")) break;
                    bossMirror=!bossMirror; bossLaser.RotateMirror(new GridPosition(3,4));
                    if (bossLaser.IsSolved()) { AdventureRules.Complete(State,"boss.laser"); Changed(Theme.T("골렘의 거울 보호막 해제!","Golem mirror shield released!")); }
                    RefreshWorld(); break;
                case "sequence": case "boss.sequence": Hud.Toast(Theme.T("2 → 4 → 6 순서로 발판을 밟으세요. 잘못 밟으면 점프로 지나가 보세요.","Step on 2, then 4, then 6. Jump over other plates.")); break;
                case "checkpoint": case "checkpoint-top": State.checkpointX=nearest.x; State.checkpointY=nearest.y; State.health=5; State.shield=1; Commit(); Hud.Toast(Theme.T("체크포인트 저장! 체력과 실드 회복.","Checkpoint saved. Health and shield restored.")); break;
                case "repair":
                    if (!AdventureRules.HasTool(State,1)) { Hud.Toast(Theme.T("파워 렌치가 필요합니다.","Equip the Power Wrench.")); break; }
                    if (State.Has("repaired")) break;
                    repairAttempts++;
                    if (repairAttempts>=2 || AdventureRules.HasEnchant(State,1,0)) { State.flags.Add("repaired"); Changed(Theme.T("보물 구역을 수리했어요!","Treasure passage repaired!")); }
                    else Hud.Toast(Theme.T("회로 하나를 연결했어요. 한 번 더 조사하세요.","One circuit connected. Interact once more.")); break;
                case "cache-a": case "cache-b": case "high-cache":
                    if (id=="high-cache" && !AdventureRules.HasTool(State,2)) { Hud.Toast(Theme.T("점프 부스터를 장착해 주세요.","Equip Jump Boots for the high chest.")); break; }
                    if (AdventureRules.Claim(State,id,12,new[]{5+(AdventureRules.HasEnchant(State,0,1)?1:0),3,3,3,0})) Changed(Theme.T("재료와 경험치를 찾았어요!","Materials and knowledge found!")); break;
                case "ladderA": Hud.Toast(Theme.T("위 버튼을 눌러 사다리를 올라가세요.","Hold UP to climb the ladder.")); break;
                case "boss":
                    if (AdventureRules.Finish(State)) { Changed(Theme.T("챕터 1 완료!","Chapter 1 complete!")); Hud.ClearScreen(); }
                    else Hud.Toast(Theme.T("골렘: 숫자·발판·거울 장치를 풀어 주시겠습니까?","Golem: Please solve the number, plate and mirror devices.")); break;
            }
        }
        private void Scale()
        {
            if (State.Has("bridge")) { Hud.Toast(Theme.T("다리가 이미 열려 있습니다.","The bridge is already open.")); return; }
            if (carried>=0) { weights[carried]=true; weight.PlaceWeight("crate"+carried,kg[carried]); carried=-1; }
            else for(int i=2;i>=0;i--) if(weights[i]) { weights[i]=false; weight.RemoveWeight("crate"+i); carried=i; break; }
            if (weight.IsSolved()) { AdventureRules.Complete(State,"bridge"); Changed(Theme.T("8kg! 다리가 내려왔습니다.","8kg! The bridge is lowered.")); }
            else Hud.Toast(Theme.T("현재 무게 ","Current weight ")+weight.CurrentWeight+" / 8kg");
            RefreshWorld();
        }
        private void CheckPlates()
        {
            string key=null; int hit=-1; bool boss=Mathf.Abs(Motor.Y-16)<.12f;
            if (Motor.Grounded && (boss || Mathf.Abs(Motor.Y-8)<.12f))
                for(int i=0;i<3;i++) if(Mathf.Abs(Motor.X-((boss?43:15)+i*3))<.55f) { hit=i; key=(boss?"boss.":"")+i; break; }
            if(key==null) { lastPlate=null; return; } if(key==lastPlate) return; lastPlate=key;
            string flag=boss?"boss.sequence":"sequence"; if(State.Has(flag)) return;
            var order=AdventureQuestions.PlateOrder(State.seed); var p=boss?bossSequence:sequence;
            bool correct=p.Step(order[boss?2-hit:hit]); world.PlateFeedback(boss,hit,correct);
            if (p.IsSolved()) { AdventureRules.Complete(State,flag); Changed(Theme.T("순서 장치를 해제했어요!","Sequence device released!")); }
            else if (!correct) Hud.Toast(Theme.T("다시 2부터 시작해 볼까요?","Try starting from 2 again."));
        }
        private void TickBoss()
        {
            if (Motor.Y<15 || Motor.X<37 || State.Has("clear")) { bossClock=0; return; }
            // Non-lethal arena pulse: answers themselves never cost health; all modal panels pause it.
            bossClock+=Time.unscaledDeltaTime;
            float warning=AdventureRules.HasEnchant(State,3,1)?4:2;
            if (bossClock>8 && bossClock<8+Time.unscaledDeltaTime*2) Hud.Toast(Theme.T("골렘: 바닥을 울리겠습니다. 점프해 주세요!","Golem: a floor pulse is coming. Please jump!"));
            if (bossClock>8+warning)
            {
                if (Motor.Grounded) { AdventureRules.Recover(State,false); Commit(); }
                bossClock=0;
            }
        }
        public bool Answer(string input)
        {
            bool correct=AdventureQuestions.Submit(State,input); Commit(); if(correct) { noProgress=0; RefreshWorld(); } return correct;
        }
        public bool Craft(int tool,int[] grid=null)
        {
            if (!AtWorkshop || !AdventureRules.Craft(State,tool,grid)) return false;
            Changed(Theme.Tool(tool)+Theme.T(" 제작 완료!"," crafted!")); return true;
        }
        public bool Enchant(int tool,int variant)
        {
            if (!AtWorkshop || !AdventureRules.Enchant(State,tool,variant)) return false;
            Changed(Theme.T("인챈트 적용!","Enchantment applied!")); return true;
        }
        public bool AtWorkshop { get { return Motor!=null && Mathf.Abs(Motor.X-18)<1.8f && Mathf.Abs(Motor.Y)<1.8f; } }
        public void ToggleEquipment(int tool) { if(State.owned[tool]) { State.equipped[tool]=!State.equipped[tool]; Commit(); RefreshWorld(); } }
        public void ResetLocalPuzzle()
        {
            if (Mathf.Abs(Motor.Y)<2 && Motor.X>36 && Motor.X<47 && !State.Has("bridge")) { weights=new bool[3]; carried=-1; weight.ResetPuzzle(); weight.StartPuzzle(); RefreshWorld(); }
            if (Mathf.Abs(Motor.Y-8)<2 && !State.Has("sequence")) { sequence.ResetPuzzle(); sequence.StartPuzzle(); }
            if (Mathf.Abs(Motor.Y-16)<2 && !State.Has("boss.sequence")) { bossSequence.ResetPuzzle(); bossSequence.StartPuzzle(); }
            lastPlate=null; Hud.Toast(Theme.T("현재 퍼즐만 다시 시작합니다. 획득한 장비는 유지됩니다.","Only the local puzzle was reset. Equipment is safe."));
        }
        public void ReturnCheckpoint() { Motor.Place(State.checkpointX,State.checkpointY); State.x=Motor.X; State.y=Motor.Y; Controls.Clear(); Commit(); Hud.ForceClose(); }
        public void Title() { Commit(); Running=false; CanContinue=save.TryLoad(out resume,out var backup); BackupRecovered=backup; Controls.Clear(); Hud.Title(); }
        private void Changed(string text) { noProgress=0; Commit(); RefreshWorld(); Hud.Toast(text); }
        private void RefreshWorld() { if(world!=null) world.Refresh(State,carried,weights,mirrorA,mirrorB,bossMirror); }
        public void Commit()
        {
            if (!Running || State==null) return;
            State.x=Motor.X; State.y=Motor.Y;
            try { save.Save(State); lastSave=Time.unscaledTime; }
            catch (Exception e) { Debug.LogWarning("Adventure save failed: "+e.Message); if(Hud!=null) Hud.Toast(Theme.T("저장에 실패했습니다. 저장 공간을 확인해 주세요.","Save failed. Please check storage availability.")); lastSave=Time.unscaledTime; }
        }
        private void OnApplicationFocus(bool focus) { suspended=!focus; Controls.Clear(); if(!focus) Commit(); }
        private void OnApplicationPause(bool pause) { suspended=pause; Controls.Clear(); if(pause) Commit(); }
        private void OnApplicationQuit() { Commit(); }
        private void OnDestroy() { Controls.Clear(); art?.Dispose(); Theme?.Dispose(); }
    }
}
