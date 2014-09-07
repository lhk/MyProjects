from numpy import sin,cos
import numpy as np
from numpy.linalg import norm
from numpy.random import rand

class RocketPhysics(object):
	def __init__(self):
		self.trajectory = []
		self.x = 0.0
		self.y = 40.0
		self.vx = 0.0
		self.vy = 0.0
		self.core_radius = 2.0
		self.height = 31.0
		self.throttle = 0.2
		self.pitch = 3.1415/2
		self.omega = 0
		self.gimbal = 0
		self.mass = 5.0e6
		self.g = 9.81
		self.max_thrust = 1.0e8

	def enforceRanges(self):
		self.throttle = min(1,max(0,self.throttle))
		self.gimbal = min(3.1415/10,max(-3.1415/10,self.gimbal))

	def resetRandom(self):		
		self.trajectory = []
		self.x = (rand()*2-1)*200
		self.y = rand()*120+150
		self.vx = (rand()*2-1)*10
		self.vy = (rand()*2-1)*10
		self.omega = (rand()*2-1)*0.8
		self.pitch = (rand()*2-1)*20

	def timestep(self, dt):
		self.enforceRanges()
		thrust_angle = self.pitch + self.gimbal
		self.vx += dt * self.max_thrust * self.throttle * cos(thrust_angle) / self.mass
		self.vy += dt * (self.max_thrust * self.throttle * sin(thrust_angle) / self.mass - self.g)
		self.omega -= dt * self.max_thrust * self.throttle * 6 * sin(self.gimbal) / self.mass / self.height

		self.x += dt * self.vx
		self.y += dt * self.vy
		self.pitch += dt * self.omega

		if(len(self.trajectory)==0 or norm(np.array(self.trajectory[-1])-np.array([self.x, self.y]))>6 ):
			self.trajectory.append((self.x, self.y))

		if len(self.trajectory)>150:
			del self.trajectory[0]

	# returns None, False or True
	# None -> still flying
	# True -> landed
	# False -> crashed
	def landingStatus(self):
		e_f = np.array([cos(self.pitch),sin(self.pitch)])
		e_l = np.array([-sin(self.pitch),cos(self.pitch)])
		cg = np.array([self.x,self.y])
		top = cg + self.height/2* e_f
		bottom = cg - self.height/2* e_f

		hullVertices = [top,
		top-(e_f-e_l)*self.core_radius,
		top-(e_f+e_l)*self.core_radius,
		bottom+e_l*self.core_radius,
		bottom-e_l*self.core_radius,
		bottom-(4*e_l+2*e_f)*self.core_radius,
		bottom-(e_l-2*e_f)*self.core_radius,
		bottom+(e_l+2*e_f)*self.core_radius,
		bottom-(-4*e_l+2*e_f)*self.core_radius]

		touchdown = any(v[1] <= 0 for v in hullVertices)
		if not touchdown: return None

		#print self.x,' -- ',abs(self.vx),' -- ',abs(self.vy),' -- ',abs(self.omega),' -- ',sin(self.pitch)

		print 'landing score: ',abs(self.vx)/6 + abs(self.vy)/12 + abs(self.omega)/0.2 + 100*(1-sin(self.pitch))

		return abs(self.x)<50 and abs(self.vx)<6 and abs(self.vy)<12 and abs(self.omega)<0.2 and sin(self.pitch)>0.99
	
