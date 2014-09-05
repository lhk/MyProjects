from Serializable import Serializable
from numpy import sin,cos

def deadzone(x,a):
	if x < -a:
		return x+a
	elif x < a:
		return 0
	else:
		return x-a 

class RocketController(Serializable):

	def __init__(self, rocket):
		self.rocket = rocket
		
	def setControls(self):
		r = self.rocket
		if sin(r.pitch)<0.7:
			r.throttle = 1
		else:
			r.throttle = -0.01*r.y-0.1*r.vy+0.5
		r.gimbal = -cos(r.pitch)+r.omega

		if sin(r.pitch)>0:
			r.gimbal -= 0.005*sin(r.pitch)*(5*r.vx+deadzone(r.x,30))