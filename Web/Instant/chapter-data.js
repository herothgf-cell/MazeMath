(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.MMChapters=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){'use strict';
const chapters=[
{id:'chapter-01',order:1,title:'광산 입구',floors:3,width:60,start:[14,0],requiredTool:0,palette:{sky:'#96b6b1',stone:'#718369',accent:'#82a354'},boss:{id:'guardian-golem',phases:['boss.math','boss.sequence','boss.laser']}},
{id:'chapter-02',order:2,title:'용암 제련소',floors:3,width:60,start:[8,0],requiredTool:1,palette:{sky:'#563b32',stone:'#756252',accent:'#e47b36'},boss:{id:'furnace-golem',phases:['boss.math','boss.power','boss.plates']}},
{id:'chapter-03',order:3,title:'하늘 창고',floors:3,width:60,start:[8,0],requiredTool:2,palette:{sky:'#9dcce0',stone:'#c7ba91',accent:'#e5cc70'},boss:{id:'warehouse-manager',phases:['boss.sokoban','boss.memory','boss.missing']}},
{id:'chapter-04',order:4,title:'수정 회로 연구소',floors:3,width:60,start:[8,0],requiredTool:4,palette:{sky:'#39445f',stone:'#66708a',accent:'#9be0dc'},boss:{id:'crystal-guardian',phases:['boss.rule','boss.laser','boss.switch']}},
{id:'chapter-05',order:5,title:'별빛 코어 타워',floors:3,width:60,start:[8,0],requiredTool:null,palette:{sky:'#28324b',stone:'#756e85',accent:'#f0d47b'},boss:{id:'starlight-core-golem',phases:['boss.number','boss.equipment','boss.spatial','boss.memory','boss.core']}}
];
const byId=Object.fromEntries(chapters.map(x=>[x.id,x]));
function get(id){return byId[id]||null;}
function list(){return chapters.map(x=>({...x,start:[...x.start],boss:{...x.boss,phases:[...x.boss.phases]}}));}
return{get,list,firstId:'chapter-01'};
});