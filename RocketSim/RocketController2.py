from numpy import sin,cos,exp
from math import atan2,sqrt,pi

def zero_sqrt(x):
	if x <= 0: return 0
	return sqrt(x)

def sign(x):
	if x < 0: return -1
	if x > 0: return 1
	return 0

def sym_mod(x,a):
	x = x % (2*a)
	if x > a: return x - 2*a
	return x

def deadzone(x,a):
	if x < -a:
		return x+a
	elif x < a:
		return 0
	else:
		return x-a 

class RocketController2:

	def __init__(self, rocket):
		self.rocket = rocket
		
	def setControls(self):
		r = self.rocket


		x = deadzone(r.x,30)
		y = r.y-10
		ground_factor = max(0,min(1,1-y/200))

		# desired velocity
		vx = -.8*sign(x)*sqrt(abs(x))+.5*r.vx
		vy = -1.5*sqrt(abs(y))+200*ground_factor*(max(0,1-sin(r.pitch))+min(1,abs(r.vx)/100))
		vx = max(-100,min(100,vx))

		ax = (vx-r.vx)
		ay = max(0,(vy-r.vy) + 9.81)

		a_mag = sqrt(ax**2+ay**2)
		if a_mag > 20:
			ax -=0.5*(a_mag-20)

		target_attitude = pi/2
		if a_mag > 1e-8:
			target_attitude = atan2(ay,ax)

		r.throttle = .05 * a_mag #+ 0.5*abs(sym_mod(r.pitch-target_attitude,pi))
		r.gimbal = 0.8*sym_mod(r.pitch-target_attitude,pi) + .8*r.omega