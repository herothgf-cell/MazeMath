/* Browser integration walks through the real input/physics loop using a deterministic test clock. */
const http=require('node:http'),fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const PW=require(process.env.PLAYWRIGHT_NODE_PATH||'playwright'),C=require('./core.js'),D=require('./chapter-data.js'),P=require('./puzzles.js');
const allowed=['index.html','style.css','game.js','core.js','puzzles.js','chapter-data.js','chapter-runtime.js','world-renderer.js'];
const server=http.createServer((req,res)=>{const u=new URL(req.url,'http://localhost'),f=u.pathname==='/'?'index.html':u.pathname.slice(1);if(!allowed.includes(f)){res.writeHead(404);res.end();return;}res.writeHead(200,{'Content-Type':f.endsWith('.html')?'text/html; charset=utf-8':f.endsWith('.css')?'text/css; charset=utf-8':'text/javascript; charset=utf-8'});res.end(fs.readFileSync(path.join(__dirname,f)));});
const root=path.resolve(__dirname,'../../TestResults/instant');fs.mkdirSync(root,{recursive:true});
(async()=>{await new Promise(r=>server.listen(0,'127.0.0.1',r));const url=`http://127.0.0.1:${server.address().port}/?test=1`;
 const engines=(process.env.MM_BROWSERS||'chromium,webkit').split(',');for(const name of engines){const type=PW[name],browser=await type.launch(name==='chromium'&&process.env.CHROMIUM_PATH?{executablePath:process.env.CHROMIUM_PATH}:{});let page;
 try{page=await browser.newPage({viewport:{width:393,height:852},deviceScaleFactor:2,isMobile:true,hasTouch:true});let errors=[];page.on('pageerror',e=>{errors.push(String(e));console.error('PAGE ERROR',e.message);});if(process.env.MM_INLINE==='1'){
 const html=fs.readFileSync(path.join(__dirname,'index.html'),'utf8').replace(/<script[^>]*src=[^>]*><\/script>/g,'').replace(/<link[^>]*>/g,'');
 await page.setContent(html);await page.addStyleTag({content:fs.readFileSync(path.join(__dirname,'style.css'),'utf8')});
 await page.evaluate(()=>{window.__MM_TEST__=true;const store={};Object.defineProperty(window,'localStorage',{configurable:true,value:{getItem:k=>store[k]??null,setItem:(k,v)=>store[k]=String(v),removeItem:k=>delete store[k]}});});
 for(const f of ['chapter-data.js','puzzles.js','core.js','chapter-runtime.js','world-renderer.js','game.js'])await page.addScriptTag({content:fs.readFileSync(path.join(__dirname,f),'utf8')});
 }else await page.goto(url);await page.locator('#new').click();
 const inspect=()=>page.evaluate(()=>MMApp.inspect());const frames=n=>page.evaluate(n=>MMApp.testFrames(n),n);
 const hold=async(direction,n)=>{const el=page.locator(`[data-hold=${direction}]`);await el.dispatchEvent('pointerdown',{pointerId:55,pointerType:'touch'});await frames(n);await el.dispatchEvent('pointerup',{pointerId:55,pointerType:'touch'});};
 async function travel(x,floor){for(let n=0;n<140;n++){const info=await inspect(),st=info.state;const f=Math.max(0,Math.min(2,Math.floor((st.y+.15)/8)));if(Math.abs(st.y-floor*8)<.09&&Math.abs(st.x-x)<.25){await frames(1);return;}if(st.climb){await hold(floor*8>st.y?'up':'down',8);continue;}if(f!==floor){const l=D.get(info.campaign.chapter).ladders.find(l=>floor>f?l[1]===f*8:l[2]===f*8);assert.ok(l);if(Math.abs(st.x-l[0])>.15)await hold(st.x<l[0]?'right':'left',Math.max(1,Math.min(90,Math.round(Math.abs(st.x-l[0])/.1))));else await hold(floor>f?'up':'down',90);}else{const dist=Math.abs(st.x-x);await hold(st.x<x?'right':'left',Math.max(1,Math.min(90,Math.round(dist/.1))));}}
 throw Error(`Route blocked -> ${x},${floor}: ${JSON.stringify(await inspect())}`);}
 const use=()=>page.locator('#use').click();
 async function craft(i){await page.locator('#recipe'+i).click();assert.equal(await page.locator('#makeCraft').isEnabled(),false);for(let j=0;j<9;j++){const mat=C.recipes[i][j];if(mat<0)continue;await page.locator('#pick'+mat).click();await page.locator('#cell'+j).click();}assert.equal(await page.locator('#makeCraft').isEnabled(),true);await page.locator('#makeCraft').click();assert.equal((await inspect()).campaign.inventory.owned[i],true);}
 async function solve(id){console.log('SOLVE',id);const info=await inspect(),p=info.state.puzzles[id];assert.ok(p,`Puzzle missing ${id}`);
  if(['number','rule'].includes(p.type)){
   if(p.type==='number'){if(id==='math'){const before=(await inspect()).campaign;await page.locator('#key10').click();await page.locator('#key11').click();assert.equal(await page.evaluate(()=>MMApp.modal),'question');const after=(await inspect()).campaign;assert.equal(after.xp,before.xp);assert.deepEqual(after.inventory,before.inventory);}for(const c of String(p.answer))await page.locator('#key'+(c==='0'?10:Number(c)-1)).click();await page.locator('#key11').click();}
   else await page.locator('#rule'+p.options.indexOf(p.answer)).click();
  }else if(['order','memory','equipment'].includes(p.type)){
   if(p.type==='memory')await page.waitForTimeout(950+p.sequence.length*150);
   const choices=p.type==='order'?p.sequence:[0,1,2,3,4];for(const value of p.sequence.slice(p.input.length))await page.locator('#seq'+choices.indexOf(value)).click();
  }else if(p.type==='pipe'){
   const rotations=P.solvePipe(p);assert.ok(rotations);
   // Straight pipes have equivalent orientations: another valid route may finish before the planned rotations.
   pipeMoves: for(let i=0;i<9;i++){if(!p.cells[i])continue;const n=(rotations[i]-p.rotations[i]+4)%4;for(let j=0;j<n;j++){
    const live=await inspect();if(live.state.flags.includes(id)){assert.ok(P.pipeTrace(live.state.puzzles[id]).solved);break pipeMoves;}
    await page.locator('#pipe'+i).click();
   }}
   const connected=await inspect();assert.ok(connected.state.flags.includes(id));assert.ok(P.pipeTrace(connected.state.puzzles[id]).solved);
  }else if(p.type==='laser'){
   const rotations=P.solveLaser(p);assert.ok(rotations);for(let i=0;i<rotations.length;i++){
    const live=await inspect();if(live.state.flags.includes(id)){assert.ok(P.laserTrace(live.state.puzzles[id]).solved);break;}
    if(rotations[i]!==p.rotations[i])await page.locator('#mirror'+i).click();
   }
   const lit=await inspect();assert.ok(lit.state.flags.includes(id));assert.ok(P.laserTrace(lit.state.puzzles[id]).solved);
  }else if(p.type==='sokoban'){
   const moves=P.solveSokoban(p);assert.ok(moves);for(const move of moves)await page.locator(['#sUp','#sRight','#sDown','#sLeft'][move]).click();
  }else throw Error('Unsupported UI puzzle '+p.type);
  console.log('SUBMITTED',id,await page.evaluate(()=>MMApp.modal));await page.waitForFunction(()=>MMApp.modal===''||MMApp.modal==='clear',{},{timeout:3500});await frames(50);
 }
 // Genuine touch movement and release, not just DOM presence.
 let x=(await inspect()).state.x;await hold('right',20);assert.ok((await inspect()).state.x>x+1);x=(await inspect()).state.x;await frames(20);assert.ok(Math.abs((await inspect()).state.x-x)<.01);
 for(const d of D.list()){
  assert.equal((await inspect()).campaign.chapter,d.id);
  for(let guard=0;guard<70;guard++){
   await frames(50);const info=await inspect();if(info.state.flags.includes('clear'))break;const o=info.objective;console.log('STEP',name,d.order,o.id,'at',info.state.x.toFixed(1),info.state.y.toFixed(1));
   if(o.kind==='craft'){await travel(18,0);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'workshop');await craft(o.tool);continue;}
   if(o.kind==='equip'){await page.locator('#hot'+o.tool).click();continue;}
   await travel(o.x,o.floor);
   if(o.kind==='boss'){
    if(d.order===1&&!(await inspect()).campaign.inventory.owned[1]){await travel(18,0);await use();await craft(1);continue;}
    await use();assert.equal(await page.evaluate(()=>MMApp.modal),'boss');assert.match(await page.locator('#dialogTitle').innerText(),/골렘/);const id=(await inspect()).boss.next.id;await page.locator('#acceptQuestion').click();assert.equal(await page.evaluate(()=>MMApp.modal),'boss-question');await solve(id);continue;
   }
   const t=d.tasks.find(t=>t.id===o.id);assert.ok(t);if(t.tool!==undefined)await page.locator('#hot'+t.tool).click();
   if(t.type==='sequence'){
    // Real foot-contact test: approach each target by jumping over other plates.
    for(let tries=0;tries<40;tries++){
     const now=await inspect();if(now.state.flags.includes(t.id))break;const p=now.state.puzzles[t.id],layout=C.plateLayout(now.campaign,t),target=layout.find(v=>v.n===p.sequence[p.input.length]);
     // Jump in short hops so intervening wrong plates aren't pressed.
     const dx=target.x-now.state.x;if(Math.abs(dx)>.23){await page.locator('#jump').dispatchEvent('pointerdown',{pointerId:77,pointerType:'touch'});await hold(dx>0?'right':'left',Math.min(39,Math.max(1,Math.round(Math.abs(dx)/.1))));await frames(65);}else await frames(70);
    }
    assert.ok((await inspect()).state.flags.includes(t.id),'Physical sequence completion '+d.id);continue;
   }
   if(t.type==='weight'){
    // Actual carrying: walk to a crate, pick up, walk back and place on the scale.
    let need=[];for(let mask=0;mask<1<<t.numbers.length;mask++){const picked=t.numbers.map((_,i)=>mask&(1<<i)?i:-1).filter(i=>i>=0);if(picked.reduce((n,i)=>n+t.numbers[i],0)===t.target){need=picked;break;}}
    for(const i of need){await travel(t.x-4+i*1.25,t.floor);await use();assert.ok((await inspect()).state.carried);await travel(t.x,t.floor);await use();}assert.ok((await inspect()).state.flags.includes(t.id));continue;
   }
   await use();
   if(t.type==='supply'){assert.equal(await page.evaluate(()=>MMApp.modal),'');continue;}
   if(['repair','mine','shield'].includes(t.type)){
    let st=(await inspect()).state;assert.ok(st.action);assert.equal(st.action.tool,t.tool);
    if(t.type==='repair'&&d.order===2){await page.waitForTimeout(1700);await frames(8);await page.screenshot({path:path.join(root,name+'-generator-wrench.png')});}
    while(!(await inspect()).state.flags.includes(t.id)){await frames(32);await use();}
   }else if(t.type==='jump'){await frames(100);assert.ok((await inspect()).state.flags.includes(t.id));}
   else{if(t.tool===4)await page.waitForTimeout(720);if(t.id==='c3.memory'){await page.locator('#close').click();await page.waitForTimeout(1700);assert.equal(await page.evaluate(()=>MMApp.modal),'');await use();}await solve(t.id);}
  }
  assert.ok((await inspect()).state.flags.includes('clear'),'Chapter cleared '+d.id);await page.waitForFunction(()=>MMApp.modal==='clear',{},{timeout:3500});
  assert.equal((await inspect()).campaign.completedChapters.length,d.order);
  if(d.order===3||d.order===10)await page.screenshot({path:path.join(root,`${name}-chapter-${d.order}-clear.png`)});
  console.log(`PASS ${name}: chapter ${d.order} field route, tool actions, golem questions, automatic clear`);
  if(d.order<10){await page.locator('#nextChapter').click();if(d.order===4&&process.env.MM_INLINE!=='1'){await page.evaluate(()=>MMApp.save());const before=await inspect();await page.reload();await page.locator('#continue').click();const after=await inspect();assert.equal(after.campaign.chapter,'chapter-05');assert.deepEqual(after.campaign.inventory,before.campaign.inventory);}}
 }
 await page.locator('#chapters').click();assert.equal(await page.locator('.chapter-card').count(),10);
 for(const [width,height] of [[393,852],[844,390],[320,568],[667,375]]){
  await page.setViewportSize({width,height});await page.waitForTimeout(80);assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));await page.locator('#close').click();
  for(const selector of ['#jump','#use','[data-hold=left]','[data-hold=down]','#hot4','#bagbtn']){const b=await page.locator(selector).boundingBox();assert.ok(b&&b.x>=0&&b.y>=0&&b.x+b.width<=width+.6&&b.y+b.height<=height+.6,`${name} ${selector} ${width}x${height}`);}
  await page.screenshot({path:path.join(root,`${name}-layout-${width}x${height}.png`)});await page.locator('#pausebtn').click();await page.locator('#selectChapters').click();
 }
 assert.deepEqual(errors,[]);console.log(`PASS ${name}: ten-chapter campaign, touch release, 3x3 recipes, ${process.env.MM_INLINE==='1'?'inline storage stub (real reload checked in CI)':'real localStorage save/reload'}, four mobile layouts; no page errors`);
 }catch(e){if(page){await page.screenshot({path:path.join(root,name+'-failure.png')}).catch(()=>{});fs.writeFileSync(path.join(root,name+'-failure.txt'),String(e.stack||e));}throw e;}finally{await browser.close();}}
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(()=>server.close());
