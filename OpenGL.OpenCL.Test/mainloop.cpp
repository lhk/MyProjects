#include <windows.h>
#include "guicon.h"
#include <stdio.h>
#include <gl/GL.h>
#include <gl/GLU.h>
#include <CL/cl.h>
#include <CL/cl_gl.h>
#include <time.h>
#include <math.h>
#include "myOpenCLContext.h"
#define SQUARE(x) x*x

typedef struct float2 { float x, y; } float2;

typedef struct Particle 
{ 
	float pX;
	float pY;
	float vX;
	float vY; 
} Particle;

typedef struct KernelParameters 
{ 
	float2 gravity;
	float dt;
	float radius;
	int particleCount;
} KernelParameters;


int checkCLError(cl_int err)
{
	if(err < 0) 
	{
		perror("ERROR: ");
		return 0;
	}
	return 1;
}

float random()
{
	return (rand() / (float)RAND_MAX) * 2 - 1;
}

float FastInverseSquareRoot( float y )
{
    long i = 0x5f3759df-((*(long*)&y)>>1);
    return (*(float*)&i)*(1.5F-(y*0.5f*(*(float*)&i)*(*(float*)&i)));
}

int PhysicsUpdateOpenCL(Particle *Particles, myOpenCLContext &my_cl_context, KernelParameters &kernelParameters)
{
	// refresh parameters 
	if(!checkCLError(clEnqueueWriteBuffer(my_cl_context.queue, my_cl_context.buffer[2], CL_TRUE, 0, sizeof(KernelParameters), &kernelParameters, 0, NULL, NULL))) return 0;

	// collision kernel O(n^2)
	{
		size_t global_size = kernelParameters.particleCount * kernelParameters.particleCount;
		global_size = (global_size/64+1)*64;
		size_t local_size = 64;
		if(!checkCLError(clEnqueueNDRangeKernel(my_cl_context.queue, my_cl_context.kernel[0], 1, NULL, &global_size, &local_size, 0, NULL, NULL))) return 0; 
	}

	// motion kernel O(n)
	{
		size_t global_size = kernelParameters.particleCount;
		global_size = (global_size/64+1)*64;
		size_t local_size = 64;
		if(!checkCLError(clEnqueueNDRangeKernel(my_cl_context.queue, my_cl_context.kernel[1], 1, NULL, &global_size, &local_size, 0, NULL, NULL))) return 0; 
	}

	// Read the kernel's output 
	if(!checkCLError(clEnqueueReadBuffer(my_cl_context.queue, my_cl_context.buffer[0], CL_TRUE, 0, kernelParameters.particleCount * sizeof(Particle), Particles, 0, NULL, NULL))) return 0; 

	return 1;
}

