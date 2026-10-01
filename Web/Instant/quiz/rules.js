/* 15.2: original reasoning generators, bounded recent-question history, safe resumable variants. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.MMQuiz=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){
'use strict';
const clone=x=>JSON.parse(JSON.stringify(x));
function hash(s){let h=2166136261;for(const c of String(s))h=Math.imul(h^c.charCodeAt(0),16777619);return h>>>0;}
function rng(seed){let x=(seed>>>0)||1831565813;return n=>{x^=x<<13;x^=x>>>17;x^=x<<5;return(x>>>0)%n;};}
function shuffle(xs,r){for(let i=xs.length-1;i>0;i--){let j=r(i+1);[xs[i],xs[j]]=[xs[j],xs[i]];}return xs;}
function profile(n=1){n=Math.max(1,Math.min(15,Number(n)||1));const tier=Math.floor((n-1)/3);return{chapter:n,tier,bossCount:5+tier,cardCount:[4,5,6,7,9][tier],cardSteps:[1,2,2,3,3][tier],tables:[2,3,5],factorMax:Math.min(8,3+Math.floor((n-1)/2)),addMax:10+2*n,memory:Math.min(5,3+Math.floor((n-1)/6))};}
function cardStates(cards,operations){const out=[cards.slice()];for(const op of operations){const a=out.at(-1).slice(),i=op.from-1,j=op.to-1;if(!Number.isInteger(i)||i<0||i>=a.length)throw Error('Invalid card position');if(op.kind==='swap'){if(!Number.isInteger(j)||j<0||j>=a.length)throw Error('Invalid swap');[a[i],a[j]]=[a[j],a[i]];}else if(op.kind==='front')a.unshift(a.splice(i,1)[0]);else if(op.kind==='end')a.push(a.splice(i,1)[0]);else throw Error('Invalid card action');out.push(a);}return out;}
function cardAnswer(before,after,ask,position){const same=before.filter((n,i)=>after[i]===n);if(ask==='count')return same.length;if(ask==='difference'){if(same.length<2)throw Error('Need two unchanged cards');return Math.max(...same)-Math.min(...same);}return after[position-1];}
function cards(seed,chapter){const r=rng(seed),g=profile(chapter),n=g.cardCount;for(let attempt=0;attempt<300;attempt++){
 const start=1+r(3),step=g.tier>=2?1+r(2):1,list=Array.from({length:n},(_,i)=>start+i*step),ops=[];
 for(let k=0;k<g.cardSteps;k++){let kind=g.tier===0?'swap':['front','end','swap'][r(3)],from=1+r(n),to=1+r(n);if(kind==='swap'&&to===from)to=to%n+1;if(kind==='front'&&from===1)from=2;if(kind==='end'&&from===n)from=n-1;ops.push({kind,from,...(kind==='swap'?{to}:{})});}
 const end=cardStates(list,ops).at(-1),unchanged=list.filter((v,i)=>end[i]===v);if(unchanged.length===n)continue;
 const ask=g.tier>=3?'difference':g.tier>=1&&r(2)?'count':'position',position=1+r(n);if(ask==='difference'&&unchanged.length<2)continue;
 const prompt=ask==='difference'?'모든 활동 뒤, 처음과 같은 자리에 남은 카드 중 가장 큰 수와 가장 작은 수의 차이는 얼마일까요?':ask==='count'?'모든 활동 뒤, 처음과 같은 자리에 남은 카드는 몇 장일까요?':`모든 활동 뒤, 왼쪽에서 ${position}번째 카드의 수는 무엇일까요?`;
 return{type:'cards',seed,cards:list,operations:ops,ask,position,prompt,answer:cardAnswer(list,end,ask,position),unchanged,hint:'카드의 수가 아니라 지금 줄에서 왼쪽부터 몇 번째인지 세어요.',solved:false,attempts:0};
 }throw Error('No card puzzle generated');}
function symbols(seed,chapter){const r=rng(seed),g=profile(chapter),a=1+r(3+g.tier),k=1+r(3),b=g.tier===0?1+r(5):a+k,c=2+r(5+g.tier),variables=g.tier<2?2:3;
 const clues=g.tier===0?[{left:['0','0'],right:2*a},{left:['0','1'],right:a+b}]:[{left:['0',k],right:'1'},{left:['0','1'],right:a+b}];
 if(variables===3)clues.push({left:[g.tier===2?'1':'0','2'],right:(g.tier===2?b:a)+c});
 const ask=variables===3?2:g.tier===0?1:(seed%2),values=[a,b,c];
 return{type:'symbols',seed,variables,clues,ask,prompt:'같은 모양은 같은 수예요. 아래 단서를 함께 보고 물음표의 값을 찾아요.',answer:values[ask],hint:g.tier===0?'별 두 개를 모은 수부터 살펴봐요. 별 하나의 수를 찾은 뒤 다음 단서에 넣어요.':`두 번째 단서의 합에서 ${k}를 빼면 별 두 개를 모은 수가 돼요.`,explanation:values.slice(0,variables),solved:false,attempts:0};}
function arithmetic(seed,mode,chapter){const r=rng(seed),g=profile(chapter),small=()=>2+r(g.factorMax-1);let a,b,c,answer,prompt,hint;
 if(mode==='mul'){a=g.tables[r(3)];b=small();answer=a*b;prompt=`${a} × ${b} = ?`;hint=`${a}개씩 ${b}묶음을 세어요.`;}
 else if(mode==='div'||mode==='divideAdd'){b=g.tables[r(3)];answer=small();a=b*answer;c=mode==='divideAdd'?1+r(5):0;answer+=c;prompt=c?`(${a} ÷ ${b}) + ${c} = ?`:`${a} ÷ ${b} = ?`;hint=`${a}개를 ${b}개씩 나눈 다음${c?' '+c+'를 더해요.':' 묶음 수를 세어요.'}`;}
 else if(mode==='missing'){a=2+r(Math.min(18,g.addMax));answer=small();b=a+answer;prompt=`${a} + □ = ${b}`;hint=`${a}에서 ${b}가 되려면 몇 개 더 필요할까요?`;}
 else if(mode==='pattern'){a=1+r(9);b=[1,2,3,5][r(g.tier<2?3:4)];answer=a+3*b;prompt=`${a}, ${a+b}, ${a+2*b}, □`;hint='이웃한 두 수의 차이를 찾아요.';}
 else if(mode==='sub'){b=small();answer=2+r(g.addMax);a=answer+b;prompt=`${a} − ${b} = ?`;hint=`${a}에서 ${b}를 빼요.`;}
 else if(mode==='twoStep'){a=5+r(g.addMax-4);b=small();c=1+r(Math.min(a,6));answer=a+b-c;prompt=`${a} + ${b} − ${c} = ?`;hint='왼쪽 계산부터 한 번씩 차근차근 해요.';}
 else{mode='add';a=2+r(g.addMax-1);b=2+r(Math.min(12,g.addMax)-1);answer=a+b;prompt=`${a} + ${b} = ?`;hint='10을 만들 수 있는 수를 먼저 묶어 보세요.';}
 return{type:'number',seed,mode,a,b,c,answer,prompt,hint,attempts:0,solved:false};}
function rule(seed,chapter){const r=rng(seed),g=profile(chapter),step=[2,3,5][r(g.tier<1?2:3)],base=1+r(g.addMax-2),answer=base+3*step;
 const condition=g.tier>=2&&r(2);let p;if(condition){const lo=2+2*r(10),ans=lo+2,odd=lo+1,out=lo+6;p={prompt:`${lo}보다 크고 ${lo+5}보다 작은 짝수를 골라요.`,answer:ans,options:[ans,odd,out],hint:'먼저 두 수 사이인지, 그다음 짝수인지 확인해요.'};}
 else p={prompt:`${base}, ${base+step}, ${base+2*step}, □ — 다음 수를 골라요.`,answer,options:[answer,answer+1,answer-1],hint:'앞뒤 수가 얼마씩 달라지는지 살펴봐요.'};
 return{type:'rule',seed,...p,options:shuffle(p.options,r),attempts:0,solved:false};}
function fingerprint(p){let key;
 if(p.type==='number')key=p.mode==='add'||p.mode==='mul'?[p.type,p.mode,...[p.a,p.b].sort((a,b)=>a-b)]:[p.type,p.prompt];
 else if(p.type==='cards')key=[p.type,p.cards,p.operations,p.ask,p.ask==='position'?p.position:0];
 else if(p.type==='symbols')key=[p.type,p.clues,p.ask];
 else if(p.type==='rule')key=[p.type,p.prompt,p.answer];
 else if(p.sequence)key=[p.type,p.sequence];
 else if(p.type==='pipe')key=[p.type,p.cells,p.source,p.target];
 else if(p.type==='laser')key=[p.type,p.mirrors,p.source,p.target];
 else if(p.type==='sokoban')key=[p.type,p.initial,p.goals];
 else key=[p.type,p.seed];return JSON.stringify(key);
}
function boardVariant(P,p,seed,opts){const r=rng(hash('board:'+seed)),g=profile(opts.chapterOrder||1);
 if(p.type==='pipe'){
  const sr=r(3),tr=r(3),paths=[],limit=g.tier<1?5:7;
  function visit(xs){const [x,y]=xs.at(-1);if(x===2&&y===tr){if(xs.length>=3)paths.push(xs);return;}if(xs.length>=limit)return;for(const [dx,dy] of [[1,0],[0,1],[0,-1],[-1,0]]){const a=x+dx,b=y+dy;if(a>=0&&a<3&&b>=0&&b<3&&!xs.some(v=>v[0]===a&&v[1]===b))visit([...xs,[a,b]]);}}
  visit([[0,sr]]);const route=paths[r(paths.length)],dirs=[[0,-1],[1,0],[0,1],[-1,0]];p.source=[-1,sr];p.target=[3,tr];p.cells=Array(9).fill(0);
  route.forEach(([x,y],i)=>{const ends=[route[i-1]||p.source,route[i+1]||p.target];p.cells[y*3+x]=ends.reduce((mask,[a,b])=>mask|(1<<dirs.findIndex(([dx,dy])=>dx===a-x&&dy===b-y)),0);});p.rotations=Array.from({length:9},()=>r(4));for(let k=0;k<4&&P.pipeTrace(p).solved;k++)p.rotations[sr*3]=(p.rotations[sr*3]+1)%4;
 }else if(p.type==='laser'){
  const low=3+r(2),top=r(2),a=1+r(2),b=a+1+r(4-a),mid=top+1+r(low-top-1);p.source=[-1,low];p.direction=1;p.target=g.tier===0?[4,top]:[b,top];p.mirrors=g.tier===0?[[a,low],[a,top]]:[[a,low],[a,mid],[b,mid]];p.rotations=p.mirrors.map(()=>r(2));if(P.laserTrace(p).solved)p.rotations[0]^=1;
 }else if(p.type==='sokoban'){
  const flipX=!!r(2),flipY=!!r(2),f=([x,y])=>[flipX?p.width-1-x:x,flipY?p.height-1-y:y];p.player=f(p.player);p.boxes=p.boxes.map(f);p.goals=p.goals.map(f);p.walls=p.walls.map(f);p.initial={player:p.player.slice(),boxes:p.boxes.map(v=>v.slice())};
 }return p;
}
function install(P){if(P.quizInstalled)return;const create=P.create,press=P.press;P.legacyCreate=create;P.quizInstalled=true;
 P.create=function(type,seed=1,opts={}){const n=opts.chapterOrder||1;if(type==='cards')return cards(seed,n);if(type==='symbols')return symbols(seed,n);if(type==='number'||type==='monster')return{...arithmetic(seed,opts.problem||'add',n),...(type==='monster'?{encounter:true}:{})};if(type==='rule')return rule(seed,n);
  const p=create(type,seed,opts);if(type==='equipment')shuffle(p.sequence,rng(seed));return boardVariant(P,p,seed,opts);};
 P.press=function(p,value){if(['cards','symbols'].includes(p?.type)){if(p.solved)return{ok:false,duplicate:true};const ok=/^\d{1,3}$/.test(String(value))&&Number(value)===p.answer;p.solved=ok;if(!ok)p.attempts++;return{ok,solved:ok};}return press(p,value);};
}
function issue(P,s,st,desc){s.quizBook=s.quizBook||{recent:[]};const recent=s.quizBook.recent,opts={...desc,chapterOrder:Number(st.chapterId.slice(-2))};let p,key,chosen;
 for(let i=0;i<256;i++){chosen=hash(st.seed+':'+desc.id+':quiz-v2:'+i);p=P.create(desc.type,chosen,opts);key=fingerprint(p);if(!recent.includes(key))break;}
 p.quizVersion=2;p.quizSeed=chosen;p.quizFingerprint=key;recent.push(key);if(recent.length>128)recent.splice(0,recent.length-128);return p;
}
function restoreBase(P,def,raw,seed,chapterOrder){if(raw.quizVersion===2&&Number.isInteger(raw.quizSeed)&&raw.quizSeed>=0&&raw.quizSeed<=0xffffffff){const p=P.create(def.type,raw.quizSeed,{...def,chapterOrder});p.quizVersion=2;p.quizSeed=raw.quizSeed;p.quizFingerprint=fingerprint(p);return p;}return P.legacyCreate(def.type,hash(seed+':'+def.id),def);}
function restoreBook(raw){return{recent:Array.isArray(raw?.recent)?raw.recent.filter(x=>typeof x==='string'&&x.length<3000).slice(-128):[]};}
function expand(chapters){for(const d of chapters){const g=profile(d.order);for(const t of [...d.tasks,...d.boss.questions,...d.encounters])t.chapterOrder=d.order;
 // Additional reasoning is on a reachable lane, not a menu-only promise.
 if(!d.tasks.some(t=>t.id==='thinking.beacon'))d.tasks.push({id:'thinking.beacon',type:d.order%2?'cards':'symbols',kind:'terminal',label:'모모의 생각 장치',x:32,floor:1,chapterOrder:d.order,text:'생각 장치에서 카드나 모양의 규칙을 풀어요.'});
 for(const q of d.boss.questions)q.prompt=({number:'이번 숫자 문제를 함께 풀어 줄래?',rule:'단서를 보고 조건에 맞는 답을 골라 줘.',memory:'빛의 순서를 기억해 줘. 다시 보아도 괜찮아.',order:'표시된 순서대로 신호를 눌러 줘.',equipment:'상황에 맞는 도구를 골라 줘.',laser:'빛이 코어까지 가도록 거울을 돌려 줘.',pipe:'입구에서 도착점까지 관을 이어 줘.',sokoban:'화물 상자를 별 칸으로 옮겨 줘.'})[q.type]||q.prompt;
 const extra=['symbols','cards','rule','number','cards','symbols'];let i=0;while(d.boss.questions.length<g.bossCount){const type=extra[i];d.boss.questions.push({id:'boss.thinking.'+i,type,chapterOrder:d.order,problem:d.order>=10?'twoStep':d.order>=4?'missing':'add',prompt:type==='cards'?'카드를 옮긴 뒤 무엇이 달라지는지 찾아 줘.':type==='symbols'?'같은 모양의 숨은 수를 알아내 줄래?':'한 가지씩 생각해 다음 답을 찾아 줘.'});i++;}
 }}
return{profile,cardStates,cardAnswer,cards,symbols,arithmetic,rule,fingerprint,install,issue,restoreBase,restoreBook,expand};
});
