using System;
using MazeMath.Maze;

namespace MazeMath.Adventure
{
    // Reuses the original room graph; caches paths so HUD refresh does not allocate graphs each frame.
    public static class AdventureNavigator
    {
        private static AdventureState cachedState;
        private static int revision=-1, start=-1, goal=-1;
        private static MazeGraph graph;
        private static int next=-1;
        public static void Route(AdventureState s, ref float x, ref float y)
        {
            int from=AdventureWorld.RoomAt(s.x,s.y), to=AdventureWorld.RoomAt(x,y);
            if (!ReferenceEquals(s,cachedState) || revision!=s.flags.Count)
            {
                cachedState=s; revision=s.flags.Count; graph=Build(s); start=goal=-1;
            }
            if(from!=start || to!=goal)
            {
                start=from; goal=to; var path=graph.FindPath("r"+from,"r"+to);
                next=path.Count>1?int.Parse(path[1].Substring(1)):-1;
            }
            if(next<0) return;
            x=(next%5)*12+6; y=(next/5)*8;
        }
        public static MazeGraph Build(AdventureState s)
        {
            var g=new MazeGraph();
            for(int i=0;i<15;i++) g.AddNode(new MazeNode("r"+i,i/5,i==0?RoomType.Start:i==14?RoomType.Boss:RoomType.Corridor));
            for(int f=0;f<3;f++) for(int c=0;c<4;c++)
            {
                int a=f*5+c,b=a+1;
                if((a==2&&!s.Has("mined"))||(a==3&&!s.Has("bridge"))||(a==7&&!s.Has("laser"))||((a==5||a==10)&&!s.Has("repaired"))) continue;
                g.AddEdge(new MazeEdge("h"+a,"r"+a,"r"+b,EdgeType.Open,true));
            }
            g.AddEdge(new MazeEdge("ladder-a","r4","r9",EdgeType.Ladder,true));
            if(s.Has("sequence"))
            {
                g.AddEdge(new MazeEdge("ladder-b","r7","r12",EdgeType.Ladder,true));
                g.AddEdge(new MazeEdge("shortcut","r1","r6",EdgeType.Ladder,true));
            }
            return g;
        }
    }
}
