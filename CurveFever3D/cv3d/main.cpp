#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <sys/types.h> 
#include <netinet/tcp.h>

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <sys/time.h>

#include <iostream>
#include <cerrno>
#include <list>
#include <vector>

#define PLAYER_SLOTS 32
#define GLOBAL_PRINT_BUFFER_SIZE 1024*8
#define MAX_FPS 100


using namespace std;


char glob_PrintBuffer[GLOBAL_PRINT_BUFFER_SIZE];

#include "helperFunctions.cpp"
#include "mathFunctions.cpp"
#include "player.cpp"

Player glob_Players[PLAYER_SLOTS];

#include "networkFunctions.cpp"
#include "gameLogic.cpp"


int main()// (int argc, const char * argv[])
{
	int ServerTCPSocket;
	long long loopTimer = 0;
	long long dt = 0; // frametime in usec
	long long frameCounter = 0;
	
	for(int i=0;i<PLAYER_SLOTS;i++) glob_Players[i].dispose();
	
	printf("Socket Creation...\n");
	ServerTCPSocket = setupTCP();
	
	while(1)
	{
		handleNewClients(ServerTCPSocket);
		
		for(int i=0;i<PLAYER_SLOTS;i++)  if(glob_Players[i].exists) // iterate over all connected players
		{
			if(canRead(glob_Players[i].socketTCP))
			{
				// if receive() returns false, the user has disconnected, skip further interaction.
				if(!glob_Players[i].receive()) continue;
			}
			
			glob_Players[i].updatePosition(dt*0.00001);
				
			if(canWrite(glob_Players[i].socketTCP))
			{
				glob_Players[i].send("YourPosition",glob_Players[i].position.toString());
			}
			
			if(glob_Players[i].timeoutExpired())
			{
				printf("Timeout %s\n",glob_Players[i].endpointToString());
				shutdown(glob_Players[i].socketTCP, SHUT_RDWR);
				glob_Players[i].dispose();
				continue;
			}
		}
		
		frameCounter++;
		if(frameCounter%100==0) printf("dt:%lldµs\n",dt);
		
		FpsLimiter(loopTimer, dt);
	}	
}