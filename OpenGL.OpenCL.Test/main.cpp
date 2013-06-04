
#include "guicon.h"
#include "mainloop.h"
#include "myOpenCLContext.h"
#include <windows.h>
#include <stdio.h>
#include <gl/GL.h>
#include <math.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <CL/cl.h>

#define PROGRAM_FILE "particles_kernel.cl"


const LPCWSTR g_szClassName = L"myWindowClass";

// Step 4: the Window Procedure
LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch(msg)
    {
        case WM_CLOSE:
            DestroyWindow(hwnd); 
        break;
        case WM_DESTROY:
            PostQuitMessage(0);
        break;
        default:
            return DefWindowProc(hwnd, msg, wParam, lParam);
    }
    return 0;
}

int InitWindowOpenGL(HWND &hwnd, HDC &hDC, HINSTANCE &hInstance, int &nCmdShow)
{
	WNDCLASSEX wc;


    //Step 1: Registering the Window Class
    wc.cbSize        = sizeof(WNDCLASSEX);
    wc.style         = 0;
    wc.lpfnWndProc   = WndProc;
    wc.cbClsExtra    = 0;
    wc.cbWndExtra    = 0;
    wc.hInstance     = hInstance;
    wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = g_szClassName;
    wc.hIconSm       = LoadIcon(NULL, IDI_APPLICATION);

    if(!RegisterClassEx(&wc))
    {
        MessageBox(NULL, L"Window Registration Failed!", L"Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
    }

    // Step 2: Creating the Window
    hwnd = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        g_szClassName,
        L"The title of my window",
        WS_OVERLAPPEDWINDOW | WS_MAXIMIZE,
        CW_USEDEFAULT, CW_USEDEFAULT, 200, 200,
        NULL, NULL, hInstance, NULL);

    if(hwnd == NULL)
    {
        MessageBox(NULL, L"Window Creation Failed!", L"Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
    }

    ShowWindow(hwnd, nCmdShow);
    UpdateWindow(hwnd);
	SendMessage(hwnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
	
	hDC = GetDC( hwnd );

	PIXELFORMATDESCRIPTOR pfd;
	ZeroMemory( &pfd, sizeof( pfd ) );
	pfd.nSize = sizeof( pfd );
	pfd.nVersion = 1;
	pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
	pfd.iPixelType = PFD_TYPE_RGBA;
	pfd.cColorBits = 24;
	pfd.cDepthBits = 16;
	pfd.iLayerType = PFD_MAIN_PLANE;
	int iFormat = ChoosePixelFormat( hDC, &pfd );
	SetPixelFormat( hDC, iFormat, &pfd );

	HGLRC hRC;
	hRC = wglCreateContext( hDC );
	wglMakeCurrent( hDC, hRC );

	return 1;
}

/* Find a GPU or CPU associated with the first available platform */
int create_device(myOpenCLContext &my_cl_context) 
{
   int err;

   /* Identify a platform */
   err = clGetPlatformIDs(1, &my_cl_context.platform, NULL);
   if(err < 0) {
      perror("Couldn't identify a platform");
      return 0;
   }

   /* Access a device */
   err = clGetDeviceIDs(my_cl_context.platform, CL_DEVICE_TYPE_GPU, 1, &my_cl_context.device, NULL);
   if(err == CL_DEVICE_NOT_FOUND) {
	   err = clGetDeviceIDs(my_cl_context.platform, CL_DEVICE_TYPE_CPU, 1, &my_cl_context.device, NULL);
   }
   if(err < 0) {
      perror("Couldn't access any devices");
      return 0;
   }

   return 1;
}

/* Create program from a file and compile it */
int build_program(myOpenCLContext &my_cl_context, const char* filename) 
{
   FILE *program_handle;
   char *program_buffer, *program_log;
   size_t program_size, log_size;
   int err;

   /* Read program file and place content into buffer */
   program_handle = fopen(filename, "r");
   if(program_handle == NULL) {
      perror("Couldn't find the program file");
      return 0;
   }
   fseek(program_handle, 0, SEEK_END);
   program_size = ftell(program_handle);
   rewind(program_handle);
   program_buffer = (char*)malloc(program_size + 1);
   program_buffer[program_size] = '\0';
   fread(program_buffer, sizeof(char), program_size, program_handle);
   fclose(program_handle);

   /* Create program from file */
   my_cl_context.program = clCreateProgramWithSource(my_cl_context.context, 1, (const char**)&program_buffer, &program_size, &err);
   if(err < 0) {
      perror("Couldn't create the program");
      return 0;
   }
   free(program_buffer);

   /* Build program */
   err = clBuildProgram(my_cl_context.program, 0, NULL, NULL, NULL, NULL);
   if(err < 0) {

      /* Find size of log and print to std output */
      clGetProgramBuildInfo(my_cl_context.program, my_cl_context.device, CL_PROGRAM_BUILD_LOG, 0, NULL, &log_size);
      program_log = (char*) malloc(log_size + 1);
      program_log[log_size] = '\0';
      clGetProgramBuildInfo(my_cl_context.program, my_cl_context.device, CL_PROGRAM_BUILD_LOG, log_size + 1, program_log, NULL);
      printf("%s\n", program_log);
      free(program_log);
      return 0;
   }

   return 1;
}

void print_device_info(myOpenCLContext &my_cl_context)
{
	size_t param_value_size = 10000;
	char param_value[10000];	
 	size_t param_value_size_ret;

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_NAME, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_NAME: %s\n", param_value);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_VENDOR, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_VENDOR: %s\n", param_value);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_VERSION, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_VERSION: %s\n", param_value);

	clGetDeviceInfo(my_cl_context.device, CL_DRIVER_VERSION, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DRIVER_VERSION: %s\n", param_value);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_GLOBAL_MEM_SIZE, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_GLOBAL_MEM_SIZE: %lu\n", ((cl_ulong*)param_value)[0]);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_LOCAL_MEM_SIZE, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_LOCAL_MEM_SIZE: %lu\n", ((cl_ulong*)param_value)[0]);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_MAX_COMPUTE_UNITS, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_MAX_COMPUTE_UNITS: %u\n", ((cl_uint*)param_value)[0]);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_MAX_WORK_GROUP_SIZE, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_MAX_WORK_GROUP_SIZE: %u\n", ((size_t*)param_value)[0]);

	clGetDeviceInfo(my_cl_context.device, CL_DEVICE_MAX_WORK_ITEM_SIZES, param_value_size, param_value, &param_value_size_ret);
	printf("CL_DEVICE_MAX_WORK_ITEM_SIZES: %u / %u / %u\n", ((size_t*)param_value)[0], ((size_t*)param_value)[1], ((size_t*)param_value)[2]);

	clGetKernelWorkGroupInfo(my_cl_context.kernel[0], my_cl_context.device, CL_KERNEL_WORK_GROUP_SIZE, param_value_size, param_value, &param_value_size_ret);
	printf("CL_KERNEL_WORK_GROUP_SIZE: %u\n", ((size_t*)param_value)[0]);

	clGetKernelWorkGroupInfo(my_cl_context.kernel[0], my_cl_context.device, CL_KERNEL_LOCAL_MEM_SIZE, param_value_size, param_value, &param_value_size_ret);
	printf("CL_KERNEL_LOCAL_MEM_SIZE: %u\n", ((cl_ulong*)param_value)[0]);

}

