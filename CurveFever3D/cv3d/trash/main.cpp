#include <stdio.h>
#include <sys/socket.h>
#include <sys/types.h> 
#include <sys/un.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <netinet/in.h>
#include <errno.h>

#include <iostream>
#include <cerrno>

using namespace std;


enum GameState 
{
   ReadyUp,
   Running,
   Scores
};

//#include "checkFD.cpp"
//#include "setupINET.cpp"
//#include "handleNewClients.cpp"
#include "NetworkFunctions.cpp"
#include "Player.cpp"



enum GameState glob_GameState;

int main()// (int argc, const char * argv[])
{
	int ServerTCPSocket;
	
	fprintf(stderr,"\nSocket Creation...\n");
	ServerTCPSocket = setupTCP();
	
	while(1)
	{
		fprintf(stderr, ".");
		handleNewClients(ServerTCPSocket);
		sleep(1);
	}	
}