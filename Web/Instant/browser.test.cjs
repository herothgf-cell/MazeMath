const {chromium,webkit}=require('playwright');
const http=require('node:http'),fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const files={'/':'index.html','/index.html':'index.html','/chapter-data.js':'chapter-data.js','/core.js':'core.js','/chapter-runtime.js':'chapter-runtime.js','/game.js':'game.js'};
const server=http.createServer((req,res)=>{const file=files[req.url];if(!file){res.writeHead(404);res.end();return;}res.writeHead(200,{'Content-Type':file.endsWith('.html')?'text/html; charset=utf-8':'text/javascript; charset=utf-8'});res.end(fs.readFileSync(path.join(__dirname,file)));});
(async()=>{await new Promise(r=>server.listen(0,'127.0.0.1',r));const url=`http://127.0.0.1:${server.address().port}/`;fs.mkdirSync('TestResults/instant',{recursive:true});
for(const [name,type] of [['chromium',chromium],['webkit',webkit]]){const browser=await type.launch();try{const page=await browser.newPage({viewport:{width:393,height:852},deviceScaleFactor:2,isMobile:true,hasTouch:true});let errors=[];page.on('pageerror',e=>errors.push(String(e)));await page.goto(url);await page.locator('#new').click();
const place=async(x,y=0)=>{await page.evaluate(([x,y])=>{Object.assign(MMApp.state(),{x,y,vy:0,climb:null,grounded:true});},[x,y]);await page.waitForTimeout(100);};
const use=async()=>{await page.locator('#use').dispatchEvent('pointerdown',{pointerId:31,pointerType:'touch'});};
const solve=async()=>{const q=await page.evaluate(()=>MMApp.state().question);for(const d of String(q.answer))await page.locator('#key'+(d==='0'?'10':Number(d)-1)).click();await page.locator('#key11').click();await page.waitForTimeout(850);assert.equal(await page.evaluate(()=>MMApp.modal),'');};
let start=await page.evaluate(()=>MMApp.state().x);await page.locator('[data-hold=right]').dispatchEvent('pointerdown',{pointerId:21,pointerType:'touch'});await page.waitForTimeout(450);await page.locator('[data-hold=right]').dispatchEvent('pointerup',{pointerId:21,pointerType:'touch'});let stop=await page.evaluate(()=>MMApp.state().x);assert.ok(stop>start+1);await page.waitForTimeout(180);assert.ok(Math.abs(await page.evaluate(()=>MMApp.state().x)-stop)<.2);
await place(30);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'question');await page.locator('#key10').click();await page.locator('#key11').click();assert.equal(await page.evaluate(()=>MMApp.modal),'question');const q1=await page.evaluate(()=>MMApp.state().question);for(const d of String(q1.answer))await page.locator('#key'+(d==='0'?'10':Number(d)-1)).click();await page.locator('#key11').click();await page.waitForTimeout(850);assert.equal(await page.evaluate(()=>MMApp.modal),'craft-guide');assert.match(await page.locator('#sheet').innerText(),/곡괭이 팔/);assert.match(await page.locator('#sheet').innerText(),/철 조각 2/);await page.locator('#craftGuideClose').click();await place(18);await use();assert.match(await page.locator('#craft0').innerText(),/지금 만들/);await page.locator('#craft0').click();assert.ok(await page.evaluate(()=>MMApp.state().owned[0]));await page.locator('#close').click();await page.evaluate(()=>MMApp.save());await page.reload();await page.locator('#continue').click();assert.ok(await page.evaluate(()=>MMApp.state().owned[0]));
await place(34);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('mined')));
await place(38);await use();await place(44);await use();await place(40);await use();await place(44);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('bridge')));
await place(44,8);await use();await place(48,8);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('laser')));
for(const f of [1,2]){if(f===2){await place(38,16);await use();await solve();}const xs=await page.evaluate(f=>{let r=MM.rng(MMApp.state().seed+f*777),nums=[2,4,6];for(let i=2;i>0;i--){let j=r(i+1);[nums[i],nums[j]]=[nums[j],nums[i]];}return [2,4,6].map(n=>(f===1?16:42)+nums.indexOf(n)*4);},f);for(let x of xs)await place(x,8*f);assert.ok(await page.evaluate(k=>MMApp.state().flags.includes(k),f===1?'sequence':'boss.sequence'));}
await place(54,16);await use();assert.match(await page.locator('#goal').innerText(),/골렘/);assert.match(await page.locator('#goal').innerText(),/조사/);assert.match(await page.locator('#toast').innerText(),/보호막 0개/);await place(57,16);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'clear');assert.ok(await page.locator('#nextChapter').isVisible());await page.locator('#nextChapter').click();assert.equal(await page.evaluate(()=>MMApp.campaign().chapter),'chapter-02');await page.evaluate(()=>MMApp.save());await page.reload();await page.locator('#continue').click();assert.equal(await page.evaluate(()=>MMApp.campaign().chapter),'chapter-02');
// Chapter 2 full route
await place(14,0);await use();if(await page.locator('#tutorialOk').count())await page.locator('#tutorialOk').click();
await place(18,0);await use();await page.locator('#craft1').click();assert.ok(await page.evaluate(()=>MMApp.state().owned[1]));await page.locator('#close').click();
await place(30,0);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c2.genA')));
await place(10,8);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c2.genB')));
await place(46,8);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c2.bridge')));
await place(38,16);await use();await solve();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('boss.math')));
await place(48,16);await use();await page.locator('#pipe0').click();await page.locator('#pipe1').click();await page.locator('#pipe1').click();await page.locator('#pipe2').click();await page.locator('#pipeCheck').click();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('boss.power')));
const xs2=await page.evaluate(()=>{let r=MM.rng(MMApp.state().seed+2*777),nums=[2,3,5];for(let i=2;i>0;i--){let j=r(i+1);[nums[i],nums[j]]=[nums[j],nums[i]];}return[2,3,5].map(n=>42+nums.indexOf(n)*4);});for(let x of xs2)await place(x,16);assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('boss.plates')));
await place(57,16);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'clear');assert.ok(await page.locator('#nextChapter').isVisible());

