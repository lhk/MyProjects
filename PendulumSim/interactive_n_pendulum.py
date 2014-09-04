import pygame
import numpy as np
import time
import sys
from nPendulum import nPendulum

p = nPendulum(sys.argv[1])

fps_count = 0
fps_start = time.time()

# Define some colors
BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

def draw_pendulum(pendulum):
	c=.2

	p = pendulum.getPoints()
	x = p[0][0]

	for p1,p2 in zip(p[1:],p[:-1]):
		pygame.draw.line(screen, BLACK, transf(p1), transf(p2), 5)

	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x+c,0-c]), 5)
	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x+c,0-c]), 5)

	#pygame.draw.line(screen, RED, transf(p2), transf(p3), 5)
	#pygame.draw.line(screen, RED, transf(p1), transf(p4), 5)


pygame.init()
size = [1600, 1000]
screen = pygame.display.set_mode(size)
pygame.display.set_caption("My Game")
done = False
clock = pygame.time.Clock()
pygame.mouse.set_visible(0)
F1 = 0

def transf(v):
	M = [
		[100,   0, size[0]/2],
		[0  ,-100, size[1]/2],
		[0  ,   0,   1]
		]
	v=np.append(v,1)
	return (np.matrix(M)*np.matrix(v).T).T.tolist()[0][:-1]

# -------- Main Program Loop -----------
while not done:
	for event in pygame.event.get(): 
		if event.type == pygame.QUIT: 
			done = True
	
		elif event.type == pygame.KEYDOWN:
			if event.key == pygame.K_LEFT:
				F1 = -100
			elif event.key == pygame.K_RIGHT:
				F1 = 100
		
		elif event.type == pygame.KEYUP:
			if event.key == pygame.K_LEFT:
				F1=0
			elif event.key == pygame.K_RIGHT:
				F1=0		  
	p.fx[0] = F1

	p.timestep(0.005)

	screen.fill(WHITE)
	draw_pendulum(p)

	fps_count+=1
	if fps_count > 60:
		print 'FPS: ', fps_count/(1.0*time.time()-fps_start)
		fps_count = 0
		fps_start = time.time()

	pygame.display.flip()
	#clock.tick(60)
pygame.quit()