(function(root,factory){
  const data=typeof module==='object'&&module.exports?require('./chapter-data.js'):root.MMChapters;
  const api=factory(data);
  if(typeof module==='object'&&module.exports)module.exports=api;else root.MMRuntime=api;
})(typeof globalThis!=='undefined'?globalThis:this,function(D){'use strict';
const steps={
'chapter-01':[
 ['math',30,0,'숫자 장치를 찾아 문제를 풀어요'],
 ['craft0',18,0,'작업대로 돌아가 곡괭이 팔 만들기 · 철 조각 2 + 철판 1'],
 ['mined',34,0,'곡괭이 팔로 균열벽을 열어요'],
 ['bridge',44,0,'상자를 옮겨 저울을 8kg으로 맞춰요'],
 ['laser',45,8,'2층의 거울 2개를 연결해요'],
 ['sequence',18,8,'발판을 2 → 4 → 6 순서로 밟아요'],
 ['boss.math',38,16,'골렘의 숫자 보호막을 풀어요'],
 ['boss.sequence',44,16,'골렘 앞 발판을 2 → 4 → 6 순서로 밟아요'],
 ['boss.laser',54,16,'골렘의 거울 보호막을 풀어요'],
 ['finish',57,16,'오른쪽 수호 골렘에게 가서 조사해요']
],
'chapter-02':[
 ['c2.intro',14,0,'꺼진 발전기를 조사해 전원이 왜 멈췄는지 확인해요'],
 ['craft1',18,0,'작업대에서 파워 렌치를 만들어요 · 철 조각 2 + 기어 1 + 에너지석 1'],
 ['c2.genA',30,0,'파워 렌치로 발전기 A를 수리해요'],
 ['c2.genB',10,8,'갈림길을 기억해 발전기 B를 찾아 수리해요'],
 ['c2.bridge',46,8,'두 발전기의 전력으로 용암 다리를 작동시켜요'],
 ['boss.math',38,16,'용광로 골렘의 곱셈 잠금을 풀어요'],
 ['boss.power',48,16,'오른쪽 배관 장치를 연결해요'],
 ['boss.plates',56,16,'2 → 3 → 5 발판을 순서대로 밟아요'],
 ['finish',61,16,'용광로 골렘에게 다가가 조사해요']
],
'chapter-03':[
 ['c3.intro',16,0,'높은 창고 입구를 조사해 필요한 장비를 확인해요'],
 ['craft2',18,0,'작업대에서 점프 부스터를 만들어요'],
 ['c3.sokoban',34,0,'상자를 밀어 표시된 칸에 놓아요'],
 ['c3.memory',22,8,'빛난 발판 3개의 순서를 기억해 그대로 밟아요'],
 ['c3.lift',50,8,'점프 부스터로 위쪽 리프트에 올라가요'],
 ['boss.sokoban',38,16,'관리자 로봇 앞 상자 퍼즐을 풀어요'],
 ['boss.memory',48,16,'관리자 로봇의 기억 경로를 따라가요'],
 ['boss.missing',57,16,'빈칸 숫자 문제를 풀어요'],
 ['finish',61,16,'창고 관리자 로봇에게 조사해요']
],
'chapter-04':[
 ['c4.intro',14,0,'수정 연구소의 잠긴 회로문을 조사해요'],
 ['craft4',18,0,'탐험 센서를 만들어요 · 숨은 단서를 찾을 수 있어요'],
 ['c4.rule',34,0,'센서로 단서를 찾아 규칙 기계의 답을 골라요'],
 ['c4.shield',18,8,'에너지 실드를 장착하고 전기 구간을 통과해요'],
 ['c4.laser',44,8,'여러 거울을 돌려 수정 빛을 코어에 연결해요'],
 ['boss.rule',38,16,'수정 수호자의 규칙 기계를 풀어요'],
 ['boss.laser',48,16,'수호자의 다중 거울 보호막을 해제해요'],
 ['boss.switch',58,16,'조건에 맞는 회로 스위치를 선택해요'],
 ['finish',62,16,'수정 코어 수호자에게 조사해요']
],
'chapter-05':[
 ['c5.number',14,0,'별빛 숫자 엔진을 풀어 첫 문을 열어요'],
 ['c5.equipment',30,0,'표시된 장비를 골라 장비 게이트를 통과해요'],
 ['c5.spatial',46,0,'상자와 발판을 이용해 공간 퍼즐을 풀어요'],
 ['c5.memory',20,8,'별빛 경로를 기억해 같은 순서로 이동해요'],
 ['boss.number',34,16,'최종 골렘의 숫자 엔진을 풀어요'],
 ['boss.equipment',40,16,'지정된 장비로 장비 보호막을 해제해요'],
 ['boss.spatial',46,16,'공간 퍼즐 보호막을 해제해요'],
 ['boss.memory',52,16,'기억 퍼즐 보호막을 해제해요'],
 ['boss.core',56,16,'별빛 코어를 조사해 마지막 보호막을 해제해요'],
 ['finish',58,16,'별빛 코어 골렘에게 마지막으로 조사해요']
]};
function cs(s){return s&&s.v===2?(s.chapterState||s):s;}
function hasFlag(s,k){let x=cs(s);return !!x&&Array.isArray(x.flags)&&x.flags.includes(k);}
function owns(s,i){return s&&s.v===2?s.inventory.owned[i]:!!s.owned[i];}
function done(s,key){if(key==='craft0')return owns(s,0);if(key==='craft1')return owns(s,1);if(key==='craft2')return owns(s,2);if(key==='craft4')return owns(s,4);if(key==='finish')return hasFlag(s,'clear');return hasFlag(s,key);}
function objectiveFor(s){const id=s&&s.v===2?s.chapter:'chapter-01',list=steps[id]||steps['chapter-01'];for(const row of list){if(!done(s,row[0]))return{key:row[0],x:row[1],y:row[2],text:row[3],chapter:id};}let d=D&&D.get?D.get(id):null;return{key:'clear',x:(d&&d.width?d.width-3:57),y:(d&&d.floors?(d.floors-1)*8:16),text:'챕터 탐험 성공!',chapter:id};}
function completeStep(s,key){let x=cs(s);if(!x||done(s,key))return false;if(key.startsWith('craft')){let i=Number(key.slice(5));if(s.v===2)s.inventory.owned[i]=s.inventory.equipped[i]=true;else s.owned[i]=s.equipped[i]=true;return true;}x.flags.push(key);return true;}
function waypoint(s){let o=objectiveFor(s),x=cs(s),floor=Math.max(0,Math.floor(((x&&x.y)||0+.25)/8)),target=Math.max(0,Math.floor(o.y/8));if(floor===target)return o;const d=D&&D.get?D.get(o.chapter):null;const width=d&&d.width||60;return{x:target>floor?width-8:Math.floor(width/2),y:floor*8,text:target>floor?'사다리를 찾아 ↑로 올라가요':'사다리를 찾아 ↓로 내려가요',key:'ladder',chapter:o.chapter};}
function memorySequence(seed,id){let h=2166136261;for(const ch of String(seed)+':'+id)h=Math.imul(h^ch.charCodeAt(0),16777619);let out=[];for(let i=0;i<3;i++){h^=h<<13;h^=h>>>17;h^=h<<5;out.push((h>>>0)%4);}return out;}
function rulePuzzle(seed,id){let h=2166136261;for(const ch of String(seed)+':'+id)h=Math.imul(h^ch.charCodeAt(0),16777619);let mode=(h>>>0)%3;
 const sets=[
  {clue:'2, 4, 6처럼 모두 짝수인 것을 고르세요.',vals:[8,7,9],ok:0},
  {clue:'3의 배수인 것을 고르세요.',vals:[10,12,14],ok:1},
  {clue:'10보다 크고 20보다 작은 짝수를 고르세요.',vals:[9,16,21],ok:1}
 ],s=sets[mode];
 return{clue:s.clue,options:s.vals.map((v,i)=>({label:String(v),correct:i===s.ok}))};}
function tutorialCueFor(s,eventId){let x=cs(s);x.tutorials=x.tutorials||[];if(x.tutorials.includes(eventId))return null;const cues={
 'first-wrench':{title:'파워 렌치 만들기',text:'지금 만들 것: ⚒ 파워 렌치\n왜 필요해요? 멈춘 발전기를 고칠 수 있어요.\n어디서? 작업대에서 만들어요.'},
 'first-booster':{title:'점프 부스터 만들기',text:'높은 길을 가려면 점프 부스터가 필요해요. 작업대에서 만들고 장착해요.'},
 'first-sensor':{title:'탐험 센서 만들기',text:'숨은 단서를 찾으려면 탐험 센서를 만들고 장착해요.'},
 'first-shield':{title:'에너지 실드',text:'전기 구간에서 실드가 실수 한 번을 막아줘요.'},
 'first-sokoban':{title:'상자 퍼즐',text:'상자를 밀어 표시 칸에 놓아요. 막히면 퍼즐만 다시 시작할 수 있어요.'},
 'first-memory':{title:'기억 경로',text:'빛나는 순서를 보고 기억한 뒤 같은 순서로 밟아요.'},
 'first-rule':{title:'규칙 기계',text:'단서의 공통 규칙을 찾아 조건에 맞는 답을 골라요.'},
 'final-boss':{title:'마지막 보호막',text:'남은 보호막과 다음 행동을 상단 목표에서 확인해요.'}
};return cues[eventId]||null;}
function ackTutorial(s,eventId){let x=cs(s);x.tutorials=x.tutorials||[];if(!x.tutorials.includes(eventId))x.tutorials.push(eventId);}
return{objectiveFor,completeStep,waypoint,memorySequence,rulePuzzle,tutorialCueFor,ackTutorial,steps};
});