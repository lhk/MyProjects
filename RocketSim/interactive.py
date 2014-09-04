from numpy import sin,cos
import pygame
import numpy as np
import time
import sys
import os
import ast
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

	pygame.draw.line(screen, BLACK, transf([-1000,0]), transf([1000,0]), 2)
	for i in range(-20,20):
		c = 5
		pygame.draw.line(screen, BLACK, transf([i*c,-c]), transf([i*c+c,0]), 2)



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
		[3,   0, size[0]/2],
		[0  ,-3, size[1]*5/6],
		[0  ,   0,   1]
		]
	v=np.append(v,1)
	return (np.matrix(M)*np.matrix(v).T).T.tolist()[0][:-1]

keystates = {}
fps = 60

if os.path.isfile('params'):
	with open('params','rb') as f:
		params = ast.literal_eval(f.read())

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
	

	features = [rocket.x,rocket.y,rocket.vx,rocket.vy,sin(rocket.pitch),cos(rocket.pitch),sin(rocket.omega),cos(rocket.omega)]
	rocket.throttle = np.dot(features,params[0:8]) + params[8]
	rocket.gimbal = np.dot(features,params[9:17]) + params[17]

	rocket.timestep(1.0/max(60,fps))

	screen.fill(WHITE)
	draw_rocket(rocket)

	fps_count+=1
	if fps_count > 60:
		fps = fps_count/(1.0*time.time()-fps_start)
		print 'FPS: ', fps
		fps_count = 0
		fps_start = time.time()

	pygame.display.flip()
pygame.quit()