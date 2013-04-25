int setupTCP();
void handleNewClients(int);
int canRead(int);
int canWrite(int);

int setupTCP()
{
	int sfd;
	struct sockaddr_in serv_addr;

	sfd = socket(AF_INET, SOCK_STREAM, 0);

	if (sfd == -1)
		printf("Error: socket: %s\n",strerror(errno));

	int flag = 1;
	if(setsockopt(sfd, IPPROTO_TCP, TCP_NODELAY, (char *) &flag,sizeof(int)) == -1)
		printf("Error: setsockopt: %s\n",strerror(errno));
	
	bzero((char *)&serv_addr, sizeof(serv_addr));

	// Fill server's address family
	serv_addr.sin_family = AF_INET;

	// Server should allow connections from any ip address
	serv_addr.sin_addr.s_addr = INADDR_ANY;

	// 16 bit port number on which server listens
	// The function htons (host to network short) ensures that an integer is interpretted
	// correctly (whether little endian or big endian) even if client and server have different architectures
	serv_addr.sin_port = htons(3449);
	
	 if (bind(sfd, (struct sockaddr *)&serv_addr, sizeof(struct sockaddr_in)) == -1)
		printf("Error: bind: %s\n",strerror(errno));

	if (listen(sfd, 16) == -1)
		printf("Error: listen: %s\n",strerror(errno));
		
	return sfd;

}

void handleNewClients(int ServerTCPSocket)
{
	int ClientTCPSocket,selectResult;
    struct sockaddr peer_addr;
	struct sockaddr_in* peer_addr_in;
    socklen_t peer_addr_size;
	char buffer[4096];
	
	peer_addr_size = sizeof(struct sockaddr);
	
	if(canRead(ServerTCPSocket))
	{
		ClientTCPSocket = accept(ServerTCPSocket, &peer_addr, &peer_addr_size);
		if (ClientTCPSocket == -1)
			printf("Error: accept: %s\n",strerror(errno));
		else
		{
			peer_addr_in = (struct sockaddr_in*)&peer_addr;
			
			Player newPlayer;
			newPlayer.readyUp = false;
			newPlayer.ip = peer_addr_in->sin_addr;
			newPlayer.port = ntohs(peer_addr_in->sin_port);
			newPlayer.socketTCP = ClientTCPSocket;
			newPlayer.exists = true;
			newPlayer.timeout = 5;
			newPlayer.resetTimeout();
			
			newPlayer.position.x = 40;
			newPlayer.position.y = 40;
			newPlayer.position.z = 40;
			
			newPlayer.pitch = 0;
			newPlayer.yaw = 0;
			
			printf("New connection: %s\n", newPlayer.endpointToString());
			
			bool freeSlotFound = false;
			for(int i=0;i<PLAYER_SLOTS;i++) if(!glob_Players[i].exists)
			{
				glob_Players[i] = newPlayer;
				freeSlotFound = true;
				break;
			}
			if(!freeSlotFound) 
			{
				shutdown(newPlayer.socketTCP, SHUT_RDWR);
			}
		}		
	}
}

int canRead(int fd)
{
	// all the ugly structs
	int ClientTCPSocket,selectResult;
    struct sockaddr_in peer_addr;
    socklen_t peer_addr_size;
	fd_set readSet;
	struct timeval timer;
	
	// initailizing shit
    timer.tv_sec = 0;
    timer.tv_usec = 0;
	
	FD_ZERO(&readSet);
    FD_SET(fd, &readSet);
	
	
	// ok here it starts, checking for new connections
	selectResult = select(fd+1, &readSet, NULL, NULL, &timer);
	
	if(selectResult == -1)
		printf("Error: select: %s\n",strerror(errno));
		
	return selectResult > 0;
}

int canWrite(int fd)
{
	// all the ugly structs
	int ClientTCPSocket,selectResult;
    struct sockaddr_in peer_addr;
    socklen_t peer_addr_size;
	fd_set writeSet;
	struct timeval timer;
	
	// initailizing shit
    timer.tv_sec = 0;
    timer.tv_usec = 0;
	
	FD_ZERO(&writeSet);
    FD_SET(fd, &writeSet);
	
	
	// ok here it starts, checking for new connections
	selectResult = select(fd+1, NULL, &writeSet, NULL, &timer);
	
	if(selectResult == -1)
		printf("Error: select: %s\n",strerror(errno));
		
	return selectResult > 0;
}