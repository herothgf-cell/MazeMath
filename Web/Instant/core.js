/* Campaign rules and save migration. No DOM, animation frames or browser storage here. */
(function(root,factory){const node=typeof module==='object'&&module.exports;const api=factory(node?require('./chapter-data.js'):root.MMChapters,node?require('./puzzles.js'):root.MMPuzzles);if(node)module.exports=api;else root.MM=api;})(typeof globalThis!=='undefined'?globalThis:this,function(D,P){
'use strict';
const costs=[[2,1,0,0,0],[2,0,1,1,0],[2,0,2,1,0],[0,2,0,2,0],[2,0,2,1,0]].map(Object.freeze);
const recipes=[[-1,1,-1,-1,0,-1,-1,0,-1],[2,-1,-1,-1,0,3,-1,-1,0],[2,-1,2,0,3,0,-1,-1,-1],[-1,1,-1,3,-1,3,-1,1,-1],[2,-1,2,-1,3,-1,0,-1,0]].map(Object.freeze);
Object.freeze(costs);Object.freeze(recipes);
const clamp=(n,a,b)=>Math.max(a,Math.min(b,n)),clone=x=>JSON.parse(JSON.stringify(x));
const ids=D.list().map(d=>d.id),numeric=n=>typeof n==='number'&&Number.isFinite(n);
function freshState(seed,id){const d=D.get(id);return{chapterId:id,seed:P.hash(seed+':'+id),x:d.start[0],y:d.start[1],vy:0,face:1,grounded:true,climb:null,checkpoint:d.start.slice(),flags:[],visited:[],puzzles:{},toolHits:{},selectedHotbar:0,health:5,shield:1,lastPlate:null,action:null,clock:0};}
function fresh(seed){return{v:3,seed:seed>>>0,chapter:ids[0],inventory:{items:[0,1,2,1,0],owned:Array(5).fill(false),equipped:Array(5).fill(false),ench:Array(5).fill(-1),unlocked:[]},xp:0,completedChapters:[],unlockedChapters:[ids[0]],states:{[ids[0]]:freshState(seed,ids[0])}};}
function session(s){return s.states[s.chapter];}
function definition(s){return D.get(s.chapter);}
function has(s,id){return session(s).flags.includes(id);}
function flag(s,id){if(has(s,id))return false;session(s).flags.push(id);return true;}
function owned(s,i){return !!s.inventory.owned[i];}
function tool(s,i){return owned(s,i)&&!!s.inventory.equipped[i];}
function nextUnlocks(s){const done=new Set(s.completedChapters);s.unlockedChapters=ids.filter((id,i)=>i===0||done.has(ids[i-1]));}
function startChapter(s,id){if(!D.get(id)||!s.unlockedChapters.includes(id))return false;session(s).action=null;s.chapter=id;if(!s.states[id])s.states[id]=freshState(s.seed,id);session(s).climb=null;session(s).vy=0;session(s).lastPlate=null;session(s).action=null;return true;}
function reward(s,id,xp=10,items=[0,0,0,0,0]){if(!flag(s,'reward/'+id))return false;s.xp=clamp(s.xp+xp,0,99999);items.forEach((n,i)=>s.inventory.items[i]=clamp(s.inventory.items[i]+n,0,9999));return true;}
function complete(s,id){if(!flag(s,id))return false;reward(s,id,id.startsWith('boss.')?20:10,id==='math'?[2,0,0,0,0]:[1,0,0,0,0]);if(tool(s,3)&&s.inventory.ench[3]===0)session(s).shield=1;return true;}
function craft(s,i,grid){
 if(!Number.isInteger(i)||i<0||i>=5)return{ok:false,code:'invalid'};
 if(owned(s,i))return{ok:false,code:'owned'};
 if(i!==0&&!owned(s,0))return{ok:false,code:'first-tool'};
 if(!Array.isArray(grid)||grid.length!==9)return{ok:false,code:'pattern'};
 for(let n=0;n<9;n++)if(grid[n]!==recipes[i][n])return{ok:false,code:'pattern'};
 if(costs[i].some((n,k)=>s.inventory.items[k]<n))return{ok:false,code:'materials'};
 costs[i].forEach((n,k)=>s.inventory.items[k]-=n);s.inventory.owned[i]=s.inventory.equipped[i]=true;session(s).selectedHotbar=i;
 return{ok:true,code:'crafted'};
}
function equip(s,i){if(!owned(s,i))return false;s.inventory.equipped[i]=true;session(s).selectedHotbar=i;return true;}
function enchant(s,i,v){if(!owned(s,i)||![0,1].includes(v))return false;const id=i*2+v;if(!s.inventory.unlocked.includes(id)){if(s.xp<10)return false;s.xp-=10;s.inventory.unlocked.push(id);}s.inventory.ench[i]=v;s.inventory.equipped[i]=true;return true;}
function ensureSupplies(s){const remaining=definition(s).requiredTools.filter(i=>!owned(s,i)),needs=Array(5).fill(0);remaining.forEach(i=>costs[i].forEach((v,j)=>needs[j]+=v));needs.forEach((v,j)=>s.inventory.items[j]=Math.max(s.inventory.items[j],v));}
function currentTask(s){return definition(s).tasks.find(t=>!has(s,t.id))||null;}
function fieldDone(s){return !currentTask(s);}
function bossInfo(s){const d=definition(s),done=d.boss.questions.filter(q=>has(s,q.id)).length;return{available:fieldDone(s),done,total:d.boss.questions.length,next:d.boss.questions.find(q=>!has(s,q.id))||null,cleared:has(s,'clear')};}
function objective(s){
 const d=definition(s),t=currentTask(s),st=session(s);
 if(t){if(t.type!=='supply'&&t.id!=='math'){
   const missing=d.requiredTools.find(i=>!owned(s,i));
   if(missing!==undefined)return{kind:'craft',tool:missing,x:18,floor:0,id:'craft'+missing,text:`작업대에서 ${D.tools[missing]}을 3×3으로 만들어요.`};
   if(t.tool!==undefined&&!tool(s,t.tool))return{kind:'equip',tool:t.tool,x:st.x,floor:Math.floor((st.y+.2)/8),id:'equip'+t.tool,text:`가방에서 이미 만든 ${D.tools[t.tool]}을 장착해요.`};
  }
  let text=t.text||`${t.label}을 조사해요.`;
  if(t.type==='sequence'){const p=puzzle(s,t.id),n=p.input.length;text=`발판 ${n}/${p.sequence.length} · 다음은 ${p.sequence[n]} — 직접 밟아요.`;}
  return{kind:t.type,id:t.id,x:t.x,floor:t.floor,text,tool:t.tool};
 }
 const b=bossInfo(s);return{kind:b.cleared?'clear':'boss',id:'golem',x:d.boss.x,floor:d.boss.floor,text:b.cleared?'탐험 완료! 다음 챕터로 떠나요.':`${d.boss.label}에게 말해 ${b.done+1}/${b.total}번째 문제를 받아요.`};
}
function floorDone(s,f){return definition(s).tasks.filter(t=>t.floor===f).every(t=>has(s,t.id));}
function solids(s){const d=definition(s),a=[];for(let f=0;f<d.floors;f++)a.push([0,f*8-.5,d.width,f*8,1,0]);for(let f=0;f<2;f++)if(!floorDone(s,f)){const x=d.gateX[f];a.push([x-.32,f*8,x+.32,f*8+6.5,0,f===0?1:-1]);}a.push([-1,-4,0,26,0,0],[d.width,-4,d.width+1,26,0,0]);return a;}
function ladders(s){return definition(s).ladders;}
function waypoint(s){const o=objective(s),st=session(s),f=clamp(Math.floor((st.y+.2)/8),0,2);if(f===o.floor)return o;const l=ladders(s).find(v=>o.floor>f?v[1]===f*8:v[2]===f*8);return l?{kind:'ladder',id:'ladder',x:l[0],floor:f,text:o.floor>f?'사다리 앞에서 ↑를 누르고 올라가요.':'사다리 앞에서 ↓를 누르고 내려가요.'}:o;}
function descriptor(s,id){const d=definition(s);return d.tasks.find(t=>t.id===id)||d.boss.questions.find(t=>t.id===id)||null;}
function puzzle(s,id){const st=session(s),desc=descriptor(s,id);if(!desc)return null;if(!st.puzzles[id])st.puzzles[id]=P.create(desc.type,P.hash(st.seed+':'+id),desc);return st.puzzles[id];}
function canUse(s,t){if(!t)return{ok:false,code:'missing'};if(has(s,t.id))return{ok:false,code:'complete'};const o=objective(s);if(o.kind==='craft')return{ok:false,code:'craft',tool:o.tool};if(o.kind==='equip')return{ok:false,code:'equip',tool:o.tool};if(currentTask(s)?.id!==t.id)return{ok:false,code:'order',text:o.text};if(t.tool!==undefined&&!tool(s,t.tool))return{ok:false,code:owned(s,t.tool)?'equip':'craft',tool:t.tool};return{ok:true};}
function nearTask(s,id,reach=1.9){const d=descriptor(s,id)||objects(s).find(t=>t.id===id),st=session(s);return !!d&&Math.abs(st.x-d.x)<=reach&&Math.abs(st.y-d.floor*8)<2.2;}
function interact(s,id){
 const st=session(s);if(id==='drop'&&st.carried){st.carried=null;return{ok:true,code:'putdown'};}if(st.action&&st.clock<st.action.until)return{ok:false,code:'busy'};
 if(id==='golem'){if(!nearTask(s,'golem',2.5))return{ok:false,code:'far'};const b=bossInfo(s);if(b.cleared)return{ok:true,code:'clear'};if(!b.available)return{ok:false,code:'order',text:objective(s).text};return{ok:true,code:'boss',question:b.next};}
 if(id==='workshop')return nearTask(s,id,2)?{ok:true,code:'workshop'}:{ok:false,code:'far'};
 const optional=definition(s).optional.find(t=>t.id===id);if(optional){if(!nearTask(s,id))return{ok:false,code:'far'};return{ok:reward(s,id,5,[2,1,1,1,0]),code:'cache'};}
 const crate=objects(s).find(t=>t.id===id&&t.kind==='weightCrate');if(crate){if(!nearTask(s,id,1.5))return{ok:false,code:'far'};const t=descriptor(s,crate.parent),check=canUse(s,t);if(!check.ok)return check;if(st.carried)return{ok:false,code:'carrying'};st.carried={id:t.id,index:crate.index};return{ok:true,code:'carried'};}
 const t=definition(s).tasks.find(t=>t.id===id);if(!t||!nearTask(s,id))return{ok:false,code:'far'};const check=canUse(s,t);if(!check.ok)return check;
 if(t.type==='supply'){ensureSupplies(s);complete(s,id);return{ok:true,code:'supply'};}
 if(['repair','mine','shield'].includes(t.type)){
  const selected=st.selectedHotbar;
  if(selected!==t.tool)return{ok:false,code:'select',tool:t.tool};
  if(Math.abs(st.x-t.x)<1.1){const side=st.x<t.x?1:st.x>t.x?-1:st.face;st.face=side;st.x=t.x-side*1.45;}else st.face=t.x>=st.x?1:-1;const hits=(st.toolHits[id]||0)+1;st.toolHits[id]=hits;st.action={id,tool:t.tool,kind:t.type,x:t.x,y:t.floor*8,started:st.clock,until:st.clock+.45};
  if(hits>=(t.hits||1))complete(s,id);return{ok:true,code:has(s,id)?'done':'working',hits,total:t.hits||1};
 }
 if(t.type==='weight'){const p=puzzle(s,id);if(st.carried?.id===id){if(!p.placed.includes(st.carried.index))p.placed.push(st.carried.index);st.carried=null;if(P.solved(p)){completeProblem(s,id);return{ok:true,code:'done'};}return{ok:true,code:'placed'};}if(p.placed.length){st.carried={id,index:p.placed.pop()};return{ok:true,code:'carried'};}return{ok:true,code:'weight',id};}
 if(t.type==='rule'&&t.tool===4){if(st.selectedHotbar!==4)return{ok:false,code:'select',tool:4};st.action={id,tool:4,kind:'scan',x:t.x,y:t.floor*8,started:st.clock,until:st.clock+.65};return{ok:true,code:'puzzle',id,delay:650};}
 if(t.type==='jump'){if(st.selectedHotbar!==2)return{ok:false,code:'select',tool:2};return{ok:true,code:'jump'};}
 return{ok:true,code:t.type==='sequence'?'sequence':'puzzle',id};
}
function submit(s,id,value){const desc=descriptor(s,id),st=session(s);if(!desc||has(s,id))return{ok:false,code:'complete'};const isBoss=id.startsWith('boss.');if(isBoss){const b=bossInfo(s);if(!b.available||b.next?.id!==id)return{ok:false,code:'order'};}else if(!canUse(s,desc).ok)return{ok:false,code:'order'};
 const p=puzzle(s,id);if(p.type==='equipment'){if(!Number.isInteger(value)||!owned(s,value))return{ok:false,code:'craft',tool:value};equip(s,value);}const result=P.press(p,value);if(result.solved)completeProblem(s,id);return result;
}
function completeProblem(s,id){
 const d=descriptor(s,id);if(!d||has(s,id))return false;
 if(id.startsWith('boss.')){const b=bossInfo(s);if(!b.available||b.next?.id!==id)return false;}else if(!canUse(s,d).ok)return false;
 if(!P.solved(puzzle(s,id)))return false;complete(s,id);
 const st=session(s);st.action={id,kind:'success',x:definition(s).boss.x,y:16,started:st.clock,until:st.clock+.65};
 if(id.startsWith('boss.')&&!bossInfo(s).next)finish(s);return true;
}
function finish(s){if(!fieldDone(s)||bossInfo(s).next||has(s,'clear'))return false;flag(s,'clear');reward(s,'clear',30,[0,0,0,0,1]);if(!s.completedChapters.includes(s.chapter))s.completedChapters.push(s.chapter);nextUnlocks(s);return true;}
function plateLayout(s,t){const p=puzzle(s,t.id),nums=p.sequence.slice(),r=P.rng(P.hash(session(s).seed+':plate:'+t.id));if(t.guided)nums.reverse();else for(let i=nums.length-1;i>0;i--){const j=r(i+1);[nums[i],nums[j]]=[nums[j],nums[i]];}return nums.map((n,i)=>({n,x:t.x+(i-1)*3,y:t.floor*8}));}
function move(s,h=0,v=0,jump=false,dt=1/60){
 const st=session(s),d=definition(s);dt=clamp(dt,0,.05);if(!dt)return;h=clamp(h,-1,1);v=clamp(v,-1,1);st.clock+=dt;
 const list=solids(s),hw=.38,height=1.65;st.grounded=!st.climb&&st.vy<=0&&list.some(f=>st.x+hw>f[0]&&st.x-hw<f[2]&&Math.abs(st.y-f[3])<.004);
 if(h){st.climb=null;st.face=h>0?1:-1;}
 if(v)for(const l of ladders(s)){if(Math.abs(st.x-l[0])>.7||st.y<l[1]-.1||st.y>l[2]+.1||(v>0&&st.y>=l[2]-.001)||(v<0&&st.y<=l[1]+.001))continue;st.climb=l;st.x=l[0];break;}
 if(jump&&st.grounded&&!st.climb){st.vy=tool(s,2)?13:9.5;st.grounded=false;if(tool(s,2))st.action={kind:'jump',tool:2,x:st.x,y:st.y,started:st.clock,until:st.clock+.6};}
 let tx=st.x+h*6*dt;
 for(const f of list){if(f[4]||st.y+height<=f[1]+.001||st.y>=f[3]-.001||(f[5]&&Math.sign(h)!==f[5]))continue;if(h>0&&st.x+hw<=f[0]+.001&&tx+hw>f[0])tx=Math.min(tx,f[0]-hw);if(h<0&&st.x-hw>=f[2]-.001&&tx-hw<f[2])tx=Math.max(tx,f[2]+hw);}
 st.x=clamp(tx,.4,d.width-.4);
 if(st.climb){const l=st.climb;st.vy=0;st.grounded=false;st.y=clamp(st.y+v*4.5*dt,l[1],l[2]);if(st.y>=l[2]&&v>0||st.y<=l[1]&&v<0){st.climb=null;st.grounded=true;st.checkpoint=[st.x,st.y];}}
 else{st.vy=Math.max(-18,st.vy-24*dt);let ny=st.y+st.vy*dt;st.grounded=false;for(const f of list){if(st.x+hw<=f[0]||st.x-hw>=f[2])continue;if(st.vy<=0&&st.y>=f[3]-.002&&ny<=f[3]){ny=Math.max(ny,f[3]);st.grounded=true;st.vy=0;}}st.y=ny;}
 if(st.y< -3){[st.x,st.y]=st.checkpoint;st.vy=0;st.grounded=true;st.climb=null;}
 const room=clamp(Math.floor((st.y+.2)/8),0,d.floors-1)*5+clamp(Math.floor(st.x/12),0,4);if(!st.visited.includes(room))st.visited.push(room);
 const active=currentTask(s);
 if(active?.type==='jump'&&tool(s,2)&&Math.abs(st.x-active.x)<1.9&&st.y>active.floor*8+1.8&&st.y<active.floor*8+5){complete(s,active.id);}
 if(active?.type==='sequence'&&canUse(s,active).ok){let contact=null;if(st.grounded)contact=plateLayout(s,active).find(p=>Math.abs(st.y-p.y)<.12&&Math.abs(st.x-p.x)<.62);const key=contact?active.id+':'+contact.n:null;
  if(contact&&key!==st.lastPlate){const p=puzzle(s,active.id),r=P.press(p,contact.n);st.plateEvent={id:active.id,n:contact.n,x:contact.x,y:contact.y,ok:r.ok,progress:p.input.length,started:st.clock,until:st.clock+1.1};if(r.solved)complete(s,active.id);}
  st.lastPlate=key;
 }else st.lastPlate=null;
}
function objects(s){const d=definition(s),st=session(s),all=[d.bench,...d.tasks,d.boss,...d.optional];for(const t of d.tasks.filter(t=>t.type==='weight'&&!has(s,t.id))){const p=puzzle(s,t.id);p.numbers.forEach((n,i)=>{if(!p.placed.includes(i)&&!(st.carried?.id===t.id&&st.carried.index===i))all.push({id:t.id+'.weight.'+i,kind:'weightCrate',label:n+'kg 상자',x:t.x-4+i*1.25,floor:t.floor,weight:n,index:i,parent:t.id});});}return all;}
function sanitizeInventory(raw){
 if(!raw||!Array.isArray(raw.items)||raw.items.length!==5||raw.items.some(n=>!Number.isInteger(n)||n<0||n>9999))return null;
 for(const k of ['owned','equipped'])if(!Array.isArray(raw[k])||raw[k].length!==5||raw[k].some(v=>typeof v!=='boolean'))return null;
 return{items:raw.items.slice(),owned:raw.owned.slice(),equipped:raw.equipped.map((v,i)=>v&&raw.owned[i]),ench:Array.from({length:5},(_,i)=>[-1,0,1].includes(raw.ench?.[i])?raw.ench[i]:-1),unlocked:Array.isArray(raw.unlocked)?[...new Set(raw.unlocked.filter(v=>Number.isInteger(v)&&v>=0&&v<10))]:[]};
}
function validFlags(raw){return Array.isArray(raw)&&raw.length<300&&raw.every(k=>typeof k==='string'&&k.length<100);}
function normalizeState(raw,seed,id){
 if(!raw||!numeric(raw.x)||!numeric(raw.y)||raw.x<0||raw.x>D.get(id).width||raw.y< -3||raw.y>26||!validFlags(raw.flags))return null;
 const out=freshState(seed,id);out.x=raw.x;out.y=Math.max(0,raw.y);out.seed=Number.isInteger(raw.seed)?raw.seed>>>0:out.seed;out.face=raw.face===-1?-1:1;out.flags=[...new Set(raw.flags)];out.visited=Array.isArray(raw.visited)?raw.visited.filter(v=>Number.isInteger(v)&&v>=0&&v<15):[];
 if(Array.isArray(raw.checkpoint)&&raw.checkpoint.length===2&&raw.checkpoint.every(n=>numeric(n))&&raw.checkpoint[0]>=.4&&raw.checkpoint[0]<59.6&&[0,8,16].includes(raw.checkpoint[1]))out.checkpoint=raw.checkpoint.slice();
 out.selectedHotbar=Number.isInteger(raw.selectedHotbar)&&raw.selectedHotbar>=0&&raw.selectedHotbar<5?raw.selectedHotbar:0;
 const desc=[...D.get(id).tasks,...D.get(id).boss.questions];
 for(const def of desc){if(['supply','repair','mine','shield','jump'].includes(def.type))continue;
  const r=raw.puzzles?.[def.id];if(!r)continue;const p=P.create(def.type,P.hash(out.seed+':'+def.id),def);
  if(p.rotations&&Array.isArray(r.rotations)&&r.rotations.length===p.rotations.length&&r.rotations.every(n=>Number.isInteger(n)&&n>=0&&n<(p.type==='laser'?2:4)))p.rotations=r.rotations.slice();
  if(p.input&&Array.isArray(r.input)&&r.input.length<=p.sequence.length&&r.input.every((n,i)=>n===p.sequence[i]))p.input=r.input.slice();
  if(p.placed&&Array.isArray(r.placed))p.placed=[...new Set(r.placed.filter(i=>Number.isInteger(i)&&i>=0&&i<p.numbers.length))];
  if(p.type==='number'&&r.type==='number'&&typeof r.prompt==='string'&&r.prompt.length<=160&&Number.isInteger(r.answer)&&r.answer>=0&&r.answer<=999){p.prompt=r.prompt;p.answer=r.answer;p.hint=typeof r.hint==='string'?r.hint.slice(0,160):p.hint;}
  if(p.type==='sokoban'){
   const validPoint=v=>Array.isArray(v)&&v.length===2&&v.every(Number.isInteger)&&v[0]>=0&&v[0]<p.width&&v[1]>=0&&v[1]<p.height&&!p.walls.some(w=>w[0]===v[0]&&w[1]===v[1]);
   const validBoard=b=>b&&validPoint(b.player)&&Array.isArray(b.boxes)&&b.boxes.length===p.boxes.length&&b.boxes.every(validPoint)&&new Set(b.boxes.map(v=>v.join(','))).size===p.boxes.length&&!b.boxes.some(v=>v[0]===b.player[0]&&v[1]===b.player[1]);
   if(validBoard(r)){p.player=r.player.slice();p.boxes=r.boxes.map(v=>v.slice());p.history=Array.isArray(r.history)?r.history.slice(-150).filter(validBoard).map(b=>({player:b.player.slice(),boxes:b.boxes.map(v=>v.slice())})):[];}
  }
  // Solved flags are authoritative; malformed unfinished layouts use the safe authored board.
  if(out.flags.includes(def.id))p.solved=true;out.puzzles[def.id]=p;
 }
 if(raw.toolHits&&typeof raw.toolHits==='object')for(const t of desc){if(t.hits&&Number.isInteger(raw.toolHits[t.id]))out.toolHits[t.id]=clamp(raw.toolHits[t.id],0,t.hits);}
 // Legacy numeric prompt is retained instead of rerolled after a reload.
 if(raw.question&&!raw.question.solved&&typeof raw.question.id==='string'){
  const q=raw.question,def=desc.find(t=>t.id===q.id);if(def?.type==='number'&&typeof q.prompt==='string'&&q.prompt.length<=160&&Number.isInteger(q.answer)&&q.answer>=0&&q.answer<=999)out.puzzles[q.id]={...P.create('number',P.hash(out.seed+':'+q.id),def),prompt:q.prompt,answer:q.answer};
 }
 return out;
}
function restore(text){
 try{if(typeof text!=='string'||text.length>500000)return null;const raw=JSON.parse(text);if(!raw||![1,2,3].includes(raw.v)||!Number.isInteger(raw.seed))return null;
  const id=raw.v===1?ids[0]:raw.chapter;if(!D.get(id))return null;const inv=sanitizeInventory(raw.v===1?raw:raw.inventory);if(!inv)return null;
  const s=fresh(raw.seed);s.chapter=id;s.inventory=inv;s.xp=raw.v===2?raw.campaignXp:raw.xp;if(!Number.isInteger(s.xp)||s.xp<0||s.xp>99999)return null;
  s.completedChapters=Array.isArray(raw.completedChapters)?[...new Set(raw.completedChapters.filter(x=>ids.includes(x)))]:[];
  s.states={};
  if(raw.v===3){for(const c of ids){if(raw.states?.[c]){const st=normalizeState(raw.states[c],s.seed,c);if(!st)return null;s.states[c]=st;}}if(!s.states[id])return null;}
  else{const st=normalizeState(raw.v===1?raw:raw.chapterState||raw,s.seed,id);if(!st)return null;s.states[id]=st;}
  if(s.states[id].flags.includes('clear')&&!s.completedChapters.includes(id))s.completedChapters.push(id);
  // Old saves only retained the currently played chapter. Recover completed worlds from completion records.
  for(const c of s.completedChapters){if(!s.states[c]){const st=freshState(s.seed,c);st.flags=D.get(c).tasks.map(t=>t.id).concat(D.get(c).boss.questions.map(q=>q.id),'clear');s.states[c]=st;}}
  for(const c of s.completedChapters){const st=s.states[c];for(const k of D.get(c).tasks.map(t=>t.id).concat(D.get(c).boss.questions.map(q=>q.id),'clear'))if(!st.flags.includes(k))st.flags.push(k);}nextUnlocks(s);if(!s.unlockedChapters.includes(id))return null;return s;
 }catch{return null;}
}
function serialize(s){return JSON.stringify(s,(key,value)=>['action','plateEvent','lastPlate','climb','carried'].includes(key)?null:value);}
return{fresh,freshCampaign:fresh,session,definition,has,tool,owned,costs,recipes,startChapter,reward,craft,equip,enchant,objective,waypoint,currentTask,fieldDone,bossInfo,descriptor,puzzle,canUse,interact,submit,completeProblem,finish,objects,plateLayout,move,solids,ladders,restore,serialize,rng:P.rng,hash:P.hash};
});
