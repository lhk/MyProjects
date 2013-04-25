void microSleep(long long time)
{
	struct timeval t;
	t.tv_sec = time/1000000;
    t.tv_usec = time%1000000;
	select(0,NULL,NULL,NULL,&t);
}

long long getTime()
{
	struct timeval now;
	gettimeofday(&now, NULL);
	return now.tv_sec*1000000 + now.tv_usec;
}

void FpsLimiter(long long &loopTimer, long long &dt)
{
	long long now = getTime();
	long long activeTime = now - loopTimer;
	long long idleTime = (1000000/MAX_FPS) - activeTime;
	
	microSleep(idleTime);

	dt = getTime() - loopTimer;
	loopTimer = getTime(); // mark the beginning of the next loop
}

void hexDump (void *addr, int len) {
    int i;
    unsigned char buff[17];
    unsigned char *pc = (unsigned char *)addr;



    // Process every byte in the data.
    for (i = 0; i < len; i++) {
        // Multiple of 16 means new line (with line offset).

        if ((i % 16) == 0) {
            // Just don't print ASCII for the zeroth line.
            if (i != 0)
                printf ("  %s\n", buff);

            // Output the offset.
            printf ("  %04x ", i);
        }

        // Now the hex code for the specific character.
        printf (" %02x", pc[i]);

        // And store a printable ASCII character for later.
        if ((pc[i] < 0x20) || (pc[i] > 0x7e))
            buff[i % 16] = '.';
        else
            buff[i % 16] = pc[i];
        buff[(i % 16) + 1] = '\0';
    }

    // Pad out last line if not exactly 16 characters.
    while ((i % 16) != 0) {
        printf ("   ");
        i++;
    }

    // And print the final ASCII bit.
    printf ("  %s\n", buff);
}