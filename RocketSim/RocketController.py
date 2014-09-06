from numpy import sin,cos,exp

def deadzone(x,a):
	if x < -a:
		return x+a
	elif x < a:
		return 0
	else:
		return x-a 

class RocketController:

	def __init__(self, rocket):
		self.rocket = rocket
		
	def setControls(self):
		r = self.rocket

		# if the nose is low, full throttle to give attitude authority
		if sin(r.pitch)<0.7:
			r.throttle = 1
		else:
			r.throttle = max(0.2,-0.02*r.y-0.1*r.vy+0.5  +10*exp(-(r.y*0.02)**2)*abs(cos(r.pitch)))

		# pitch the nose vertical and reduce rotational velocity
		r.gimbal = -min(0.5,max(-0.5,cos(r.pitch)))+r.omega

		# if the nose is above the horizon, steer towards the landing platform (x=0)
		if sin(r.pitch)>0:
			r.gimbal -= 0.005*sin(r.pitch)*(5*r.vx+deadzone(r.x,30))