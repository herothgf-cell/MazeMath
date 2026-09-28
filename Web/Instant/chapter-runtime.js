/* Compatibility facade: one authoritative objective/runtime lives in MM. */
(function(root){const node=typeof module==='object'&&module.exports;const C=node?require('./core.js'):root.MM;const api={objectiveFor:C.objective,waypoint:C.waypoint,bossInfo:C.bossInfo};if(node)module.exports=api;else root.MMRuntime=api;})(typeof globalThis!=='undefined'?globalThis:this);
