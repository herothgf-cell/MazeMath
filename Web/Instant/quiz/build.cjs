/* Reproducible source adapter: keep the deployed 15.1 source, apply reviewed quiz modules,
   and test the exact generated page rather than the stale 10.1 modular copy. */
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const root=path.resolve(__dirname,'../../..');
function once(text,find,replacement){const parts=text.split(find);if(parts.length!==2)throw Error('Source anchor must match once: '+find.slice(0,100));return parts[0]+replacement+parts[1];}
function replaceFunction(text,name,next,code){const start=text.indexOf('function '+name+'('),end=text.indexOf('function '+next+'(',start);if(start<0||end<0)throw Error('Function boundary missing: '+name);return text.slice(0,start)+code+'\n'+text.slice(end);}
function build(input=path.join(root,'Web/Instant/index.html'),output=path.join(root,'_site')){
 let html=fs.readFileSync(input,'utf8'),scripts=[...html.matchAll(/<script\b[^>]*>([\s\S]*?)<\/script>/gi)].map(m=>m[1]);if(scripts.length!==9||!scripts[0].includes('Original chapter content')||!scripts[3].includes('Campaign rules'))throw Error('Unrecognized baseline bundle; review before building.');
 const read=f=>fs.readFileSync(path.join(__dirname,f),'utf8');
 scripts[0]=once(scripts[0],'function freeze(o){','globalThis.MMQuiz.expand(chapters);\nfunction freeze(o){');
 scripts[2]+='\nglobalThis.MMQuiz.install(globalThis.MMPuzzles);\n';
 scripts[3]=replaceFunction(scripts[3],'puzzle','canUse',"function puzzle(s,id){const st=session(s),desc=descriptor(s,id);if(!desc)return null;if(!st.puzzles[id])st.puzzles[id]=globalThis.MMQuiz.issue(P,s,st,desc);return st.puzzles[id];}");
 scripts[3]=once(scripts[3],"const r=raw.puzzles?.[def.id];if(!r)continue;const p=P.create(def.type,P.hash(out.seed+':'+def.id),def);","const r=raw.puzzles?.[def.id];if(!r)continue;const p=globalThis.MMQuiz.restoreBase(P,def,r,out.seed,D.get(id).order);");
 scripts[3]=once(scripts[3],"if(p.type==='number'&&r.type==='number'", "if(!r.quizVersion&&p.type==='number'&&r.type==='number'");
 scripts[3]=once(scripts[3],"if(!s.unlockedChapters.includes(id))return null;return s;", "if(!s.unlockedChapters.includes(id))return null;s.quizBook=globalThis.MMQuiz.restoreBook(raw.quizBook);return s;");
 let game=scripts[8];
 game=once(game,'<div class="pipe-end">🔥<br>입구 →</div>','<div class="pipe-end pipe-port"><span style="grid-row:${p.source[1]+1}">🔥<br>입구 →</span></div>');
 game=once(game,'<div class="pipe-end">→ 🏭<br>도착</div>','<div class="pipe-end pipe-port"><span style="grid-row:${p.target[1]+1}">→ 🏭<br>도착</span></div>');
 game=replaceFunction(game,'completeUI','chapterClear','');game=replaceFunction(game,'numberUI','ruleUI',read('answer-ui.js'));
 game=once(game,"if(p.type==='number')numberUI(id,titleText,boss);","if(['number','cards','symbols'].includes(p.type))numberUI(id,titleText,boss);");
 const start=game.indexOf('function openPuzzle('),end=game.indexOf('function mechanismUI(',start);if(start<0||end<0)throw Error('Quiz dispatcher not found');const block=game.slice(start,end),close=block.lastIndexOf('}');game=game.slice(0,start)+block.slice(0,close)+' if(boss)decorateBoss(id);\n'+block.slice(close)+game.slice(end);
 game=once(game,"VERSION='15.1'","VERSION='15.2'");scripts[8]=game;
 // Include the helpers before their consumers. Each generated script is syntax-checked.
 const emitted=[read('rules.js'),...scripts.slice(0,8),read('view.js'),scripts[8]];for(let i=0;i<emitted.length;i++)new vm.Script(emitted[i],{filename:'release-script-'+i+'.js'});
 let i=0;html=html.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi,()=>i++===0?'__QUIZ_SCRIPTS__':'');html=html.replace('__QUIZ_SCRIPTS__',()=>emitted.map(s=>'<script>\n'+s+'\n</script>').join('\n'));html=once(html,'</style>',read('style.css')+'\n</style>');html=html.replaceAll('15.1','15.2').replace(' PC 미리보기','');
 fs.mkdirSync(output,{recursive:true});fs.writeFileSync(path.join(output,'index.html'),html);return{html,scripts:emitted,output};
}
if(require.main===module){const result=build(process.argv[2],process.argv[3]);console.log('Built 15.2 -> '+result.output+' ('+Buffer.byteLength(result.html)+' bytes)');}
module.exports={build,once};
