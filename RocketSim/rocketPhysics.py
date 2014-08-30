from numpy import sin,cos

class rocketPhysics(object):
	def __init__(self	):
		self.x = 0.0
		self.y = 40.0
		self.vx = 0.0
		self.vy = 0.0
		self.height = 22.0
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

	def timestep(self, dt):
		self.enforceRanges()
		thrust_angle = self.pitch + self.gimbal
		self.vx += dt * self.max_thrust * self.throttle * cos(thrust_angle) / self.mass
		self.vy += dt * (self.max_thrust * self.throttle * sin(thrust_angle) / self.mass - self.g)
		self.omega -= dt * self.max_thrust * self.throttle * 6 * sin(self.gimbal) / self.mass / self.height

		self.x += dt * self.vx
		self.y += dt * self.vy
		self.pitch += dt * self.omega
