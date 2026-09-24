'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '../..');
const source = path.join(root, 'Assets/WebGLTemplates/MazeMathMobile/mobile-web.js');
function api() { delete require.cache[require.resolve(source)]; return require(source); }

test('recognizes iPhone, Android and desktop-mode iPad without misclassifying a mouse-only Mac', () => {
  const a = api();
  assert.equal(a.isTouchBrowser({userAgent:'iPhone', maxTouchPoints:5}), true);
  assert.equal(a.isTouchBrowser({userAgent:'Android', maxTouchPoints:0}), true);
  assert.equal(a.isTouchBrowser({userAgent:'Macintosh', maxTouchPoints:5}), true);
  assert.equal(a.isTouchBrowser({userAgent:'Macintosh', maxTouchPoints:0}), false);
});
test('default clarity retains high DPI on an ordinary landscape phone', () => {
  assert.equal(api().renderRatio(844,390,3,'sharp'), 2);
});
test('large screens respect the pixel budget while light mode is available', () => {
  const a = api(), ratio = a.renderRatio(1440,900,3,'sharp');
  assert.ok(1440*900*ratio*ratio <= 2400001);
  assert.equal(a.renderRatio(844,390,3,'light'), 1);
  assert.equal(a.renderRatio(844,390,1,'sharp'), 1);
});
test('invalid browser dimensions cannot produce NaN or an infinite render ratio', () => {
  const a=api();
  for (const args of [[0,0,0],[-5,0,NaN],[Infinity,1,3],[500,800,Infinity]]) {
    const r=a.renderRatio(...args,'sharp');
    assert.ok(Number.isFinite(r) && r > 0 && r <= 2);
  }
});
test('missing optional fullscreen and orientation APIs do not block play', async () => {
  assert.equal(await api().tryFullscreen({},{}), false);
  assert.equal(await api().tryFullscreen({requestFullscreen:()=>Promise.reject(new Error('denied'))},{}), false);
  assert.equal(await api().tryFullscreen({requestFullscreen:()=>Promise.resolve()}, {orientation:{lock:()=>Promise.reject(new Error('unsupported'))}}), true);
});
test('template uses actual Unity build file placeholders and a touch-safe responsive canvas', () => {
  const html=fs.readFileSync(path.join(root,'Assets/WebGLTemplates/MazeMathMobile/index.html'),'utf8');
  for(const key of ['LOADER_FILENAME','DATA_FILENAME','FRAMEWORK_FILENAME','CODE_FILENAME']) assert.ok(html.includes('{{{ '+key+' }}}'));
  assert.ok(html.includes('viewport-fit=cover'));
  assert.ok(!html.includes('maximum-scale=1'));
  const css=fs.readFileSync(path.join(root,'Assets/WebGLTemplates/MazeMathMobile/mobile-web.css'),'utf8');
  assert.ok(css.includes('touch-action: none'));
  assert.ok(css.includes('safe-area-inset'));
});
test('loader failures display an error rather than a false success screen', async () => {
  const a=api();
  const make=()=>({hidden:false,disabled:false,textContent:'',value:'sharp',style:{},classList:{add(){},remove(){}},listeners:{},addEventListener(n,f){this.listeners[n]=f;},getBoundingClientRect(){return {width:844,height:390};},getContext(){return {};},focus(){}});
  const ids={}; ['unity-canvas','start-layer','start-button','loading','progress','status','error','rotate-layer','fullscreen','quality','storage-warning'].forEach(id=>ids[id]=make());
  ids.error.hidden=true;
  const doc={hidden:false,getElementById:id=>ids[id],createElement:make,addEventListener(){},head:{appendChild(script){queueMicrotask(()=>script.onerror());}}};
  const win={document:doc,navigator:{userAgent:'iPhone',maxTouchPoints:5},location:{protocol:'https:'},WebAssembly:{},devicePixelRatio:3,screen:{},addEventListener(){},MAZEMATH_BUILD:{loaderUrl:'Build/no.loader.js',config:{}},localStorage:{setItem(){},removeItem(){}}};
  a.mount(win);
  await ids['start-button'].listeners.click();
  await new Promise(r=>setTimeout(r,0));
  assert.equal(ids.error.hidden,false);
  assert.match(ids.error.textContent,/불러|오류|실패/);
});
test('mobile build has a distinct output and restores project settings in finally', () => {
  const cs=fs.readFileSync(path.join(root,'Assets/MazeMath/Editor/Build/BuildMobileWeb.cs'),'utf8');
  assert.ok(cs.includes('Build/MobileWeb'));
  assert.ok(cs.includes('PROJECT:MazeMathMobile'));
  assert.ok(cs.includes('finally'));
  assert.ok(cs.includes('threadsSupport = false'));
  assert.ok(!cs.includes('SetupProject'));
});
