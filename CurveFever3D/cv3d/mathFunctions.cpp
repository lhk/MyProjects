
class Vector3
{
	public:
	float x,y,z;
	
	Vector3 operator+(Vector3 v1)
	{
		Vector3 v;
		v.x = v1.x + this->x;
		v.y = v1.y + this->y;
		v.z = v1.z + this->z;
		return v;
	}
	
	Vector3 operator*(float f)
	{
		Vector3 v;
		v.x = f * this->x;
		v.y = f * this->y;
		v.z = f * this->z;
		return v;
	}
	
	Vector3 operator-()
	{
		Vector3 v;
		v.x = - this->x;
		v.y = - this->y;
		v.z = - this->z;
		return v;
	}
	
	char* toString()
	{
		snprintf((char*)&glob_PrintBuffer,GLOBAL_PRINT_BUFFER_SIZE,"{%f:%f:%f}",this->x,this->y,this->z);
		glob_PrintBuffer[GLOBAL_PRINT_BUFFER_SIZE-1] = 0;
		return (char*)&glob_PrintBuffer;
	}
};