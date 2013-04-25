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
		fprintf(stderr, "\nError: select: %s",strerror(errno));
		
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
		fprintf(stderr, "\nError: select: %s",strerror(errno));
		
	return selectResult > 0;
}