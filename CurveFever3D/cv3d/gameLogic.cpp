
enum ServerState {ReadyUp, Running};
enum ServerState glob_ServerState;

class gameLogic
{
	public:
	
	static float boxSize;
	
	static void resetGame()
	{
		for(int i=0;i<PLAYER_SLOTS;i++) 
		{
			glob_Players[i].path.clear();
			glob_Players[i].path.reserve(1024);
		}
	}

};

float gameLogic::boxSize = 80;