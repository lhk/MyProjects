from numpy import sin,cos
import pygame
import numpy as np
import time
import sys
from rocketPhysics import rocketPhysics

rocket = rocketPhysics()

fps_count = 0
fps_start = time.time()

# Define some colors
BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

def draw_rocket(rocket):

	cg = np.array([rocket.x,rocket.y])
	top = cg + rocket.height/2* np.array([cos(rocket.pitch),sin(rocket.pitch)])
	bottom = cg - rocket.height/2* np.array([cos(rocket.pitch),sin(rocket.pitch)])
	plume_end = bottom - rocket.throttle *5* np.array([cos(rocket.pitch + rocket.gimbal),sin(rocket.pitch + rocket.gimbal)])

	pygame.draw.line(screen, BLACK, transf(top), transf(bottom), 5)
	pygame.draw.line(screen, RED, transf(plume_end), transf(bottom), 5)
	#pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x-c,0+c]), 5)
	#pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x-c,0+c]), 5)
	#pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x+c,0-c]), 5)

	#pygame.draw.line(screen, RED, transf(p2), transf(p3), 5)
	#pygame.draw.line(screen, RED, transf(p1), transf(p4), 5)


pygame.init()
size = [1600, 1000]
screen = pygame.display.set_mode(size)
pygame.display.set_caption("My Game")
done = False
clock = pygame.time.Clock()
pygame.mouse.set_visible(0)


def transf(v):
	M = [
		[10,   0, size[0]/2],
		[0  ,-10, size[1]/2],
		[0  ,   0,   1]
		]
	v=np.append(v,1)
	return (np.matrix(M)*np.matrix(v).T).T.tolist()[0][:-1]

keystates = {}

# -------- Main Program Loop -----------
while not done:
	for event in pygame.event.get(): 
		if event.type == pygame.QUIT: 
			done = True
	
		elif event.type == pygame.KEYDOWN:
			keystates[event.key] = True		
		elif event.type == pygame.KEYUP:
			keystates[event.key] = False


	rocket.throttle = 1 if pygame.K_UP in keystates and keystates[pygame.K_UP] else 0
	rocket.gimbal = 0
	if pygame.K_LEFT in keystates and keystates[pygame.K_LEFT]: rocket.gimbal = -1
	if pygame.K_RIGHT in keystates and keystates[pygame.K_RIGHT]: rocket.gimbal = 1
	
	rocket.timestep(0.005)

	screen.fill(WHITE)
	draw_rocket(rocket)

	fps_count+=1
	if fps_count > 60:
		print 'FPS: ', fps_count/(1.0*time.time()-fps_start)
		fps_count = 0
		fps_start = time.time()

	pygame.display.flip()
pygame.quit()