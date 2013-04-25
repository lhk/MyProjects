
class Player 
{
	public:
	bool exists;
	bool readyUp;
	struct in_addr ip;
	unsigned short port;
	int socketTCP;
	struct timeval lastContact;
	int timeout;
	vector<Vector3> path;
	
	Vector3 position;
	float pitch, yaw;
	
	void updatePosition(float dt)
	{
		Vector3 d;
		d.x = (sinf(yaw) * cosf(pitch));
		d.z = (-cosf(yaw) * cosf(pitch));
		d.y = (-sinf(pitch));
		d = d * dt;
		position = position + d;
		printf("Pos: %s\n", this->position.toString());
	}
	
	void resetTimeout()
	{
		gettimeofday(&(this->lastContact), NULL);
	}
	
	void dispose()
	{
		exists = false;
		readyUp = false;
		//ip = 0;
		port = 0;
		socketTCP = -1;
		//lastContact = 0;
		timeout = 0;
	}
	
	char* endpointToString()
	{
		snprintf((char*)&glob_PrintBuffer,GLOBAL_PRINT_BUFFER_SIZE,"%s:%i",inet_ntoa(this->ip),this->port);
		glob_PrintBuffer[GLOBAL_PRINT_BUFFER_SIZE-1] = 0;
		return (char*)&glob_PrintBuffer;
	}
	
	bool timeoutExpired()
	{
		struct timeval now;
		gettimeofday(&now, NULL);
		//fprintf(stderr,"\nTimeout Test; LC %i; Now %i\n",lastContact.tv_sec,now.tv_sec);
		return (lastContact.tv_sec + timeout < now.tv_sec);
	}

	void send(char* name, char* value)
	{
		int bufsize = 1024*4;
		char sendBuffer[bufsize];
		snprintf((char*)&sendBuffer,bufsize-1,"%s=%s\n",name,value);
		sendBuffer[bufsize-1] = 0;
		write(this->socketTCP,&sendBuffer, strnlen((char*)&sendBuffer,bufsize));
	}
	
	bool receive()
	{
		char readBuffer[4096];
		char nameBuffer[4096];
		char valueBuffer[4096];
		int nameBufferIndex = 0;
		int valueBufferIndex = 0;
		bool copyToNameBuffer = true;
		
		int bytes = read(this->socketTCP, &readBuffer, 4096-1);
		if(bytes > 0) // data received
		{
			this->resetTimeout();
			readBuffer[bytes] = 0;
			//printf("Msg from %s n:%i \"%s\"\n",this->endpointToString(),bytes,&readBuffer);
			
			for(int i = 0; i<bytes; i++)
			{
				if(readBuffer[i] == 0) break;
				else if(readBuffer[i] == '=') copyToNameBuffer = false;
				else if(readBuffer[i] == '\n') 
				{
					nameBuffer[nameBufferIndex+1] = 0;
					valueBuffer[valueBufferIndex+1] = 0;
					
					this->interpretCommand((char*)&nameBuffer,(char*)&valueBuffer);
					
					copyToNameBuffer = true;
					nameBufferIndex = 0;
					valueBufferIndex = 0;
				}
				else if(0x20 < readBuffer[i] && readBuffer[i] < 0x7f)
				{
					if(copyToNameBuffer) nameBuffer[nameBufferIndex++] = readBuffer[i];
					else valueBuffer[valueBufferIndex++] = readBuffer[i];
				}
			}
		}
		else // connection closed/broken
		{
			printf("Disconnected %s\n",this->endpointToString());
			shutdown(this->socketTCP, SHUT_RDWR);
			this->dispose();
			return false;
		}
		
		return true;
	}
		
	void interpretCommand(char* name, char* value)
	{
		if(strncmp("Yaw",name,3)==0)
		{
			sscanf(value, "%f", &(this->yaw));
			printf("Yaw:f:%f - s:%s\n",this->yaw,value);
		}
		else if(strncmp("Pitch",name,5)==0)
		{
			sscanf(value, "%f", &(this->pitch));
			printf("Pitch:f:%f - s:%s\n",this->pitch,value);
		}
	}
};