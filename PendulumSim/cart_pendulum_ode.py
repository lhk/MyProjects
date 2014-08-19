import pygame
from numpy import sin, cos, pi, array
from numpy.linalg import solve
import numpy as np
import matplotlib.pyplot as plt
import scipy.integrate as integrate
import matplotlib.animation as animation

state = [3.5,0,0.5,0,0] # x, v, th, w, int_th
F1 = 0
F2 = 0
dt = 0.01
L = 1.5

def sign(x):
	if x>0: return 1
	elif x<0: return -1
	else: return 0

def derivs(state, t,F1,F2):
	G =	9.81
	M1 = 3.0
	M2 = 10.0
	dydx = np.zeros_like(state)
	x, dx, th, w, int_th = state
	dydx[0] = state[1]
	dydx[2] = state[3]
	dydx[4] = state[2]

	soln = solve([
		[M1,0,0,0,-sin(th),0],
		[0,M1,0,0,cos(th),0],
		[0,0,M2,0,sin(th),0],
		[0,0,0,0,-cos(th),-1],
		[1,0,-1,L*cos(th),0,0],
		[0,1,0,sin(th)*L,0,0],
		],
		[F1,-M1*G,F2,-M2*G,w**2*L*sin(th),-w**2*L*cos(th)])

	dydx[1]=soln[2] # ddx / ax
	dydx[3]=soln[3] # alpha

	return dydx 


# Define some colors
BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

def draw_pendulum(state):
	x, dx, th, dth, int_th = state
	c=.2
	p1 = np.array([x,0])
	p2 = p1 + np.array([-L*sin(th), L*cos(th)])
	p4 = p1 + np.array([F2*0.01,0])
	p3 = p2 + np.array([0.01*F1,0])
	pygame.draw.line(screen, BLACK, transf(p1), transf(p2), 5)
	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x+c,0-c]), 5)
	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x+c,0-c]), 5)

	pygame.draw.line(screen, RED, transf(p2), transf(p3), 5)
	pygame.draw.line(screen, RED, transf(p1), transf(p4), 5)
def transf(v):
	M = [
		[100,   0, 600],
		[0  ,-100, 300],
		[0  ,   0,   1]
		]
	v=np.append(v,1)
	return (np.matrix(M)*np.matrix(v).T).T.tolist()[0][:-1]

pygame.init()
size = [1200, 600]
screen = pygame.display.set_mode(size)
pygame.display.set_caption("My Game")
done = False
clock = pygame.time.Clock()
pygame.mouse.set_visible(0)

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

	# PID
	state[2] = state[2] % (2*pi)
	if state[2] >= pi: state[2]-=2*pi
	err = state[2]
	if abs(err) < 1:
		d_err = state[3]
		int_err = state[4]
		F2 = -800*err-300*d_err+80*state[1]
	else:
		F2=0

	state = integrate.odeint(lambda s,t:derivs(s,t,F1,F2), state, [0, dt])[1]

	screen.fill(WHITE)
	draw_pendulum(state)

	pygame.display.flip()
	clock.tick(60)
pygame.quit()