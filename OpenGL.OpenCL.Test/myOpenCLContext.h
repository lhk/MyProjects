#ifndef __MYOPENCLCONTEXT_H__
#define __MYOPENCLCONTEXT_H__
#include <CL/cl.h>

typedef struct myOpenCLContext 
{ 
	cl_context context;
	cl_kernel kernel[2];
	cl_command_queue queue;
	cl_platform_id platform;
	cl_device_id device;
    cl_program program;
	cl_mem buffer[3];

} myOpenCLContext;

#endif