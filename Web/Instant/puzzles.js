/* Deterministic puzzles: rule evaluation never depends on UI or hidden target rotations. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.MMPuzzles=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){
'use strict';
const dirs=[[0,-1],[1,0],[0,1],[-1,0]];
function hash(text){let h=2166136261;for(const c of String(text))h=Math.imul(h^c.charCodeAt(0),16777619);return h>>>0;}
function rng(seed){let x=(seed>>>0)||1831565813;return n=>{x^=x<<13;x^=x>>>17;x^=x<<5;return(x>>>0)%n;};}
function copy(x){return JSON.parse(JSON.stringify(x));}
function number(seed,mode='add'){
 const r=rng(seed),small=()=>2+r(7);let a,b,c,answer,prompt,hint;
 if(mode==='mul'){a=[2,3,5][r(3)];b=small();answer=a*b;prompt=`${a} × ${b} = ?`;hint=`${a}개씩 ${b}묶음을 생각해요.`;}
 else if(mode==='div'){b=[2,3,5][r(3)];answer=small();a=b*answer;prompt=`${a} ÷ ${b} = ?`;hint=`${a}개를 ${b}개씩 묶어 보세요.`;}
 else if(mode==='missing'){a=4+r(12);answer=small();b=a+answer;prompt=`${a} + □ = ${b}`;hint=`${a}에서 ${b}가 되려면 몇 개 더 필요할까요?`;}
 else if(mode==='pattern'){a=1+r(5);b=[2,3,5][r(3)];answer=a+3*b;prompt=`${a}, ${a+b}, ${a+2*b}, □`;hint=`앞뒤 숫자가 ${b}씩 늘어나요.`;}
 else if(mode==='twoStep'){a=8+r(12);b=small();c=2+r(5);answer=a+b-c;prompt=`${a} + ${b} − ${c} = ?`;hint=`먼저 ${a} + ${b}를 구하고 ${c}를 빼요.`;}
 else if(mode==='divideAdd'){b=[2,3,5][r(3)];c=small();a=b*small();answer=a/b+c;prompt=`(${a} ÷ ${b}) + ${c} = ?`;hint=`먼저 ${a} ÷ ${b}를 구한 뒤 ${c}를 더해요.`;}
 else if(mode==='sub'){b=small();answer=small();a=answer+b;prompt=`${a} − ${b} = ?`;hint=`${a}에서 ${b}를 빼요.`;}
 else{a=11+r(17);b=3+r(8);answer=a+b;prompt=`${a} + ${b} = ?`;hint='10을 먼저 만들고 남은 수를 더해요.';}
 return{type:'number',mode,prompt,answer,a,b,c,hint,solved:false,attempts:0};
}
function create(type,seed=1,opts={}){
 const r=rng(seed),p={type,seed,attempts:0,solved:false};
 if(type==='number')return number(seed,opts.problem||'add');
 if(type==='rule'){
  const sets=[{prompt:'10보다 크고 20보다 작은 짝수를 골라요.',options:[9,16,21],answer:16,hint:'짝수는 2개씩 나누어 남지 않는 수예요.'},{prompt:'3씩 커져요: 3, 6, 9, □. 다음 수는?',options:[10,12,14],answer:12,hint:'앞의 수에 3을 더해 보세요.'},{prompt:'2개씩 묶어도 남지 않고, 15보다 작은 수는?',options:[11,14,18],answer:14,hint:'짝수인지 먼저 보고, 15보다 작은지 확인해요.'}];
  Object.assign(p,copy(sets[r(sets.length)]));for(let i=2;i>0;i--){const j=r(i+1);[p.options[i],p.options[j]]=[p.options[j],p.options[i]];}return p;
 }
 if(type==='memory'||type==='order'||type==='sequence'){p.sequence=opts.sequence?opts.sequence.slice():Array.from({length:Math.max(3,Math.min(5,opts.length||3))},()=>r(4));p.input=[];p.last=null;return p;}
 if(type==='equipment'){p.sequence=[0,1,2,3,4];p.input=[];return p;}
 if(type==='weight'){p.numbers=(opts.numbers||[3,5,2]).slice();p.target=opts.target||8;p.placed=[];return p;}
 if(type==='sokoban'){
  const hard=(opts.level||0)>=6;p.width=hard?6:5;p.height=5;p.walls=[];
  for(let y=0;y<p.height;y++)for(let x=0;x<p.width;x++)if(!x||!y||x===p.width-1||y===p.height-1)p.walls.push([x,y]);
  p.player=hard?[1,2]:[1,3];p.boxes=hard?[[2,2],[2,3]]:[[2,2]];p.goals=hard?[[4,2],[4,3]]:[[3,2]];p.initial={player:copy(p.player),boxes:copy(p.boxes)};p.history=[];return p;
 }
 if(type==='pipe'){
  p.width=p.height=3;p.source=[-1,1];p.target=[3,1];p.cells=Array(9).fill(0);
  const bottom=!!(seed%2),indices=bottom?[3,6,7,8,5]:[3,0,1,2,5];
  for(const i of indices)p.cells[i]=i===indices[2]?5:3;
  p.rotations=Array.from({length:9},()=>r(4));if(pipeTrace(p).solved)p.rotations[3]=(p.rotations[3]+1)%4;return p;
 }
 if(type==='laser'){
  p.width=p.height=5;const advanced=(opts.level||0)>=4;
  p.source=advanced?[-1,4]:[-1,3];p.direction=1;p.target=advanced?[4,0]:[4,1];p.mirrors=advanced?[[2,4],[2,2],[4,2]]:[[2,3],[2,1]];p.walls=[];p.rotations=p.mirrors.map(()=>r(2));if(laserTrace(p).solved)p.rotations[0]^=1;return p;
 }
 throw new Error('Unknown puzzle type: '+type);
}
function ports(mask,rotation){for(let i=0;i<rotation;i++)mask=((mask<<1)&15)|(mask>>3);return mask;}
function pipeTrace(p){
 if(!p||p.type!=='pipe'||!Array.isArray(p.rotations)||p.rotations.length!==9)return{solved:false,reached:[]};
 const reached=[],queue=[];const first=p.source[1]*p.width;
 if(!(ports(p.cells[first],p.rotations[first])&8))return{solved:false,reached};queue.push(first);let ok=false;
 while(queue.length){const i=queue.shift();if(reached.includes(i))continue;reached.push(i);const x=i%p.width,y=Math.floor(i/p.width),mask=ports(p.cells[i],p.rotations[i]);
  for(let d=0;d<4;d++){if(!(mask&(1<<d)))continue;const nx=x+dirs[d][0],ny=y+dirs[d][1];
   if(nx===p.target[0]&&ny===p.target[1]){ok=true;continue;}
   if(nx<0||nx>=p.width||ny<0||ny>=p.height)continue;const j=ny*p.width+nx;
   if(ports(p.cells[j],p.rotations[j])&(1<<((d+2)%4)))queue.push(j);
  }
 }
 return{solved:ok,reached};
}
function laserTrace(p){
 if(!p||p.type!=='laser')return{solved:false,path:[]};let[x,y]=p.source,d=p.direction;const path=[[x,y]],seen=new Set();
 for(let n=0;n<p.width*p.height*4+4;n++){
  x+=dirs[d][0];y+=dirs[d][1];path.push([x,y]);if(x<0||x>=p.width||y<0||y>=p.height)return{solved:false,path};
  if(x===p.target[0]&&y===p.target[1])return{solved:true,path};
  if(p.walls.some(w=>w[0]===x&&w[1]===y))return{solved:false,path};const key=x+','+y+','+d;if(seen.has(key))return{solved:false,path};seen.add(key);
  const i=p.mirrors.findIndex(m=>m[0]===x&&m[1]===y);if(i>=0)d=(p.rotations[i]===0?[1,0,3,2]:[3,2,1,0])[d];
 }
 return{solved:false,path};
}
function same(a,b){return a[0]===b[0]&&a[1]===b[1];}
function solved(p){
 if(!p)return false;
 if(p.type==='pipe')return pipeTrace(p).solved;
 if(p.type==='laser')return laserTrace(p).solved;
 if(p.type==='sokoban')return p.goals.every(g=>p.boxes.some(b=>same(b,g)));
 if(p.type==='weight')return p.placed.reduce((n,i)=>n+p.numbers[i],0)===p.target;
 if(['memory','order','sequence','equipment'].includes(p.type))return p.input.length===p.sequence.length&&p.input.every((v,i)=>v===p.sequence[i]);
 return !!p.solved;
}
function press(p,value){
 if(solved(p))return{ok:false,duplicate:true};
 if(['number','rule'].includes(p.type)){const text=String(value);if(!/^\d{1,3}$/.test(text))return{ok:false};const ok=Number(text)===p.answer;p.solved=ok;if(!ok)p.attempts++;return{ok,solved:ok};}
 if(['memory','order','sequence','equipment'].includes(p.type)){const ok=value===p.sequence[p.input.length];if(ok)p.input.push(value);else{p.input=[];p.attempts++;}p.last={value,ok};return{ok,solved:solved(p),progress:p.input.length,next:p.sequence[p.input.length]};}
 return{ok:false};
}
function rotate(p,index){if(solved(p))return false;if(p.type==='pipe'&&p.cells[index]){p.rotations[index]=(p.rotations[index]+1)%4;return true;}if(p.type==='laser'&&Number.isInteger(index)&&index>=0&&index<p.rotations.length){p.rotations[index]^=1;return true;}return false;}
function sokobanMove(p,direction){
 if(p.type!=='sokoban'||solved(p)||!dirs[direction])return false;const delta=dirs[direction],next=[p.player[0]+delta[0],p.player[1]+delta[1]],inside=v=>v[0]>=0&&v[0]<p.width&&v[1]>=0&&v[1]<p.height&&!p.walls.some(w=>same(w,v));
 if(!inside(next))return false;const idx=p.boxes.findIndex(b=>same(b,next)),after=[next[0]+delta[0],next[1]+delta[1]];
 if(idx>=0&&(!inside(after)||p.boxes.some(b=>same(b,after))))return false;
 p.history.push({player:copy(p.player),boxes:copy(p.boxes)});if(p.history.length>150)p.history.shift();if(idx>=0)p.boxes[idx]=after;p.player=next;return true;
}
function reset(p){if(p.type==='sokoban'){p.player=copy(p.initial.player);p.boxes=copy(p.initial.boxes);p.history=[];}else if(p.input)p.input=[];else if(p.placed)p.placed=[];p.solved=false;}
function undo(p){if(p.type!=='sokoban'||!p.history.length||solved(p))return false;const s=p.history.pop();p.player=s.player;p.boxes=s.boxes;return true;}
function solvePipe(p){const xs=p.cells.map((m,i)=>m?i:-1).filter(i=>i>=0),trial=copy(p);function visit(n){if(n===xs.length)return pipeTrace(trial).solved?trial.rotations.slice():null;for(let r=0;r<4;r++){trial.rotations[xs[n]]=r;const a=visit(n+1);if(a)return a;}return null;}return visit(0);}
function solveLaser(p){const t=copy(p);for(let mask=0;mask<1<<t.mirrors.length;mask++){t.rotations=t.mirrors.map((_,i)=>(mask>>i)&1);if(laserTrace(t).solved)return t.rotations.slice();}return null;}
function solveSokoban(p){const queue=[{p:copy(p),moves:[]}],seen=new Set();while(queue.length&&seen.size<12000){const s=queue.shift(),key=JSON.stringify([s.p.player,s.p.boxes.slice().sort((a,b)=>a[0]*10+a[1]-b[0]*10-b[1])]);if(seen.has(key))continue;seen.add(key);if(solved(s.p))return s.moves;for(let d=0;d<4;d++){const t=copy(s.p);t.history=[];if(sokobanMove(t,d))queue.push({p:t,moves:s.moves.concat(d)});}}return null;}
return{hash,rng,create,number,solved,press,rotate,ports,pipeTrace,laserTrace,sokobanMove,reset,undo,solvePipe,solveLaser,solveSokoban};
});
