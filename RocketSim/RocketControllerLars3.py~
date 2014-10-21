from __future__ import division
from numpy import sin,cos,exp
from math import atan2,sqrt,pi,pow

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

def capped_percentage(value, max):
	if(abs(value)>max):
		return sign(value)*1
	else:
		return value/max

def percentage(value,max):
	return value/max

def scale(value, target, fallof, max_value):
	p = capped_percentage(target-value, fallof)
	return p*max_value

def scale_with(value, target, fallof, max_value, func):
	p=percentage(value, fallof)
	res=func(p)*max_value
	return res

def polynom(p):
	if(abs(p)>1):
		return sign(p)
	return pow(p,3)

# speed scales with the distance from the center
max_x_speed=-30
fallof_x_distance=200

# pitch scales with the distance from the target speed
max_pitch=0.4
fallof_pitch=20

# omega scales with the distance from the target pitch
max_omega=1
fallof_omega=0.3

# gimbal scales with the distance from the target omega
max_gimbal=3.1415/10
fallof_gimbal=1/10

# y speed scales with the distance from the ground
max_y_speed=5
descend_y_speed=-5
ascend_y_speed=5
panic_y_speed=-11
panic_y_height=30
fallof_y_speed=2
tolerance_x_dist=20

glide_height=100

class RocketControllerLars3:

	def __init__(self, rocket):
		self.rocket = rocket
		self.throttle=0.5
		
	def setControls(self):
		r = self.rocket
		pitch=sym_mod(r.pitch,pi)

		# calculate the correct pitch in 0..2*pi
		if(pitch<0):
			pitch=2*pi+pitch
	
		if(pitch>3/2*pi):
			pitch-=2*pi

		# calculate the desired values for ...
		vx=scale_with(r.x, 0, fallof_x_distance, max_x_speed,polynom)

		target_pitch=pi/2 - scale(r.vx, vx, fallof_pitch, max_pitch)
		
		#this is a panic behaviour: if the rocket is falling too fast, we try to get it upright first
		if(r.vy<panic_y_speed):
			target_pitch=pi/2

		target_omega=scale(pitch, target_pitch, fallof_omega, max_omega)

		gimbal=scale(r.omega, target_omega,fallof_gimbal, max_gimbal)

		r.gimbal=-gimbal
		
		vy=0

		if(abs(r.x)>tolerance_x_dist):
			if(r.y>glide_height):
				vy=-max_y_speed
			else:
				if(r.y<panic_y_height):
					self.throttle=1
					vy=max_y_speed
				else:
					vy=ascend_y_speed
		else:
			vy=descend_y_speed
		
		if(r.vy>vy):
			self.throttle-=0.1
		elif(r.vy<vy):
			self.throttle+=0.1
		
		if(r.vy>0):
			self.throttle=max(0.3,self.throttle)

		if(abs(r.omega)>max_omega):
			self.throttle=1
		
		if(r.vy<panic_y_speed):
			self.throttle=1

		if(self.throttle<0):
			self.throttle=0
		elif(self.throttle>1):
			self.throttle=1

		r.throttle=self.throttle

		#print("target_pitch",target_pitch)
		#print("rocket pitch", r.pitch)
		#print("vx", vx)
		#print("rocket vx", r.vx)
		#print("vy", vy)
		#print("rocket vy", r.vy)
		#print("throttle", self.throttle)
