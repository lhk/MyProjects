from numpy import sin, cos, pi, array
from scipy.sparse import coo_matrix,csc_matrix
from scipy.sparse.linalg import spsolve
from numpy.linalg import solve
import numpy as np
import scipy.integrate as integrate


class nPendulum:
	def __init__(self, n):
		self.n = max(1,int(n))
		n = self.n

		self.fx = [0 for i in range(n+1)] # external forces
		self.fy = [-9.81 for i in range(n+1)]
		self.m = [.1 for i in range(n+1)] # masses
		self.l = [4.0/n for i in range(n)] # bar length
		self.dynamic_state = [0.001 for i in range(2*n+2)] # == [x, th_1, th_2, .. th_n, --- v, w_1, w_2, .. w_n]
		self.column_indices = {}
		self.column_indices['alpha'] = range(n)
		self.column_indices['ax'] = [i+n for i in range(n+1)]
		self.column_indices['ay'] = [i+n+n+1 for i in range(n+1)]
		self.column_indices['T'] = [i+n+n+1+n+1 for i in range(n)]
		self.m[0] = 1


	def derivs(self, dynamic_state,t,n,fx,fy,m,l):

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
			IJV.append((row_counter, self.column_indices['ax'][i+1], 1))
			IJV.append((row_counter, self.column_indices['ax'][i], -1))
			IJV.append((row_counter, self.column_indices['alpha'][i], l[i]*cos(theta[i])))
			right_side.append(omega[i]**2*l[i]*sin(theta[i]))
			row_counter += 1

			IJV.append((row_counter, self.column_indices['ay'][i+1], 1))
			IJV.append((row_counter, self.column_indices['ay'][i], -1))
			IJV.append((row_counter, self.column_indices['alpha'][i], l[i]*sin(theta[i])))
			right_side.append(-omega[i]**2*l[i]*cos(theta[i]))
			row_counter += 1

		# kinetics equations
		IJV.append((row_counter, self.column_indices['ax'][0], m[0]))
		IJV.append((row_counter, self.column_indices['T'][0], sin(theta[0])))
		right_side.append(fx[0])
		row_counter += 1

		IJV.append((row_counter, self.column_indices['ay'][0], 1))
		right_side.append(0)
		row_counter += 1

		IJV.append((row_counter, self.column_indices['ax'][n], m[n]))
		IJV.append((row_counter, self.column_indices['T'][n-1], -sin(theta[n-1])))
		right_side.append(fx[n])
		row_counter += 1

		IJV.append((row_counter, self.column_indices['ay'][n], m[n]))
		IJV.append((row_counter, self.column_indices['T'][n-1], cos(theta[n-1])))
		right_side.append(fy[n])
		row_counter += 1

		for i in range(1,n):
			IJV.append((row_counter, self.column_indices['ax'][i], m[i]))
			IJV.append((row_counter, self.column_indices['T'][i-1], -sin(theta[i-1])))
			IJV.append((row_counter, self.column_indices['T'][i], sin(theta[i])))
			right_side.append(fx[i])
			row_counter += 1

			IJV.append((row_counter, self.column_indices['ay'][i], m[i]))
			IJV.append((row_counter, self.column_indices['T'][i-1], cos(theta[i-1])))
			IJV.append((row_counter, self.column_indices['T'][i], -cos(theta[i])))
			right_side.append(fy[i])
			row_counter += 1

		rows,cols,data = zip(*IJV)
		mass_matrix = csc_matrix(coo_matrix((data,(rows,cols)), shape=(4*n+2,4*n+2)))
		
		soln = spsolve(mass_matrix,np.array(right_side))

		ds_dt[n+1] = soln[self.column_indices['ax'][0]]
		for i in range(n):
			ds_dt[n+2+i] = soln[self.column_indices['alpha'][i]]-0.03*omega[i]*abs(omega[i])

		return ds_dt

	def timestep(self, dt):
		self.dynamic_state = integrate.odeint(lambda p1,p2:self.derivs(p1,p2,self.n,self.fx,self.fy,self.m,self.l), self.dynamic_state, [0, dt], rtol=1e-4, atol=1e-4)[1]


	def getPoints(self):
		theta = self.dynamic_state[1:self.n+1]
		x = self.dynamic_state[0]
		p = [np.array([x,0])]
		for i in range(self.n):
			p.append(p[-1] + self.l[i]*np.array([-sin(theta[i]),cos(theta[i])]))

		return p