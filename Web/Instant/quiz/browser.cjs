/* Tests the exact _site release. MM_INLINE explicitly labels local-only storage stubs. */
const fs=require('node:fs'),path=require('node:path'),http=require('node:http'),vm=require('node:vm'),a=require('node:assert/strict');
const PW=require(process.env.PLAYWRIGHT_NODE_PATH||'playwright'),{build}=require('./build.cjs');
const root=path.resolve(__dirname,'../../..'),release=build(),ctx={console};ctx.globalThis=ctx;vm.createContext(ctx);for(const code of release.scripts){if(code.includes('Mobile UI for'))break;vm.runInContext(code,ctx);}const C=ctx.MM,D=ctx.MMChapters,P=ctx.MMPuzzles,Q=ctx.MMQuiz;
const output=path.join(root,'TestResults/quiz');fs.mkdirSync(output,{recursive:true});
const server=http.createServer((req,res)=>{res.writeHead(200,{'Content-Type':'text/html; charset=utf-8'});res.end(release.html);});
const KEY='mazemath.instant.v1',inline=process.env.MM_INLINE==='1';
function fixture(chapter,field=false){let s=C.fresh(39214);s.completedChapters=D.list().filter(d=>d.order<chapter).map(d=>d.id);s.unlockedChapters=D.list().filter(d=>d.order<=chapter).map(d=>d.id);C.startChapter(s,D.list()[chapter-1].id);s.inventory.owned.fill(true);s.inventory.equipped.fill(true);s.inventory.items.fill(20);const st=C.session(s),d=C.definition(s);st.flags=d.tasks.filter(t=>!field||t.id!=='thinking.beacon').map(t=>t.id);st.x=field?32:53;st.y=field?8:16;return C.serialize(s);}
(async()=>{await new Promise(r=>server.listen(0,'127.0.0.1',r));const url=`http://127.0.0.1:${server.address().port}/?test=1`;
 for(const name of (process.env.MM_BROWSERS||'chromium,webkit').split(',')){
  const browser=await PW[name].launch(name==='chromium'&&process.env.CHROMIUM_PATH?{executablePath:process.env.CHROMIUM_PATH}:{});let page;const errors=[];
  async function open(raw=null){page=await browser.newPage({viewport:{width:393,height:852},deviceScaleFactor:2,isMobile:true,hasTouch:true});page.on('pageerror',e=>errors.push(String(e)));
   if(inline){await page.setContent(release.html.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi,''));await page.evaluate(({raw,key})=>{window.__MM_TEST__=true;const store=raw?{[key]:raw}:{};Object.defineProperty(window,'localStorage',{configurable:true,value:{getItem:k=>store[k]??null,setItem:(k,v)=>store[k]=String(v),removeItem:k=>delete store[k]}});},{raw,key:KEY});for(const code of release.scripts)await page.addScriptTag({content:code});}
   else{if(raw)await page.addInitScript(({raw,key})=>{if(!localStorage.getItem(key))localStorage.setItem(key,raw);},{raw,key:KEY});await page.goto(url);}
   await page.locator(raw?'#continue':'#new').click();await frames(1);return page;
  }
  const inspect=()=>page.evaluate(()=>MMApp.inspect()),frames=n=>page.evaluate(n=>MMApp.testFrames(n),n);
  async function input(n){for(const ch of String(n))await page.locator('#key'+(ch==='0'?10:Number(ch)-1)).click();await page.locator('#key11').click();}
  async function resumeReload(){const before=await inspect();await page.evaluate(()=>MMApp.save());if(inline){const saved=await page.evaluate(key=>localStorage.getItem(key),KEY);await page.close();await open(saved);}else{await page.reload();await page.locator('#continue').click();await frames(1);}return before;}
  async function solve(id){const info=await inspect(),p=info.state.puzzles[id];a.ok(p,'puzzle '+id);
   if(['number','cards','symbols'].includes(p.type))await input(p.answer);
   else if(p.type==='rule')await page.locator('#rule'+p.options.indexOf(p.answer)).click();
   else if(['order','memory','equipment'].includes(p.type)){if(p.type==='memory')await page.waitForTimeout(1000+p.sequence.length*150);const choices=p.type==='order'?p.sequence:[0,1,2,3,4];for(const value of p.sequence.slice(p.type==='memory'?0:p.input.length))await page.locator('#seq'+choices.indexOf(value)).click();}
   else if(p.type==='pipe'){const rotations=P.solvePipe(p);a.ok(rotations);for(let i=0;i<p.cells.length;i++){if(!p.cells[i])continue;for(let k=0;k<(rotations[i]-p.rotations[i]+4)%4;k++){if((await inspect()).state.flags.includes(id))break;await page.locator('#pipe'+i).click();}}}
   else if(p.type==='laser'){const rotations=P.solveLaser(p);a.ok(rotations);for(let i=0;i<rotations.length;i++){if((await inspect()).state.flags.includes(id))break;if(rotations[i]!==p.rotations[i])await page.locator('#mirror'+i).click();}}
   else if(p.type==='sokoban'){const moves=P.solveSokoban(p);a.ok(moves);for(const m of moves)await page.locator(['#sUp','#sRight','#sDown','#sLeft'][m]).click();}
   else throw Error('Missing test solver '+p.type);
   a.ok((await inspect()).state.flags.includes(id));
  }
  try{
   await open();a.equal(await page.evaluate(()=>MMApp.version),'15.2');let x=(await inspect()).state.x;await page.locator('[data-hold=right]').dispatchEvent('pointerdown',{pointerId:1,pointerType:'touch'});await frames(20);await page.locator('[data-hold=right]').dispatchEvent('pointerup',{pointerId:1,pointerType:'touch'});a.ok((await inspect()).state.x>x+1);await page.close();
   for(let chapter=1;chapter<=15;chapter++){
    await open(fixture(chapter));await page.locator('#use').click();await page.locator('#acceptQuestion').click();let solvedCount=0;
    while(!(await inspect()).boss.cleared){const info=await inspect(),id=info.boss.next.id,p=info.state.puzzles[id];a.equal(await page.evaluate(()=>MMApp.modal),'boss-question');a.ok(await page.locator('#pauseBoss').isVisible());a.match(await page.locator('#bossQuizProgress').innerText(),new RegExp(`${solvedCount+1} / ${info.boss.total}`));
     if(chapter===1&&solvedCount===0){const before=await inspect();await input((p.answer+1)%100);a.equal((await inspect()).campaign.xp,before.campaign.xp);a.deepEqual((await inspect()).campaign.inventory,before.campaign.inventory);await solve(id);await page.locator('#pauseBoss').click();await page.waitForTimeout(1200);a.equal(await page.evaluate(()=>MMApp.modal),'');await resumeReload();await page.locator('#use').click();await page.locator('#acceptQuestion').click();solvedCount++;continue;}
     if(chapter===4&&p.type==='symbols'){const saved=Q.fingerprint(p);await resumeReload();await page.locator('#use').click();await page.locator('#acceptQuestion').click();a.equal(Q.fingerprint((await inspect()).state.puzzles[id]),saved);}
     if(chapter===13&&p.type==='cards'){
      a.ok(await page.locator('#cardsWorkbench').isVisible());await page.locator('#cardStep').click();a.match(await page.locator('#cardStepInfo').innerText(),/1 \/ 3/);a.equal((await inspect()).state.puzzles[id].solved,false);await page.locator('#cardReset').click();
      for(const [width,height] of [[320,568],[667,375],[844,390],[393,852]]){await page.setViewportSize({width,height});a.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));const box=await page.locator('#cardsWorkbench').boundingBox();a.ok(box.x>=0&&box.x+box.width<=width+.5);await page.screenshot({path:path.join(output,name+'-cards-'+width+'.png')});}
     }
     if(chapter===4&&p.type==='symbols')await page.screenshot({path:path.join(output,name+'-symbols.png')});
     await solve(id);solvedCount++;await page.waitForFunction(id=>MMApp.modal==='clear'||(MMApp.modal==='boss-question'&&document.getElementById('sheet').dataset.quizId!==id),id,{timeout:4000});
     if(!(await inspect()).boss.cleared)a.equal(await page.locator('#acceptQuestion').count(),0,'No repeated conversation or accept click');
    }
    await page.waitForFunction(()=>MMApp.modal==='clear');a.equal(solvedCount,Q.profile(chapter).bossCount);console.log(`PASS ${name}: chapter ${chapter} continuous golem ${solvedCount} questions`);await page.close();
   }
   for(const chapter of [1,4,10,15]){await open(fixture(chapter,true));await page.locator('#use').click();const p=(await inspect()).state.puzzles['thinking.beacon'];a.ok(['cards','symbols'].includes(p.type));await solve('thinking.beacon');await page.waitForFunction(()=>MMApp.modal==='');a.equal((await inspect()).boss.available,true);await page.close();}
   a.deepEqual(errors,[]);console.log(`PASS ${name}: exact generated 15.2, reasoning, wrong answers, pause/timer cancellation, ${inline?'inline storage stub (not HTTP)':'real HTTP localStorage reload'}, mobile layouts, no page errors`);
  }catch(e){if(page&&!page.isClosed()){await page.screenshot({path:path.join(output,name+'-failure.png')}).catch(()=>{});fs.writeFileSync(path.join(output,name+'-failure.txt'),String(e.stack||e));}throw e;}finally{await browser.close();}
 }
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(()=>server.close());
