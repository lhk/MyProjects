#ifndef __MAINLOOP_H__
#define __MAINLOOP_H__
#include <windows.h>
#include <CL/cl.h>
#include "myOpenCLContext.h"
int MainLoop(HDC hDC, float AspectRatio, myOpenCLContext &my_cl_context);
#endif