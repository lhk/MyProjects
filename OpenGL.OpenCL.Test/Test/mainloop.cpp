#include <windows.h>
#include "guicon.h"
#include <stdio.h>
#include <gl/GL.h>
#include <gl/GLU.h>
#include <time.h>

int MainLoop(HDC hDC, float AspectRatio)
{
    MSG Msg;
	float theta = 0;

	clock_t LastFrameStartTime = clock();
	clock_t FrameCounterStartTime = clock();
	int FrameCounter = 0;


	while(true)
    {
		float dt = (float)(clock()-LastFrameStartTime) / CLOCKS_PER_SEC;
		LastFrameStartTime = clock();

		if((float)(clock()-FrameCounterStartTime) / CLOCKS_PER_SEC > 1)
		{
			float fc_dt = (float)(clock()-FrameCounterStartTime) / CLOCKS_PER_SEC;
			printf("%f FPS\n", FrameCounter/fc_dt);
			FrameCounterStartTime = clock();
			FrameCounter = 0;
		}

		if(PeekMessage( &Msg, NULL, 0, 0, PM_REMOVE ))
		{
			if ( Msg.message == WM_QUIT )  return Msg.wParam;
			TranslateMessage(&Msg);
			DispatchMessage(&Msg);
		}


		glClearColor( 0.0f, 0.0f, 0.0f, 0.0f );
		glClear( GL_COLOR_BUFFER_BIT );

		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		gluOrtho2D(-3*AspectRatio,3*AspectRatio,-3,3);


		glMatrixMode(GL_MODELVIEW);
		glPushMatrix();

		glRotatef( theta, 0.0f, 0.0f, 1.0f );
		glBegin( GL_TRIANGLES );
		glColor3f( 1.0f, 0.0f, 0.0f ); glVertex2f( 0.0f, 1.0f );
		glColor3f( 0.0f, 1.0f, 0.0f ); glVertex2f( 0.87f, -0.5f );
		glColor3f( 0.0f, 0.0f, 1.0f ); glVertex2f( -0.87f, -0.5f );
		glEnd();

		glPopMatrix();
		
		theta += 1e-2f;

		SwapBuffers(hDC);
		FrameCounter++;
    }
}