int MainLoop(HDC hDC, float AspectRatio, myOpenCLContext &my_cl_context)
{
	clock_t FrameCounterStartTime = clock();
	int FrameCounter = 0;

	KernelParameters kernelParameters;
	kernelParameters.radius = .01f;
	kernelParameters.dt = .001f;
	kernelParameters.gravity.x = 0;
	kernelParameters.gravity.y = 0;
	kernelParameters.particleCount = 5000;

	Particle* Particles = new Particle[ kernelParameters.particleCount ];
	float2* acceleration = new float2[ kernelParameters.particleCount * kernelParameters.particleCount ];
	for(int i = 0; i < kernelParameters.particleCount; i++)
	{
		Particles[i].pX = random();
		Particles[i].pY = random();
		Particles[i].vX = random() * 0.0001;
		Particles[i].vY = random() * 0.0001;

		for(int j = 0; j < kernelParameters.particleCount; j++)
		{
			acceleration[i*kernelParameters.particleCount+j].x = 0;
			acceleration[i*kernelParameters.particleCount+j].y = 0;
		}
	}
	

	// Create CL buffer
	cl_int err;
	my_cl_context.buffer[0] = clCreateBuffer(my_cl_context.context, CL_MEM_READ_WRITE | CL_MEM_COPY_HOST_PTR, kernelParameters.particleCount * sizeof(Particle), Particles, &err);
	if(!checkCLError(err)) return 0;

	my_cl_context.buffer[1] = clCreateBuffer(my_cl_context.context, CL_MEM_READ_WRITE | CL_MEM_COPY_HOST_PTR , kernelParameters.particleCount * kernelParameters.particleCount * sizeof(float2), acceleration, &err);
	if(!checkCLError(err)) return 0;

	my_cl_context.buffer[2] = clCreateBuffer(my_cl_context.context, CL_MEM_READ_WRITE | CL_MEM_COPY_HOST_PTR , sizeof(KernelParameters), &kernelParameters, &err);
	if(!checkCLError(err)) return 0;

	// Create kernel arguments
	if(!checkCLError(
		clSetKernelArg(my_cl_context.kernel[0], 0, sizeof(cl_mem), &my_cl_context.buffer[0]) | 
		clSetKernelArg(my_cl_context.kernel[0], 1, sizeof(cl_mem), &my_cl_context.buffer[1]) |
		clSetKernelArg(my_cl_context.kernel[0], 2, sizeof(cl_mem), &my_cl_context.buffer[2])))
		return 0;

	if(!checkCLError(
		clSetKernelArg(my_cl_context.kernel[1], 0, sizeof(cl_mem), &my_cl_context.buffer[0]) | 
		clSetKernelArg(my_cl_context.kernel[1], 1, sizeof(cl_mem), &my_cl_context.buffer[1]) |
		clSetKernelArg(my_cl_context.kernel[1], 2, sizeof(cl_mem), &my_cl_context.buffer[2])))
		return 0;

	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	float fov = 2;
	gluOrtho2D(-fov*AspectRatio,fov*AspectRatio,-fov,fov);
	glMatrixMode(GL_MODELVIEW);

	while(true)
    {
		// FPS counter
		if((float)(clock()-FrameCounterStartTime) / CLOCKS_PER_SEC > 2)
		{
			float fc_dt = (float)(clock()-FrameCounterStartTime) / CLOCKS_PER_SEC;
			float FPS = FrameCounter/fc_dt;
			printf("%f FPS\n", FPS);
			FrameCounterStartTime = clock();
			FrameCounter = 0;
		}

		// window messages
		MSG Msg;
		if(PeekMessage( &Msg, NULL, 0, 0, PM_REMOVE ))
		{
			if ( Msg.message == WM_QUIT )  return Msg.wParam;
			TranslateMessage(&Msg);
			DispatchMessage(&Msg);
		}

		// input
		kernelParameters.gravity.x=0;
		if(GetKeyState(VK_LEFT) & 0xff00) kernelParameters.gravity.x=-6;
		if(GetKeyState(VK_RIGHT) & 0xff00) kernelParameters.gravity.x=6; 

		// update
		PhysicsUpdateOpenCL(Particles, my_cl_context, kernelParameters);


		// draw
		glClearColor( 0.0f, 0.0f, 0.0f, 0.0f );
		glClear( GL_COLOR_BUFFER_BIT );
		glPushMatrix();
		glBegin( GL_TRIANGLES );
		glColor3f( 1.0f,1,1 ); 


		float maxSqrdVel = 0;
		for(int i = 0; i < kernelParameters.particleCount; i++)
		{
			float sqrtVel;
			if((sqrtVel = (SQUARE(Particles[i].vX) + SQUARE(Particles[i].vY))) > maxSqrdVel) maxSqrdVel = sqrtVel;

			float scale = .3f;
			glVertex2f( Particles[i].pX - kernelParameters.radius * scale, Particles[i].pY - kernelParameters.radius * scale);
			glVertex2f( Particles[i].pX + kernelParameters.radius * scale, Particles[i].pY - kernelParameters.radius * scale);
			glVertex2f( Particles[i].pX + kernelParameters.radius * scale, Particles[i].pY + kernelParameters.radius * scale);

			glVertex2f( Particles[i].pX - kernelParameters.radius * scale, Particles[i].pY - kernelParameters.radius * scale);
			glVertex2f( Particles[i].pX + kernelParameters.radius * scale, Particles[i].pY + kernelParameters.radius * scale);
			glVertex2f( Particles[i].pX - kernelParameters.radius * scale, Particles[i].pY + kernelParameters.radius * scale);
		}
		//kernelParameters.dt = min(1,max(0.0001f, (1.0f / maxSqrdVel)));
		//kernelParameters.dt = 1.0f / maxSqrdVel;
		printf("dt: %f - maxVel: %f\n", kernelParameters.dt, maxSqrdVel);

		glEnd();
		glPopMatrix();

		SwapBuffers(hDC);
		FrameCounter++;

		//Sleep(1);
    }
}