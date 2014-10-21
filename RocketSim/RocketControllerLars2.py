from __future__ import division
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

def percentage(value, max):
	if(abs(value)>max):
		return sign(value)*1
	else:
		return value/max

def scale(value, target, fallof, max_value):
	p = percentage(target-value, fallof)
	return p*max_value

max_omega=0.5

class RocketControllerLars2:

	def __init__(self, rocket):
		self.rocket = rocket
		# this will be adjusted not calculated
		self.throttle=0.5
		
	def setControls(self):
		r = self.rocket
		vx=0
		vy=0

		max_x_speed=10
		fallof_x_dist=200

		tolerance_x_dist=0
		if(abs(r.x)<tolerance_x_dist):
			# rocket is more or less centered, begin landing
			vx=0
			vy=-5

		else:
			# move towards the center, maintain current height
			vx=scale(r.x, 0, fallof_x_dist, max_x_speed)

		target_pitch=pi/2
		fallof_x_speed=2
		max_delta_pitch=1
		
		delta_pitch=scale(r.vx, vx, fallof_x_speed, max_delta_pitch)

		target_pitch=target_pitch-delta_pitch

		if(vy<r.vy):
			self.throttle-=0.2
		elif(vy>r.vy):
			self.throttle+=0.2

		if(abs(r.omega)>max_omega):
			self.throttle=1


		if(self.throttle<0):
			self.throttle=0
		elif(self.throttle>1):
			self.throttle=1

		print("target_pitch",target_pitch)
		print("rocket pitch", r.pitch)
		print("vx", vx)
		print("rocket vx", r.vx)
		print("vy", vy)
		print("rocket vy", r.vy)
		print("throttle", self.throttle)

		gimbal=self.calculateGimbal(target_pitch)
		r.gimbal=gimbal
		r.throttle=self.throttle



	'''calculate the gimbal to get to the desired target pitch'''
	def calculateGimbal(self, target_pitch):
		r=self.rocket
		pitch=sym_mod(r.pitch,pi)
		max_gimbal=3.1415/10

		fallof_pitch=0.3

		if(pitch>3/2*pi):
			pitch=pitch-2*pi

		target_omega=scale(r.pitch, target_pitch, fallof_pitch, max_omega)


		#percentage=(target_pitch-pitch)/tolerance_pitch
		#target_omega=sign(percentage)*min(abs(percentage),1)*max_omega
		
		if(r.omega-target_omega>0):
			return max_gimbal
		elif(r.omega-target_omega<0):
			return -max_gimbal
		else:
			return 0