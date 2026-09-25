/* MazeMath Instant 1.0. Browser adaptation; not a Unity WebGL build. */
(function(root,factory){const c=factory();if(typeof module==='object')module.exports=c;else root.MM=c;})(typeof globalThis!=='undefined'?globalThis:this,function(){
'use strict';
const costs=[[2,1,0,0,0],[2,0,1,1,0],[2,0,2,1,0],[0,2,0,2,0],[2,0,2,1,0]];
const recipes=[
 [-1,1,-1,-1,0,-1,-1,0,-1],
 [2,-1,-1,-1,0,3,-1,-1,0],
 [2,-1,2,0,3,0,-1,-1,-1],
 [-1,1,-1,3,-1,3,-1,1,-1],
 [2,-1,2,-1,3,-1,0,-1,0]
];
const clamp=(n,a,b)=>Math.max(a,Math.min(b,n)),has=(s,k)=>s.flags.includes(k),tool=(s,i)=>s.owned[i]&&s.equipped[i];
const chapterIds=['chapter-01','chapter-02','chapter-03','chapter-04','chapter-05'];
function fresh(seed){return{v:1,seed:seed>>>0,x:14,y:0,vy:0,grounded:true,climb:null,face:1,checkpoint:[14,0],flags:[],items:[0,1,2,1,0],owned:Array(5).fill(false),equipped:Array(5).fill(false),ench:Array(5).fill(-1),unlocked:[],xp:0,health:5,shield:1,visited:[],question:null};}
function freshCampaign(seed){let legacy=fresh(seed);return campaignFromLegacy(legacy);}
function campaignFromLegacy(legacy){
  const cs={...legacy,items:[...legacy.items],owned:[...legacy.owned],equipped:[...legacy.equipped],ench:[...legacy.ench],unlocked:[...legacy.unlocked],flags:[...legacy.flags],visited:[...legacy.visited],checkpoint:[...legacy.checkpoint],question:legacy.question?{...legacy.question}:null};
  const inventory={items:[...cs.items],owned:[...cs.owned],equipped:[...cs.equipped],ench:[...cs.ench],unlocked:[...cs.unlocked]};
  return {...cs,v:2,chapter:'chapter-01',unlockedChapters:['chapter-01'],completedChapters:[],chapterState:cs,inventory,campaignXp:cs.xp};
}
function syncCampaign(s){if(!s||s.v!==2)return s;let cs=s.chapterState||s;if(s.inventory){cs.items=s.inventory.items;cs.owned=s.inventory.owned;cs.equipped=s.inventory.equipped;cs.ench=s.inventory.ench;cs.unlocked=s.inventory.unlocked;}cs.xp=s.campaignXp??cs.xp;Object.assign(s,{x:cs.x,y:cs.y,flags:cs.flags,items:cs.items,owned:cs.owned,equipped:cs.equipped,ench:cs.ench,unlocked:cs.unlocked,xp:cs.xp,visited:cs.visited,question:cs.question,checkpoint:cs.checkpoint});return s;}
function markChapterComplete(s,id){if(!s||s.v!==2||!chapterIds.includes(id))return false;if(!s.completedChapters.includes(id))s.completedChapters.push(id);let i=chapterIds.indexOf(id),next=chapterIds[i+1];if(next&&!s.unlockedChapters.includes(next))s.unlockedChapters.push(next);return true;}
function unlocks(s){return s&&s.v===2?[...s.unlockedChapters]:['chapter-01'];}
function startChapter(s,id){if(!s||s.v!==2||!s.unlockedChapters.includes(id))return false;s.chapter=id;let start=(typeof MMChapters!=='undefined'&&MMChapters.get(id)?MMChapters.get(id).start:[8,0]);let cs=fresh((s.seed+chapterIds.indexOf(id)*9973)>>>0);cs.x=start[0];cs.y=start[1];cs.items=[...s.inventory.items];cs.owned=[...s.inventory.owned];cs.equipped=[...s.inventory.equipped];cs.ench=[...s.inventory.ench];cs.unlocked=[...s.inventory.unlocked];cs.xp=s.campaignXp;s.chapterState=cs;s.inventory={items:cs.items,owned:cs.owned,equipped:cs.equipped,ench:cs.ench,unlocked:cs.unlocked};syncCampaign(s);return true;}
function rng(seed){let x=(seed>>>0)||1831565813;return n=>{x^=x<<13;x^=x>>>17;x^=x<<5;return(x>>>0)%n;};}
function hash(text){let h=2166136261;for(let c of String(text))h=Math.imul(h^c.charCodeAt(0),16777619);return h>>>0;}
function question(seed,id,level){let r=rng(hash(seed+':'+id+':'+level)),a,b,answer,prompt,op=level===0?0:r(5);switch(op){case 0:a=11+r(17);b=3+r(8);answer=a+b;prompt=`${a} + ${b} = ?`;break;case 1:b=3+r(8);answer=5+r(20);a=answer+b;prompt=`${a} − ${b} = ?`;break;case 2:a=[2,3,5][r(3)];b=2+r(5);answer=a*b;prompt=`${a} × ${b} = ?`;break;case 3:b=[2,3,5][r(3)];answer=2+r(5);a=b*answer;prompt=`${a} ÷ ${b} = ?`;break;default:a=5+r(12);answer=3+r(10);b=a+answer;prompt=`${a} + □ = ${b}`;}
return{id,prompt,answer,op,a,b,level,solved:false};}
function check(q,text){return /^\d{1,3}$/.test(text)&&Number(text)===q.answer;}
function chapterQuestion(seed,chapter,id,level){
 let r=rng(hash(seed+':'+chapter+':'+id+':'+level)),a,b,answer,prompt,kind;
 if(chapter==='chapter-02'){
   let mode=r(3);
   if(mode===0){a=[2,3,5][r(3)];b=2+r(6);answer=a*b;prompt=`${a} × ${b} = ?`;kind='mul';}
   else if(mode===1){b=[2,3,5][r(3)];answer=2+r(6);a=b*answer;prompt=`${a} ÷ ${b} = ?`;kind='div';}
   else{a=8+r(18);b=2+r(9);answer=a+b;prompt=`${a} + ${b} = ?`;kind='add';}
 }else if(chapter==='chapter-03'){
   a=4+r(12);answer=2+r(10);b=a+answer;prompt=`${a} + □ = ${b}`;kind='missing';
 }else if(chapter==='chapter-04'){
   if(r(2)){a=12+r(18);b=3+r(9);answer=a-b;prompt=`${a} − ${b} = ?`;kind='sub';}
   else{a=[2,3,5][r(3)];b=2+r(5);answer=a*b;prompt=`${a} × ${b} = ?`;kind='mul';}
 }else if(chapter==='chapter-05'){
   let mode=r(4);if(mode===0){a=[2,3,5][r(3)];b=2+r(6);answer=a*b;prompt=`${a} × ${b} = ?`;kind='mul';}
   else if(mode===1){b=[2,3,5][r(3)];answer=2+r(6);a=b*answer;prompt=`${a} ÷ ${b} = ?`;kind='div';}
   else if(mode===2){a=10+r(20);b=2+r(10);answer=a+b;prompt=`${a} + ${b} = ?`;kind='add';}
   else{a=6+r(12);answer=3+r(9);b=a+answer;prompt=`${a} + □ = ${b}`;kind='missing';}
 }else return {...question(seed,id,level),kind:'basic'};
 return{id,prompt,answer,a,b,kind,level,solved:false};
}

function claim(s,id,xp,items){if(has(s,'reward/'+id))return false;s.flags.push('reward/'+id);s.xp=Math.min(99999,s.xp+xp);items.forEach((n,i)=>s.items[i]=Math.min(9999,s.items[i]+n));return true;}
function complete(s,k){if(!['math','bridge','laser','sequence','boss.math','boss.sequence','boss.laser'].includes(k)||has(s,k))return false;s.flags.push(k);claim(s,k,k.startsWith('boss.')?20:10,k==='math'?[2,0,0,0,0]:[2,1,1,1,0]);if(hasEnchant(s,3,0))s.shield=1;return true;}
function craft(s,i,grid){if(!Number.isInteger(i)||i<0||i>4||s.owned[i]||(i!==0&&!s.owned[0]))return false;if(!Array.isArray(grid)||grid.length!==9||grid.some((v,n)=>v!==recipes[i][n]))return false;let c=costs[i];if(c.some((v,n)=>v>s.items[n]))return false;c.forEach((v,n)=>s.items[n]-=v);s.owned[i]=s.equipped[i]=true;return true;}
function enchant(s,i,v){if(!s.owned[i]||![0,1].includes(v))return false;let id=i*2+v;if(!s.unlocked.includes(id)){if(s.xp<10)return false;s.xp-=10;s.unlocked.push(id);}s.ench[i]=v;s.equipped[i]=true;return true;}
function hasEnchant(s,i,v){return tool(s,i)&&s.ench[i]===v&&s.unlocked.includes(i*2+v);}
function shields(s){return 3-['boss.math','boss.sequence','boss.laser'].filter(k=>has(s,k)).length;}
function finish(s){if(shields(s)||has(s,'clear'))return false;s.flags.push('clear');claim(s,'clear',30,[0,0,0,0,1]);return true;}
function solids(s){if(s.chapterId&&s.chapterId!=='chapter-01')return[[0,-.5,60,0,1],[0,7.5,60,8,1],[0,15.5,60,16,1],[-1,-4,0,26,0],[60,-4,61,26,0],[3,18.7,9,19,1]];let a=[[0,-.5,47,0,1],[50,-.5,60,0,1],[0,7.5,60,8,1],[0,15.5,60,16,1],[-1,-4,0,26,0],[60,-4,61,26,0],[3,18.7,9,19,1]];if(has(s,'bridge'))a.push([47,-.25,50,0,1]);else a.push([46,0,46.5,6.8,0]);if(!has(s,'mined'))a.push([35.6,0,36.4,6.8,0]);if(!has(s,'laser'))a.push([35.6,8,36.4,14.8,0]);if(!has(s,'repaired'))a.push([11.6,8,12.4,14.8,0],[11.6,16,12.4,22.8,0]);return a;}
function ladders(s){if(s.chapterId&&s.chapterId!=='chapter-01')return[[54,0,8],[30,8,16],[18,0,8]];return has(s,'sequence')?[[54,0,8],[30,8,16],[18,0,8]]:[[54,0,8]];}
function move(s,h,v,jump,dt){dt=clamp(dt,0,.05);if(!dt)return;h=clamp(h,-1,1);v=clamp(v,-1,1);let list=solids(s),hw=.38,height=1.65;if(!s.climb&&s.vy<=0)s.grounded=list.some(f=>s.x+hw>f[0]&&s.x-hw<f[2]&&Math.abs(s.y-f[3])<.003);if(h){s.climb=null;s.face=h>0?1:-1;}
if(v)for(let l of ladders(s)){if(Math.abs(s.x-l[0])>.7||s.y<l[1]-.1||s.y>l[2]+.1||v>0&&s.y>=l[2]-.001||v<0&&s.y<=l[1]+.001)continue;s.climb=l;s.x=l[0];break;}
if(jump&&s.grounded&&!s.climb){s.vy=tool(s,2)?13:9.5;s.grounded=false;}
let tx=s.x+h*6*dt;for(let f of list){if(f[4]||s.y+height<=f[1]+.001||s.y>=f[3]-.001)continue;if(h>0&&s.x+hw<=f[0]+.001&&tx+hw>f[0])tx=Math.min(tx,f[0]-hw);if(h<0&&s.x-hw>=f[2]-.001&&tx-hw<f[2])tx=Math.max(tx,f[2]+hw);}s.x=clamp(tx,.4,59.6);
if(s.climb){let l=s.climb;s.vy=0;s.grounded=false;s.y=clamp(s.y+v*4.5*dt,l[1],l[2]);if(s.y>=l[2]&&v>0||s.y<=l[1]&&v<0){s.climb=null;s.grounded=true;}}
else{s.vy=Math.max(-18,s.vy-24*dt);let ny=s.y+s.vy*dt;s.grounded=false;for(let f of list){if(s.x+hw<=f[0]||s.x-hw>=f[2])continue;if(s.vy<=0&&s.y>=f[3]-.002&&ny<=f[3]){ny=Math.max(ny,f[3]);s.grounded=true;s.vy=0;}else if(!f[4]&&s.vy>0&&s.y+height<=f[1]&&ny+height>=f[1]){ny=f[1]-height;s.vy=0;}}s.y=ny;}
if(s.y< -3){s.x=s.checkpoint[0];s.y=s.checkpoint[1];s.vy=0;s.grounded=true;s.climb=null;}
let room=clamp(Math.floor((s.y+.25)/8),0,2)*5+clamp(Math.floor(s.x/12),0,4);if(!s.visited.includes(room))s.visited.push(room);}
function goal(s){if(!has(s,'math'))return[30,0,'숫자 장치를 찾아 문제를 풀어요'];if(!s.owned[0])return[18,0,'작업대로 돌아가 곡괭이 팔 만들기 · 철 조각 2 + 철판 1'];if(!has(s,'mined'))return[34,0,'곡괭이 팔로 균열벽을 열어요'];if(!has(s,'bridge'))return[44,0,'상자를 옮겨 저울을 8kg으로 맞춰요'];if(!has(s,'laser'))return[45,8,'위층의 거울 2개를 연결해요'];if(!has(s,'sequence'))return[18,8,'발판을 2 → 4 → 6 순서로 밟아요'];if(!has(s,'boss.math'))return[38,16,'골렘의 숫자 보호막을 풀어요'];if(!has(s,'boss.sequence'))return[44,16,'골렘 앞 발판을 2 → 4 → 6 순서로 밟아요'];if(!has(s,'boss.laser'))return[54,16,'골렘의 거울 보호막을 풀어요'];return[57,16,has(s,'clear')?'모모와 탐험 성공!':'골렘에게 다가가 조사해요'];}
function waypoint(s){let g=goal(s),f=clamp(Math.floor((s.y+.25)/8),0,2),t=g[1]/8;if(f===t)return g;if(t===2&&f===1)return[30,8,'가운데 사다리로 3층에 올라가요'];if(f===2)return[30,16,'가운데 사다리로 내려가요'];return[has(s,'sequence')&&g[0]<36?18:54,f*8,t>f?'사다리에서 ↑를 눌러 올라가요':'사다리에서 ↓를 눌러 내려가요'];}
function validLegacy(s){if(!s||!Number.isInteger(s.seed)||!Number.isFinite(s.x)||!Number.isFinite(s.y)||s.x<0||s.x>72||s.y< -3||s.y>34||!Array.isArray(s.flags)||s.flags.some(k=>typeof k!=='string'||k.length>90))return false;for(let key of ['items','owned','equipped','ench'])if(!Array.isArray(s[key])||s[key].length!==5)return false;return !s.items.some(n=>!Number.isInteger(n)||n<0||n>9999)&&Number.isInteger(s.xp)&&s.xp>=0&&s.xp<=99999&&Array.isArray(s.visited)&&Array.isArray(s.unlocked)&&Array.isArray(s.checkpoint)&&s.checkpoint.length===2&&!s.checkpoint.some(n=>!Number.isFinite(n))&&(!s.question||(Number.isInteger(s.question.answer)&&typeof s.question.prompt==='string'));}
function restore(raw){try{let s=JSON.parse(raw);if(s&&s.v===1){if(!validLegacy(s))return null;s.vy=0;s.climb=null;s.grounded=true;return campaignFromLegacy(s);}if(!s||s.v!==2||!chapterIds.includes(s.chapter)||!Array.isArray(s.unlockedChapters)||!Array.isArray(s.completedChapters))return null;let cs=s.chapterState&&s.chapterState!==s?s.chapterState:s;if(!validLegacy(cs))return null;if(!s.inventory||!['items','owned','equipped','ench','unlocked'].every(k=>Array.isArray(s.inventory[k])))return null;s.vy=cs.vy=0;s.climb=cs.climb=null;s.grounded=cs.grounded=true;if(s.chapterState!==s){Object.assign(s,{x:cs.x,y:cs.y,flags:cs.flags,items:s.inventory.items,owned:s.inventory.owned,equipped:s.inventory.equipped,ench:s.inventory.ench,unlocked:s.inventory.unlocked,xp:s.campaignXp??cs.xp,visited:cs.visited,question:cs.question,checkpoint:cs.checkpoint});}return s;}catch{return null;}}
return{fresh,freshCampaign,markChapterComplete,unlocks,startChapter,syncCampaign,rng,hash,has,tool,costs,recipes,question,chapterQuestion,check,claim,complete,craft,enchant,hasEnchant,shields,finish,solids,ladders,move,goal,waypoint,restore};
});
