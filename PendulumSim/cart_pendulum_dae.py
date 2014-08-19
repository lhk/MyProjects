import pygame
from numpy import sin, cos, pi, array
from numpy.linalg import solve
import numpy as np
import scipy.optimize
import matplotlib.pyplot as plt
import scipy.integrate as integrate
import sympy as sp
from sympy.utilities.lambdify import lambdify, implemented_function
#from sympy.matrices import jacobian


#state = [x1, dx1, x2, dx2, y1, dy1, y2, dy2, f]
#state = [1 1 0 0 f]
state = {'x1': 0.999, 'dx1': .0, 'x2': .0, 'dx2': .0, 'y1': 0.001, 'dy1': .0, 'y2': .0, 'dy2': .0, 'f': .0}
kin_soln = [1,1,1,1]
dt = 0.001
L = 1.0
G =	10.0
M1 = 1.0
M2 = 10.0


def setup_equations():
	symbols = sp.S('x1 dx1 ddx1 x2 dx2 ddx2 y1 dy1 ddy1 y2 dy2 ddy2 T f'.split())
	x1, dx1, ddx1, x2, dx2, ddx2, y1, dy1, ddy1, y2, dy2, ddy2, T, f = symbols
	F = sp.Matrix([
		M1*ddx1-(x2-x1)*T/L, 
		M1*ddy1-(y2-y1)*T/L+M1*G, 
		M2*ddx2-(x1-x2)*T/L-f, 
		(dx2-dx1)**2+(x2-x1)*(ddx2-ddx1) + (dy2-dy1)**2+(y2-y1)*(ddy2-ddy1)])

	JF = F.jacobian([ddx1,ddy1,ddx2,T])
	l_F = lambdify(symbols, F)
	l_DF = lambdify(symbols, JF)

	ll_F = lambda x,state: l_F(state['x1'], state['dx1'], x[0], state['x2'], state['dx2'], x[2], state['y1'], state['dy1'], x[1], state['y2'], state['dy2'], 0, x[3], state['f']).T.tolist()[0]
	ll_DF = lambda x,state: l_DF(state['x1'], state['dx1'], x[0], state['x2'], state['dx2'], x[2], state['y1'], state['dy1'], x[1], state['y2'], state['dy2'], 0, x[3], state['f'])
	return ll_F,ll_DF

kin_sys,D_kin_sys = setup_equations()


#print kin_soln, kin_sys(kin_soln,state)
#kin_soln = scipy.optimize.fsolve(lambda x:kin_sys(x,state), kin_soln, fprime=lambda x:D_kin_sys(x,state))
#print kin_soln, kin_sys(kin_soln,state)


def derivs(ode_state, t, state):
	global kin_soln
	dydx = ode_state[1:]+[0]
	kin_soln = scipy.optimize.fsolve(lambda x:kin_sys(x,state), kin_soln, fprime=lambda x:D_kin_sys(x,state))
	dydx[1] = kin_soln[0]
	dydx[3] = kin_soln[2]
	dydx[5] = kin_soln[1]
	return dydx 


# Define some colors
BLACK	= (   0,   0,   0)
WHITE	= ( 255, 255, 255)
GREEN	= (   0, 255,   0)
RED	  = ( 255,   0,   0)

def draw_pendulum(state):
	pygame.draw.line(screen, BLACK, transf([state['x1'],state['y1']]), transf([state['x2'],state['y2']]), 5)

def transf(v):
	M = [
		[100,   0, 600],
		[0  , -100, 300],
		[0  ,   0,   1]
		]
	v.append(1)
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
				state['f'] = -100
			elif event.key == pygame.K_RIGHT:
				state['f'] = 100
		
		elif event.type == pygame.KEYUP:
			if event.key == pygame.K_LEFT:
				state['f']=0
			elif event.key == pygame.K_RIGHT:
				state['f']=0		  


	state['x1'],state['dx1'],state['x2'],state['dx2'],state['y1'],state['dy1'],state['y2'],state['dy2'] = integrate.odeint(
		lambda s,t:derivs(s,t,state), [state['x1'],state['dx1'],state['x2'],state['dx2'],state['y1'],state['dy1'],state['y2'],state['dy2']], [0, dt],
		rtol=1e-6,atol=1e-6
		)[1]

	dist = np.sqrt((state['x2']-state['x1'])**2+(state['y2']-state['y1'])**2)
	state['x1'] = state['x2'] + (state['x1']-state['x2'])*L/dist
	state['y1'] = state['y2'] + (state['y1']-state['y2'])*L/dist

	screen.fill(WHITE)
	draw_pendulum(state)

	pygame.display.flip()
	clock.tick(60)
pygame.quit()