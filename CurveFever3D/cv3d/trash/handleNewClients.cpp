void handleNewClients(int ServerTCPSocket)
{
	// all the ugly structs
	int ClientTCPSocket,selectResult;
    struct sockaddr_in peer_addr;
    socklen_t peer_addr_size;
	//fd_set readSet;
	//struct timeval timer;
	/*
	// initailizing shit
    timer.tv_sec = 1;
    timer.tv_usec = 0;
	
	FD_ZERO(&readSet);
    FD_SET(ServerTCPSocket, &readSet);
	
	
	// ok here it starts, checking for new connections
	selectResult = select(ServerTCPSocket+1, &readSet, NULL, NULL, &timer);
	*/
	if(canRead(ServerTCPSocket))//if(selectResult > 0) // yay new connections
	{
		fprintf(stderr, "\nnew connection, calling accept()");
		ClientTCPSocket = accept(ServerTCPSocket, (struct sockaddr *) &peer_addr, &peer_addr_size);
		if (ClientTCPSocket == -1)
			fprintf(stderr, "\nError: accept: %s",strerror(errno));
	}
	/*else if(selectResult == -1)
		fprintf(stderr, "\nError: select: %s",strerror(errno));*/
}