/* Navigation uses the shipped movement/collision loop. Only answers are supplied by an independent solver. */
const test=require('node:test'),a=require('node:assert/strict'),vm=require('node:vm'),{build}=require('./build.cjs');
function engine(){const ctx={console};ctx.globalThis=ctx;vm.createContext(ctx);for(const code of build().scripts){if(code.includes('Mobile UI for'))break;vm.runInContext(code,ctx);}return ctx;}
function walk(C,s,x,floor){for(let i=0;i<4000;i++){const st=C.session(s),f=Math.max(0,Math.min(2,Math.floor((st.y+.1)/8)));if(Math.abs(st.y-floor*8)<.07&&Math.abs(st.x-x)<.25){C.move(s,0,0,false,1/60);return;}if(st.climb){C.move(s,0,floor*8>st.y?1:-1,false,1/60);continue;}if(f!==floor){const ladder=C.ladders(s).find(l=>floor>f?l[1]===f*8:l[2]===f*8);a.ok(ladder,'ladder exists');if(Math.abs(st.x-ladder[0])>.15)C.move(s,Math.sign(ladder[0]-st.x),0,false,1/60);else C.move(s,0,floor>f?1:-1,false,1/60);}else C.move(s,Math.sign(x-st.x),0,false,1/60);}a.fail(`Unreachable ${s.chapter}: ${C.session(s).x},${C.session(s).y} -> ${x},${floor*8}`);}
function solve(C,P,M,s,id){const p=C.puzzle(s,id);if(M.isType(p.type)){
 if(p.type==='platform'){for(let i=0;i<p.planks.length;i++){const [l,r]=p.docks[i],dx=r[0]-l[0],dy=r[1]-l[1];M.press(p,{kind:'rotate',index:i,angle:Math.atan2(dy,dx)*180/Math.PI});M.press(p,{kind:'length',index:i,length:Math.hypot(dx,dy)});M.press(p,{kind:'move',index:i,x:(l[0]+r[0])/2,y:(l[1]+r[1])/2});}}
 else if(p.type==='pulley'){for(let i=0;i<p.load+1;i++)M.press(p,{kind:'weight',delta:1});M.press(p,{kind:'brake'});for(let i=0;i<1200&&p.height<p.targetHeight;i++)M.tick(p,1/60);M.press(p,{kind:'brake'});M.press(p,{kind:'weight',delta:-1});}
 else{for(let i=0;i<2000&&!M.ready(p);i++)M.tick(p,1/60);M.press(p,{kind:'brake'});}a.ok(M.solved(p),id);a.ok(C.completeProblem(s,id));return;
 }
 if(['number','rule','cards','symbols'].includes(p.type)){a.ok(C.submit(s,id,String(p.answer)).solved,id);return;}
 if(['memory','order','sequence','equipment'].includes(p.type)){for(const value of p.sequence.slice(p.input.length))a.ok(C.submit(s,id,value).ok,id);return;}
 if(p.type==='pipe'){p.rotations=P.solvePipe(p);a.ok(p.rotations,id);}
 else if(p.type==='laser'){p.rotations=P.solveLaser(p);a.ok(p.rotations,id);}
 else if(p.type==='sokoban'){const moves=P.solveSokoban(p);a.ok(moves,id);moves.forEach(d=>P.sokobanMove(p,d));}
 else if(p.type==='weight'){let found=false;for(let mask=0;mask<1<<p.numbers.length;mask++){p.placed=p.numbers.map((_,i)=>mask&(1<<i)?i:-1).filter(i=>i>=0);if(P.solved(p)){found=true;break;}}a.ok(found,id);}
 else a.fail('Unhandled puzzle '+p.type);a.ok(C.completeProblem(s,id),id);
}
function advance(C,P,M,s){for(let j=0;j<45;j++)C.move(s,0,0,false,1/60);const o=C.objective(s);if(o.kind==='craft'){walk(C,s,18,0);a.ok(C.craft(s,o.tool,C.recipes[o.tool]).ok);return;}if(o.kind==='equip'){a.ok(C.equip(s,o.tool));return;}
 walk(C,s,o.x,o.floor);if(o.kind==='boss'){const r=C.interact(s,'golem');a.equal(r.code,'boss');solve(C,P,M,s,r.question.id);return;}
 const t=C.currentTask(s);if(t.tool!==undefined)C.equip(s,t.tool);let r=C.interact(s,t.id);a.ok(r.ok,JSON.stringify(r));
 if(r.code==='working'){let guard=0;while(!C.has(s,t.id)&&guard++<8){for(let j=0;j<30;j++)C.move(s,0,0,false,1/60);r=C.interact(s,t.id);a.ok(r.ok);}a.ok(C.has(s,t.id));}
 else if(r.code==='jump'){C.move(s,0,0,true,1/60);for(let j=0;j<90;j++)C.move(s,0,0,false,1/60);a.ok(C.has(s,t.id));}
 else if(['puzzle','sequence','weight'].includes(r.code))solve(C,P,M,s,t.id);
}
test('all 15 chapters remain reachable with new field quiz and increased boss budgets',()=>{const {MM:C,MMChapters:D,MMPuzzles:P,MMMechanics:M}=engine();for(const seed of [184,39214,987]){let s=C.fresh(seed);for(const d of D.list()){a.ok(C.startChapter(s,d.id));for(let i=0;i<65&&!C.has(s,'clear');i++)advance(C,P,M,s);a.ok(C.has(s,'clear'),d.id);a.ok(C.has(s,'thinking.beacon'),d.id+' field thinking quiz');const restored=C.restore(C.serialize(s));a.ok(restored);a.deepEqual(restored.inventory,s.inventory);a.equal(restored.xp,s.xp);s=restored;}a.equal(s.completedChapters.length,15);}});
