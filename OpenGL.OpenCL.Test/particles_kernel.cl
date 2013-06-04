
#define SQUARE(x) x*x
#define SGN(x) (x>0)?1:((x<0)?-1:0)
#define SPEED_LIMIT 100


typedef struct KernelParameters 
{ 
	float2 gravity;
	float dt;
	float radius;
	int particleCount;
} KernelParameters;

float FastInverseSquareRoot( float y )
{
    long i = 0x5f3759df - ( (* ( long * ) &y) >> 1 );
	return (* ( float * ) &i) * ( 1.5F - ( y * 0.5f * (* ( float * ) &i) * (* ( float * ) &i) ) );
}

__kernel void collision(__global float4* data, __global float2* result, __global KernelParameters* p_kp)
{
	KernelParameters kp = *p_kp;
	int i = get_global_id(0) / kp.particleCount;
	int j = get_global_id(0) % kp.particleCount;
	
	
	if(i<j && i<kp.particleCount && j<kp.particleCount)
	{
		float2 pos_i, vel_i, pos_j, vel_j, delta_pos, new_vel_i, new_vel_j, normdelta_pos, avgVel, acel_i, acel_j;
		float squaredDistance;
		
		pos_i = data[i].lo;
		vel_i = data[i].hi;
		
		pos_j = data[j].lo;
		vel_j = data[j].hi;
		
		delta_pos = pos_j - pos_i;
	
		
		if((squaredDistance = (SQUARE(delta_pos.x) + SQUARE(delta_pos.y))) < SQUARE(2 * kp.radius))
		{
			float inverseDistance, distance, penetration, weight;
			
			new_vel_i = vel_i;
			new_vel_j = vel_j;

			inverseDistance = FastInverseSquareRoot(squaredDistance);
			distance = 1/inverseDistance;
			normdelta_pos = delta_pos * inverseDistance;

			// separate particles

			penetration = 2 * kp.radius - distance;
			penetration = 2000*penetration;
			new_vel_j += normdelta_pos * penetration;
			new_vel_i -= normdelta_pos * penetration;

			// viscosity
			weight = 1;
			avgVel = (new_vel_i + new_vel_j) / 2;
			new_vel_j = (weight * new_vel_j + avgVel) / (weight + 1);
			new_vel_i = (weight * new_vel_i + avgVel) / (weight + 1);


			acel_i = new_vel_i - vel_i;
			acel_j = new_vel_j - vel_j;

			result[i*kp.particleCount+j] = acel_i;
			result[j*kp.particleCount+i] = acel_j;

		}
	}
}


__kernel void motion(__global float4* data, __global float2* result, __global KernelParameters* p_kp)
{
	KernelParameters kp = *p_kp;
	int i = get_global_id(0);
	if(i < kp.particleCount)
	{
		float2 pos_i, vel_i, deltaVel = (float2)(0,0), gravity = (float2)(0,-10);
		float AspectRatio = 1.6f;
		float top = -1, bottom = 1, left = -1*AspectRatio, right = 1*AspectRatio;
		
		pos_i = data[i].lo;
		vel_i = data[i].hi;
		
		// accumulate the deltaVelocities from all collisions
		for(int j = 0; j < kp.particleCount; j++)
		{
			deltaVel += result[i*kp.particleCount+j];
			result[i*kp.particleCount+j] = (float2)(0,0);
		}
		
		// acceleration due to collision
		vel_i += deltaVel;	

		// acceleration due to gravity
		vel_i += gravity * kp.dt * .1f;
		
		// speedlimit
		//vel_i.x = SGN(vel_i.x) * fmin(SPEED_LIMIT, fabs(vel_i.x));
		//vel_i.y = SGN(vel_i.y) * fmin(SPEED_LIMIT, fabs(vel_i.y));
		
		// ds = v * dt
		pos_i += vel_i * kp.dt;

		if(pos_i.x < left)
		{
			pos_i.x = left;
			vel_i.x = fabs(vel_i.x);
		}
		if(pos_i.x > right)
		{
			pos_i.x = right;
			vel_i.x = -fabs(vel_i.x);
		}
		if(pos_i.y < top)
		{
			pos_i.y = top;
			vel_i.y = fabs(vel_i.y);
		}
		if(pos_i.y > bottom)
		{
			pos_i.y = bottom;
			vel_i.y = -fabs(vel_i.y);
		}
		
		
		data[i].lo = pos_i;
		data[i].hi = vel_i;	
	}
}