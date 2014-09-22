from math import exp,log,sin,cos,pi
import pygame
import numpy as np
from copy import copy
import time
import sys
import os
from RocketPhysics import RocketPhysics
from RocketController import RocketController
from RocketController2 import RocketController2

rocket = RocketPhysics()
controller = RocketController2(rocket)

rocket.resetScenario(2)

fps_count = 0
fps_start = time.time()

BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

# affine transform for drawing
def transf(v):
	zoom = 2.6
	M = [
		[zoom,   0, size[0]/2],
		[0  ,-zoom, size[1]*11.0/12],
		[0  ,   0,   1]
		]
	v=np.append(v,1)
	return (np.matrix(M)*np.matrix(v).T).T.tolist()[0][:-1]

def draw_rocket(rocket):

	e_f = np.array([cos(rocket.pitch),sin(rocket.pitch)])
	e_l = np.array([-sin(rocket.pitch),cos(rocket.pitch)])
	cg = np.array([rocket.x,rocket.y])
	top = cg + rocket.height/2* e_f
	bottom = cg - rocket.height/2* e_f
	plume_end = bottom - rocket.throttle *5* np.array([cos(rocket.pitch + rocket.gimbal),sin(rocket.pitch + rocket.gimbal)])

	pygame.draw.line(screen, BLACK, transf(top-(e_f+e_l)*rocket.core_radius), transf(bottom-e_l*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(top-(e_f+e_l)*rocket.core_radius), transf(top), 2)
	pygame.draw.line(screen, BLACK, transf(top-(e_f-e_l)*rocket.core_radius), transf(top), 2)
	pygame.draw.line(screen, BLACK, transf(top-(e_f-e_l)*rocket.core_radius), transf(bottom+e_l*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(bottom-e_l*rocket.core_radius), transf(bottom+e_l*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(bottom-e_l*rocket.core_radius), transf(bottom-(4*e_l+2*e_f)*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(bottom-(e_l-2*e_f)*rocket.core_radius), transf(bottom-(4*e_l+2*e_f)*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(bottom+e_l*rocket.core_radius), transf(bottom-(-4*e_l+2*e_f)*rocket.core_radius), 2)
	pygame.draw.line(screen, BLACK, transf(bottom+(e_l+2*e_f)*rocket.core_radius), transf(bottom-(-4*e_l+2*e_f)*rocket.core_radius), 2)
	
	if(rocket.throttle > 0.01): pygame.draw.line(screen, RED, transf(plume_end), transf(bottom), 5)

	pygame.draw.line(screen, BLACK, transf([-10000,0]), transf([10000,0]), 2)
	for i in range(-10,11):
		c = 5
		pygame.draw.line(screen, BLACK, transf([i*c,-c]), transf([i*c+c,0]), 2)

	if len(rocket.trajectory)>1:
		pygame.draw.lines(screen, ( 199, 199, 199), False, [transf(p) for p in rocket.trajectory], 2)


pygame.init()
size = [1600, 1000]
screen = pygame.display.set_mode(size)
done = False
manualControl = False
simulationRunning = True
fillColor = WHITE

keystates = {}
keystates_old = {}
fps = 60

while not done:
	# input
	keystates_old = copy(keystates)
	for event in pygame.event.get(): 
		if event.type == pygame.QUIT: 
			done = True	
		elif event.type == pygame.KEYDOWN:
			keystates[event.key] = True
		elif event.type == pygame.KEYUP:
			keystates[event.key] = False

	# space pressed
	if (not pygame.K_SPACE in keystates_old or not keystates_old[pygame.K_SPACE]) and (pygame.K_SPACE in keystates and keystates[pygame.K_SPACE]):
		if simulationRunning:
			manualControl = not manualControl
		else:
			rocket = RocketPhysics()
			controller = RocketController2(rocket)

			rocket.resetRandom()

			simulationRunning = True
			fillColor = WHITE


	if simulationRunning:
		landingStatus = rocket.landingStatus()
		if landingStatus != None:
			simulationRunning = False
			fillColor = GREEN if landingStatus else RED



		if manualControl:
			rocket.throttle = 1 if pygame.K_UP in keystates and keystates[pygame.K_UP] else 0
			rocket.gimbal = 0
			if pygame.K_LEFT in keystates and keystates[pygame.K_LEFT]: rocket.gimbal = -1
			if pygame.K_RIGHT in keystates and keystates[pygame.K_RIGHT]: rocket.gimbal = 1
		else:
			controller.setControls()

		rocket.timestep(1.0/60)

	# draw
	screen.fill(fillColor)
	draw_rocket(rocket)

	# fps counter
	fps_count+=1
	if fps_count > 60:
		fps = fps_count/(1.0*time.time()-fps_start)
		print 'FPS: ', fps
		fps_count = 0
		fps_start = time.time()

	pygame.display.flip()
pygame.quit()