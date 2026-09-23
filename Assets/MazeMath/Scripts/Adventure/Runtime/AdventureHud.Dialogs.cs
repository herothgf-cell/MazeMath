using System;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.Adventure
{
    public sealed partial class AdventureHud
    {
        private int selectedTool,selectedMaterial=2;
        private int[] patternGrid={-1,-1,-1,-1,-1,-1,-1,-1,-1};
        public void Title()
        {
            Modal(t.T("모모의 작은 모험","Momo's little adventure"),"title");
            var face=t.Rect(body,"WelcomeMomo",.38f,.64f,.62f,.97f); var image=t.Fill(face,Color.white); image.sprite=art.Get("robot"); image.preserveAspect=true;
            t.Label(body,t.T("길을 찾고, 장비를 만들고, 골렘 친구를 도와요.","Find your path, craft tools and help the guardian."),17,TextAnchor.MiddleCenter,.05f,.49f,.95f,.65f);
            var b=t.Button(body,t.T("이어서 탐험","Continue exploring"),()=>game.Continue(),.18f,.31f,.82f,.44f,true); b.interactable=game.CanContinue;
            t.Button(body,t.T("새 탐험 시작","New adventure"),ConfirmNew,.18f,.14f,.82f,.27f);
            t.Label(body,t.T("방향키 이동 · Space 점프 · E 조사","Arrows move · Space jump · E use"),13,TextAnchor.MiddleCenter,.03f,0,.97f,.11f).color=t.Muted;
        }
        private void ConfirmNew()
        {
            if(!game.CanContinue) { game.BeginNew(); return; }
            Modal(t.T("새 모험을 시작할까요?","Start a new adventure?"),"confirm");
            t.Label(body,t.T("현재 이어하기 기록이 새 모험으로 바뀝니다.","Your current continue slot will be replaced."),17,TextAnchor.MiddleCenter,.05f,.48f,.95f,.85f);
            t.Button(body,t.T("취소","Cancel"),Title,.07f,.19f,.47f,.35f);
            t.Button(body,t.T("새로 시작","Start new"),()=>game.BeginNew(),.53f,.19f,.93f,.35f,true);
        }
        public void Pause()
        {
            if(!game.Running) return; game.Commit(); Modal(t.T("잠시 쉬어가기","Take a little break"),"pause");
            t.Button(body,t.T("계속 탐험","Resume"),Close,.12f,.84f,.88f,.98f,true);
            t.Button(body,t.T("목표 방향 안내","Show route hint"),()=>{game.ShowGuide=true;Close();},.12f,.68f,.88f,.81f);
            t.Button(body,t.T("체크포인트로 돌아가기","Return to checkpoint"),()=>game.ReturnCheckpoint(),.12f,.52f,.88f,.65f);
            t.Button(body,t.T("현재 퍼즐 다시 시작","Reset local puzzle"),()=>{game.ResetLocalPuzzle();Close();},.12f,.36f,.88f,.49f);
            t.Button(body,showTouch?t.T("화면 조작 버튼 끄기","Hide touch controls"):t.T("화면 조작 버튼 켜기","Show touch controls"),()=>{SetTouchControls(!showTouch);Pause();},.12f,.20f,.88f,.33f);
            t.Button(body,t.T("저장하고 제목 화면","Save and title"),()=>game.Title(),.12f,.04f,.88f,.17f);
        }
        public void Map()
        {
            if(!game.Running) return; Modal(t.T("우리의 탐험 지도","Our explorer map"),"map");
            int current=AdventureWorld.RoomAt(game.State.x,game.State.y);
            for(int f=0;f<3;f++) for(int r=0;r<5;r++)
            {
                int id=f*5+r; float x=.04f+r*.19f,y=.15f+f*.22f;
                var cell=t.Box(body,"MapRoom",x,y,x+.17f,y+.18f); cell.GetComponent<Image>().color=id==current?t.Mint:game.State.visited[id]?t.Sky:t.Edge;
                t.Label(cell,game.State.visited[id]?RoomName(id):"?",14,TextAnchor.MiddleCenter,.06f,.08f,.94f,.92f);
            }
            t.Label(body,t.T("가 본 곳이 지도에 남아요. 지금 위치는 민트색이에요.","Places you explore stay on the map. Mint marks your position."),15,TextAnchor.MiddleCenter,.02f,.82f,.98f,.97f);
            t.Button(body,t.T("다음 목표 방향 보기","Show objective direction"),()=>{game.ShowGuide=true;Close();},.20f,0,.80f,.10f);
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
            if(!game.Running) return; Modal(t.T("모모의 가방","Momo's backpack")+" · XP "+game.State.xp,"bag"); ResourceLine(.82f,.98f);
            for(int i=0;i<6;i++)
            {
                int k=i; float x=.025f+(i%3)*.325f,y=.45f-(i/3)*.31f;
                var b=t.Button(body,"",()=>{if(k<5){game.ToggleEquipment(k);Bag();}},x,y,x+.30f,y+.27f);
                var r=t.Rect(b.transform,"ItemIcon",.30f,.37f,.70f,.93f); var icon=t.Fill(r,Color.white); icon.sprite=art.Get(i<5?i.ToString():"robot"); icon.preserveAspect=true;
                string label=i<5?t.Tool(i)+"\n"+(game.State.owned[i]?game.State.equipped[i]?t.T("장착 중","Equipped"):t.T("미장착","Unequipped"):t.T("아직 없음","Not crafted")):"MOMO";
                t.Label(b.transform,label,14,TextAnchor.MiddleCenter,.03f,.03f,.97f,.36f);
                b.interactable=i<5 && game.State.owned[i];
                if(i<5 && !game.State.owned[i]) icon.color=new Color(1,1,1,.35f);
            }
            t.Label(body,t.T("장비를 눌러 장착/해제 · 제작은 작업대에서","Tap to equip or unequip. Craft at the workbench."),14,TextAnchor.MiddleCenter,0,0,1,.12f).color=t.Muted;
        }
        public void Workshop()
        {
            if(!game.AtWorkshop) return; Modal(t.T("모모의 작업대","Momo's workbench")+" · XP "+game.State.xp,"workshop"); WorkshopTabs(); ResourceLine(.71f,.85f);
            for(int i=0;i<5;i++)
            {
                int k=i; float y=.56f-i*.115f; var cost=AdventureRules.Cost(i); string line=t.Tool(i)+"\n";
                for(int m=0;m<5;m++)if(cost[m]>0)line+=t.Material(m)+" "+game.State.materials[m]+"/"+cost[m]+"  ";
                t.Label(body,line,15,TextAnchor.MiddleLeft,.025f,y,.72f,y+.105f);
                var b=t.Button(body,game.State.owned[i]?t.T("완료","Owned"):t.T("만들기","Craft"),()=>{bool ok=game.Craft(k);Workshop();if(!ok)Toast(t.T("재료를 확인해 주세요. 첫 장비는 곡괭이 팔이에요.","Check materials. Make the mining arm first."));},.74f,y,.98f,y+.105f,true);
                b.interactable=!game.State.owned[i];
            }
        }
        private void WorkshopTabs()
        {
            t.Button(body,t.T("제작","Craft"),Workshop,0,.89f,.32f,1);
            t.Button(body,"3×3",()=>{selectedTool=4;Pattern();},.34f,.89f,.66f,1);
            t.Button(body,t.T("인챈트","Enchant"),EnchantMenu,.68f,.89f,1,1);
        }
        private void ResourceLine(float y0,float y1)
        {
            for(int i=0;i<5;i++)
            {
                var r=t.Box(body,"MaterialChip",i*.20f+.005f,y0,(i+1)*.20f-.005f,y1); r.GetComponent<Image>().color=t.Rail;
                string count=game.State.materials[i].ToString()+(game.State.pending[i]>0?" +"+game.State.pending[i]:"");
                t.Label(r,t.Material(i)+"\n"+count,13,TextAnchor.MiddleCenter,.02f,.04f,.98f,.96f);
            }
        }
        private void Pattern()
        {
            if(!game.AtWorkshop) return; if(selectedTool!=0 && selectedTool!=4)selectedTool=4;
            Modal(t.T("조각을 모아 조립해요","Let's put the pieces together"),"pattern"); WorkshopTabs();
            t.Button(body,t.Tool(4),()=>{selectedTool=4;ResetGrid();Pattern();},0,.74f,.48f,.85f,selectedTool==4);
            t.Button(body,t.Tool(0),()=>{selectedTool=0;ResetGrid();Pattern();},.52f,.74f,1,.85f,selectedTool==0);
            var recipe=AdventureRules.Pattern(selectedTool);
            t.Label(body,t.T("설계도","Blueprint"),14,TextAnchor.MiddleCenter,0,.63f,.34f,.72f).color=t.Muted;
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            {
                int n=row*3+col,index=n; float x=col*.11f,y=.33f+(2-row)*.09f;
                var cell=t.Box(body,"RecipeCell",x,y,x+.105f,y+.085f); cell.GetComponent<Image>().color=t.Rail;
                t.Label(cell,recipe[n]<0?"":t.Material(recipe[n]),11,TextAnchor.MiddleCenter,.02f,.02f,.98f,.98f);
                float gx=.40f+col*.195f,gy=.25f+(2-row)*.135f;
                t.Button(body,patternGrid[n]<0?"·":t.Material(patternGrid[n]),()=>{patternGrid[index]=selectedMaterial;Pattern();},gx,gy,gx+.18f,gy+.125f);
            }
            for(int i=-1;i<4;i++)
            {
                int m=i; float x=(i+1)*.20f;
                t.Button(body,i<0?t.T("지우기","Clear"):t.Material(i),()=>{selectedMaterial=m;Pattern();},x,.10f,x+.19f,.21f,i==selectedMaterial);
            }
            t.Button(body,t.T("조립하기","Assemble"),()=>{bool ok=game.Craft(selectedTool,patternGrid);Toast(ok?t.T("조립 완료!","Assembled!"):t.T("배치와 재료를 확인해요. 재료는 그대로예요.","Check pattern and materials. Nothing was consumed."));if(ok){ResetGrid();Workshop();}},.39f,0,.99f,.087f,true);
            t.Label(body,t.T("재료 선택\n칸을 톡!","Choose a material\nThen tap a cell"),13,TextAnchor.MiddleCenter,0,0,.37f,.25f).color=t.Muted;
        }
        private void ResetGrid() { patternGrid=new[]{-1,-1,-1,-1,-1,-1,-1,-1,-1}; }
        private void EnchantMenu()
        {
            if(!game.AtWorkshop) return; Modal(t.T("장비에 특별한 힘을","A little extra magic")+" · XP "+game.State.xp,"enchant"); WorkshopTabs(); ResourceLine(.70f,.84f);
            for(int i=0;i<5;i++) { int index=i; var b=t.Button(body,t.Tool(i),()=>{selectedTool=index;EnchantMenu();},i*.20f,.50f,(i+1)*.20f-.01f,.67f,i==selectedTool); b.interactable=game.State.owned[i]; }
            for(int v=0;v<2;v++)
            {
                int variant=v; float y=.24f-v*.20f;
                var b=t.Button(body,t.Effect(selectedTool,v)+"\n"+(game.State.unlockedEnchants[selectedTool*2+v]?t.T("무료로 변경","Switch free"):"10 XP"),()=>{if(!game.Enchant(selectedTool,variant))Toast(t.T("장비와 XP를 확인해 주세요.","Check equipment and XP."));EnchantMenu();},.02f,y,.98f,y+.17f,true);
                b.interactable=game.State.owned[selectedTool];
            }
        }
        public void ClearScreen()
        {
            Modal(t.T("모모와 함께 해냈어요!","You and Momo did it!"),"clear");
            var r=t.Rect(body,"HappyMomo",.38f,.67f,.62f,.98f); var icon=t.Fill(r,Color.white); icon.sprite=art.Get("robot"); icon.preserveAspect=true;
            t.Label(body,t.T("모모의 계산 코어를 되찾았어요.\n길을 기억하고 골렘 친구도 도왔네요!","You recovered Momo's core.\nAnd helped a new friend along the way!"),18,TextAnchor.MiddleCenter,.05f,.35f,.95f,.65f);
            t.Button(body,t.T("남은 보물 탐험하기","Explore the remaining treasures"),Close,.1f,.13f,.9f,.29f,true);
        }
    }
}