int InitOpenCL(myOpenCLContext &my_cl_context) { 

   /* OpenCL structures */
   cl_int err;

   /* Create device and context */
   if(!create_device(my_cl_context)) return 0;
   my_cl_context.context = clCreateContext(NULL, 1, &my_cl_context.device, NULL, NULL, &err);
   if(err < 0) {
      perror("Couldn't create a context");
      return 0;
   }

   /* Build program */
   if(!build_program(my_cl_context, PROGRAM_FILE)) return 0;

	/* Create a command queue */
   my_cl_context.queue = clCreateCommandQueue(my_cl_context.context, my_cl_context.device, 0, &err);
   if(err < 0) {
      perror("Couldn't create a command queue");
      return 0;
   };

   /* Create a kernel */
   my_cl_context.kernel[0] = clCreateKernel(my_cl_context.program, "collision", &err);
   if(err < 0) {
      perror("Couldn't create a kernel");
      return 0;
   };

   my_cl_context.kernel[1] = clCreateKernel(my_cl_context.program, "motion", &err);
   if(err < 0) {
      perror("Couldn't create a kernel");
      return 0;
   };
   
   print_device_info(my_cl_context);

   return 1;
}


int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	HWND hwnd;	
	HDC hDC;
	myOpenCLContext my_cl_context;

	RedirectIOToConsole();

	if(InitOpenCL(my_cl_context) && InitWindowOpenGL(hwnd, hDC, hInstance, nCmdShow))
	{
		float AspectRatio = 1;
		RECT rect;
		if(GetWindowRect(hwnd, &rect)) AspectRatio = (float)(rect.right - rect.left)/(float)(rect.bottom - rect.top);
		return MainLoop(hDC, AspectRatio, my_cl_context);  
	}
	else getchar();
	return 1;
}