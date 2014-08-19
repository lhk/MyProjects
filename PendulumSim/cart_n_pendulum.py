import pygame
from numpy import sin, cos, pi, array
from scipy.sparse import coo_matrix,csc_matrix
from scipy.sparse.linalg import spsolve
from numpy.linalg import solve
import numpy as np
import matplotlib.pyplot as plt
import scipy.integrate as integrate
import matplotlib.animation as animation

n = 3

fx = [0 for i in range(n+1)] # external forces
fy = [-9.81 for i in range(n+1)]
m = [.1 for i in range(n+1)] # masses
l = [.5 for i in range(n)] # bar length
dynamic_state = [0.001 for i in range(2*n+2)] # == [x, th_1, th_2, .. th_n, --- v, w_1, w_2, .. w_n]
column_indices = {}
column_indices['alpha'] = range(n)
column_indices['ax'] = [i+n for i in range(n+1)]
column_indices['ay'] = [i+n+n+1 for i in range(n+1)]
column_indices['T'] = [i+n+n+1+n+1 for i in range(n)]
dt = 0.005
m[0] = 1

def sign(x):
	if x>0: return 1
	elif x<0: return -1
	else: return 0

def derivs(dynamic_state,t,n,fx,fy,m,l):
	
	ds_dt = np.zeros_like(dynamic_state)

	ds_dt[:n+1] = dynamic_state[n+1:]
	omega = dynamic_state[n+2:]
	theta = dynamic_state[1:n+1]

	# build sparse linear system to solve for accelerations
	row_counter = 0
	IJV = []
	right_side = []

	# kinematics equations
	for i in range(n):
		IJV.append((row_counter, column_indices['ax'][i+1], 1))
		IJV.append((row_counter, column_indices['ax'][i], -1))
		IJV.append((row_counter, column_indices['alpha'][i], l[i]*cos(theta[i])))
		right_side.append(omega[i]**2*l[i]*sin(theta[i]))
		row_counter += 1

		IJV.append((row_counter, column_indices['ay'][i+1], 1))
		IJV.append((row_counter, column_indices['ay'][i], -1))
		IJV.append((row_counter, column_indices['alpha'][i], l[i]*sin(theta[i])))
		right_side.append(-omega[i]**2*l[i]*cos(theta[i]))
		row_counter += 1

	# kinetics equations
	IJV.append((row_counter, column_indices['ax'][0], m[0]))
	IJV.append((row_counter, column_indices['T'][0], sin(theta[0])))
	right_side.append(fx[0])
	row_counter += 1

	IJV.append((row_counter, column_indices['ay'][0], 1))
	right_side.append(0)
	row_counter += 1

	IJV.append((row_counter, column_indices['ax'][n], m[n]))
	IJV.append((row_counter, column_indices['T'][n-1], -sin(theta[n-1])))
	right_side.append(fx[n])
	row_counter += 1

	IJV.append((row_counter, column_indices['ay'][n], m[n]))
	IJV.append((row_counter, column_indices['T'][n-1], cos(theta[n-1])))
	right_side.append(fy[n])
	row_counter += 1

	for i in range(1,n):
		IJV.append((row_counter, column_indices['ax'][i], m[i]))
		IJV.append((row_counter, column_indices['T'][i-1], -sin(theta[i-1])))
		IJV.append((row_counter, column_indices['T'][i], sin(theta[i])))
		right_side.append(fx[i])
		row_counter += 1

		IJV.append((row_counter, column_indices['ay'][i], m[i]))
		IJV.append((row_counter, column_indices['T'][i-1], cos(theta[i-1])))
		IJV.append((row_counter, column_indices['T'][i], -cos(theta[i])))
		right_side.append(fy[i])
		row_counter += 1

	rows,cols,data = zip(*IJV)
	mass_matrix = csc_matrix(coo_matrix((data,(rows,cols)), shape=(4*n+2,4*n+2)))
	
	soln = spsolve(mass_matrix,np.array(right_side))

	ds_dt[n+1] = soln[column_indices['ax'][0]]
	for i in range(n):
		ds_dt[n+2+i] = soln[column_indices['alpha'][i]]-0.3*omega[i]

	return ds_dt




# Define some colors
BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

def draw_pendulum(dynamic_state):
	c=.2
	theta = dynamic_state[1:n+1]
	x = dynamic_state[0]
	p = [np.array([x,0])]
	for i in range(n):
		p.append(p[-1] + l[i]*np.array([-sin(theta[i]),cos(theta[i])]))

	for p1,p2 in zip(p[1:],p[:-1]):
		pygame.draw.line(screen, BLACK, transf(p1), transf(p2), 5)

	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x+c,0-c]), 5)
	pygame.draw.line(screen, BLACK, transf([x+c,0+c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x-c,0+c]), 5)
	pygame.draw.line(screen, BLACK, transf([x-c,0-c]), transf([x+c,0-c]), 5)

	#pygame.draw.line(screen, RED, transf(p2), transf(p3), 5)
	#pygame.draw.line(screen, RED, transf(p1), transf(p4), 5)


pygame.init()
size = [1200, 1000]
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
	fx[0] = F1


	dynamic_state = integrate.odeint(lambda p1,p2:derivs(p1,p2,n,fx,fy,m,l), dynamic_state, [0, dt])[1]

	screen.fill(WHITE)
	draw_pendulum(dynamic_state)

	pygame.display.flip()
	clock.tick(60)
pygame.quit()