// Chapter 3
await page.locator('#nextChapter').click();assert.equal(await page.evaluate(()=>MMApp.campaign().chapter),'chapter-03');
await place(14,0);await use();if(await page.locator('#tutorialOk').count())await page.locator('#tutorialOk').click();
await place(18,0);await use();await page.locator('#craft2').click();assert.ok(await page.evaluate(()=>MMApp.state().owned[2]));await page.locator('#close').click();
const solveSoko=async()=>{await page.locator('#sDown').click();await page.locator('#sRight').click();await page.waitForTimeout(120);assert.equal(await page.evaluate(()=>MMApp.modal),'');};
const solveMemory=async(id)=>{await page.waitForTimeout(1000);const seq=await page.evaluate(id=>MMRuntime.memorySequence(MMApp.state().seed,id),id);for(const n of seq)await page.locator('#mem'+n).click();await page.waitForTimeout(120);assert.equal(await page.evaluate(()=>MMApp.modal),'');};
await place(34,0);await use();await solveSoko();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c3.sokoban')));
await place(22,8);await use();await solveMemory('c3.memory');assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c3.memory')));
await place(50,8);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c3.lift')));
await place(36,16);await use();await solveSoko();
await place(45,16);await use();await solveMemory('boss.memory');
await place(54,16);await use();await solve();
await place(58,16);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'clear');assert.ok(await page.locator('#nextChapter').isVisible());

// Chapter 4
await page.locator('#nextChapter').click();assert.equal(await page.evaluate(()=>MMApp.campaign().chapter),'chapter-04');
await place(14,0);await use();if(await page.locator('#tutorialOk').count())await page.locator('#tutorialOk').click();
await place(18,0);await use();await page.locator('#craft4').click();await page.locator('#craft3').click();assert.ok(await page.evaluate(()=>MMApp.state().owned[4]&&MMApp.state().owned[3]));await page.locator('#close').click();
const solveRule=async(id)=>{const p=await page.evaluate(id=>MMRuntime.rulePuzzle(MMApp.state().seed,id),id);const i=p.options.findIndex(x=>x.correct);await page.locator('#rule'+i).click();await page.waitForTimeout(100);};
const solveLaser=async()=>{for(let i=0;i<1;i++)await page.locator('#laser0').click();for(let i=0;i<3;i++)await page.locator('#laser1').click();for(let i=0;i<2;i++)await page.locator('#laser2').click();for(let i=0;i<1;i++)await page.locator('#laser3').click();await page.locator('#laserCheck').click();await page.waitForTimeout(100);};
await place(34,0);await use();await solveRule('c4.rule');
await place(18,8);await use();assert.ok(await page.evaluate(()=>MMApp.state().flags.includes('c4.shield')));
await place(44,8);await use();await solveLaser();
await place(36,16);await use();await solveRule('boss.rule');
await place(46,16);await use();await solveLaser();
await place(54,16);await use();const switchIndex=await page.evaluate(()=>((MMApp.state().seed+'boss.switch'.length)%3+3)%3);await page.locator('#switch'+switchIndex).click();
await place(58,16);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'clear');assert.ok(await page.locator('#nextChapter').isVisible());

// Chapter 5
await page.locator('#nextChapter').click();assert.equal(await page.evaluate(()=>MMApp.campaign().chapter),'chapter-05');
const solveEquipment=async(order)=>{for(const i of order)await page.locator('#trialTool'+i).click();await page.waitForTimeout(100);assert.equal(await page.evaluate(()=>MMApp.modal),'');};
await place(14,0);await use();await solve();
await place(30,0);await use();await solveEquipment([0,1,2,3,4]);
await place(46,0);await use();await solveSoko();
await place(20,8);await use();await solveMemory('c5.memory');
await place(34,16);await use();await solve();
await place(40,16);await use();await solveEquipment([4,3,2,1,0]);
await place(46,16);await use();await solveSoko();
await place(52,16);await use();await solveMemory('boss.memory');
await place(56,16);await use();await page.locator('#coreInspect').click();await page.waitForTimeout(100);
await place(58,16);await use();assert.equal(await page.evaluate(()=>MMApp.modal),'clear');assert.equal(await page.evaluate(()=>MMApp.campaign().completedChapters.length),5);
await page.locator('#chapters').click();assert.match(await page.locator('#sheet').innerText(),/5장 · 별빛 코어 타워/);await page.locator('#close').click();

for(const [width,height] of [[393,852],[844,390],[320,568],[667,375]]){await page.setViewportSize({width,height});await page.waitForTimeout(150);for(const sel of ['#jump','#use','[data-hold=left]','[data-hold=down]']){const b=await page.locator(sel).boundingBox();assert.ok(b&&b.x>=0&&b.y>=0&&b.x+b.width<=width+.5&&b.y+b.height<=height+.5,`${name} ${sel} ${width}x${height}`);}assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));await page.screenshot({path:`TestResults/instant/${name}-${width}x${height}.png`});}
assert.deepEqual(errors,[]);console.log(`PASS ${name}: touch movement, answer feedback/close, crafting, real localStorage reload, bridge/mirrors/plates/boss, four mobile viewports`);
}finally{await browser.close();}}
server.close();})().catch(e=>{console.error(e);server.close();process.exit(1);});
