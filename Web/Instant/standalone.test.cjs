/* Tests the exact bundled 15.1 index that GitHub Pages serves. */
const http=require('node:http'),fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const PW=require(process.env.PLAYWRIGHT_NODE_PATH||'playwright');
const index=fs.readFileSync(path.join(__dirname,'index.html'));
const server=http.createServer((req,res)=>{res.writeHead(200,{'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store'});res.end(index);});
const root=path.resolve(__dirname,'../../TestResults/instant');fs.mkdirSync(root,{recursive:true});
(async()=>{await new Promise(r=>server.listen(0,'127.0.0.1',r));const url=`http://127.0.0.1:${server.address().port}/?test=1`;
for(const name of (process.env.MM_BROWSERS||'chromium,webkit').split(',')){const browser=await PW[name].launch();let page;
try{page=await browser.newPage({viewport:{width:393,height:852},deviceScaleFactor:2,isMobile:true,hasTouch:true});const errors=[];page.on('pageerror',e=>errors.push(String(e)));await page.goto(url);
assert.equal(await page.evaluate(()=>MMApp.version),'15.1');
assert.equal(await page.evaluate(()=>MMChapters.list().length),15);
assert.equal(await page.evaluate(()=>MMChapters.lastId),'chapter-15');
assert.equal(await page.evaluate(()=>MMChapters.get('chapter-10').tasks.some(t=>t.type==='platform')),true);
assert.equal(await page.evaluate(()=>MMChapters.get('chapter-11').encounters.length>=1),true);
assert.deepEqual(await page.evaluate(()=>MMMechanics.profile('chapter-15').tables),[2,3,5]);
await page.locator('#new').click();
let before=await page.evaluate(()=>MMApp.inspect().state.x);
const right=page.locator('[data-hold=right]');await right.dispatchEvent('pointerdown',{pointerId:41,pointerType:'touch'});await page.evaluate(()=>MMApp.testFrames(35));await right.dispatchEvent('pointerup',{pointerId:41,pointerType:'touch'});
let after=await page.evaluate(()=>MMApp.inspect().state.x);assert.ok(after>before+.5,'touch movement');
await page.evaluate(()=>MMApp.save());await page.reload();assert.equal(await page.locator('#continue').count(),1);await page.locator('#continue').click();assert.ok(Math.abs((await page.evaluate(()=>MMApp.inspect().state.x))-after)<.15,'localStorage reload');
await page.locator('#pausebtn').click();await page.locator('#selectChapters').click();assert.equal(await page.locator('.chapter-card').count(),15);await page.locator('#close').click();
for(const [width,height] of [[393,852],[844,390],[320,568],[667,375]]){await page.setViewportSize({width,height});await page.waitForTimeout(50);assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));for(const sel of ['#jump','#use','[data-hold=left]','#hot4','#bagbtn']){const b=await page.locator(sel).boundingBox();assert.ok(b&&b.x>=0&&b.y>=0&&b.x+b.width<=width+.6&&b.y+b.height<=height+.6,`${name} ${sel} ${width}x${height}`);}await page.screenshot({path:path.join(root,`${name}-15.1-${width}x${height}.png`)});}
assert.deepEqual(errors,[]);console.log(`PASS ${name}: bundled 15.1, 15 chapters, mechanics/monsters, touch, real localStorage reload, four mobile layouts`);
}finally{if(page)await page.close();await browser.close();}}
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(()=>server.close());
