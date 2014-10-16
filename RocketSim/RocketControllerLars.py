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

class RocketControllerLars:

	def __init__(self, rocket):
		self.rocket = rocket

		# this will be adjusted not calculated
		self.throttle=0.5
		
	def setControls(self):
		r = self.rocket

		tolerance_x_dist=5

		fallof_speed=20
		max_x_speed=10
		vx=0
		vy=0

		if(abs(r.x)<tolerance_x_dist):
			# rocket is more or less centered, begin landing
			vx=0
			vy=-5

			target_pitch=pi/2
			gimbal=self.calculateGimbal(target_pitch)
			r.gimbal=gimbal

		else:
			# move towards the center, maintain current height
			vx=-(r.x)/fallof_speed*max_x_speed
			vx=sign(vx)*min(abs(vx),max_x_speed)
			if(r.y>10):
				vy=-5

		target_pitch=pi/2
		tolerance_speed=1
		factor_pitch=1
		
		temp=(vx-r.vx)/tolerance_speed
		temp=sign(temp)*min(abs(temp),1)
		print(temp)
		target_pitch=target_pitch-temp*factor_pitch


		if(vy<r.vy):
			self.throttle=self.throttle*1.2
		elif(vy>r.vy):
			self.throttle=self.throttle*0.8

		if(self.throttle<0):
			self.throttle=0
		elif(self.throttle>1):
			self.throttle=1

		gimbal=self.calculateGimbal(target_pitch)
		r.gimbal=gimbal
		r.throttle=self.throttle



	'''calculate the gimbal to get to the desired target pitch'''
	def calculateGimbal(self, target_pitch):
		r=self.rocket

		max_gimbal=3.1415/10
		tolerance_pitch=pi/6

		target_omega=0
		max_omega=1
		tolerance_omega=0.05

		percentage=0
		pitch=r.pitch

		if(pitch>3/2*pi):
			pitch=pitch-2*pi

		percentage=(target_pitch-pitch)/tolerance_pitch
		target_omega=sign(percentage)*min(abs(percentage),1)*max_omega
		
		if(r.omega-target_omega>tolerance_omega):
			return max_gimbal
		elif(r.omega-target_omega<-tolerance_omega):
			return -max_gimbal
		else:
			return 0