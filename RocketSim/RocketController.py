from numpy import sin,cos,exp

def deadzone(x,a):
	if x < -a:
		return x+a
	elif x < a:
		return 0
	else:
		return x-a 

def step(x):
	return 1 if x>=0 else -1

def limit(x,a):
	return min(a,max(-a,x))

class RocketController:

	def __init__(self, rocket):
		self.rocket = rocket
		
	def setControls(self):
		r = self.rocket

		up = max(0,sin(r.pitch))
		down = max(0,-sin(r.pitch))
		side = cos(r.pitch)


		r.gimbal = -side   +r.omega   -0.005*up*(5*r.vx+deadzone(limit(r.x,200),30))   -2*down*step(side+9*r.omega)


		r.throttle = .5    -0.02*min(300,r.y)     -0.1*r.vy          +4*abs(r.omega)       +4*abs(side)    +3*down    +0.01*limit(abs(r.x),300)
		r.throttle =  max(0.2,r.throttle)