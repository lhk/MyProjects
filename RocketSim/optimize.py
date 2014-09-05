from numpy import sin,cos
import pygame
import numpy as np
import time
import sys
import os
import ast
from multiprocessing import Pool
from RocketPhysics import RocketPhysics


def eval_score(params):
	rockets = []
	x = [-1, 1]

	for i in x:
		for j in x:
			for k in x:
				for l in x:
					rocket = RocketPhysics()
					rocket.x = 10.0
					rocket.y = 40.0+i*10.0
					rocket.vx = 0.2*j
					rocket.vy = 0.2*k
					rocket.omega = 0.2*l
					rockets.append(rocket)

	score = 0
	for rocket in rockets:
		for i in range(400):
			features = [rocket.x,rocket.y,rocket.vx,rocket.vy,cos(rocket.pitch),rocket.omega,1]
			rocket.throttle = np.dot(features,params[0:7])
			rocket.gimbal = np.dot(features,params[7:14])
			rocket.timestep(1.0/20)

			#score += abs(rocket.x)
			#score += abs(rocket.y-40)
			#score += 100*abs(rocket.vx)
			score += 0.1*abs(rocket.vy-1)**2
			#score += 100*abs(rocket.omega)
			if sin(rocket.pitch)>0:
				score += abs(cos(rocket.pitch))**2
			else: score += 2
			if rocket.y<0: score+=0.5
	return score

params = [0.00001 for i in range(14)]
if os.path.isfile('params'):
	with open('params','rb') as f:
		params = ast.literal_eval(f.read())

params = np.array(params)
oldScore = eval_score(params)
proc_pool = Pool(processes=4,)

while True:
	newParams = [params + (np.random.rand(14)*2-1)*0.001 for i in range(4)]
	newScores = proc_pool.map(eval_score, newParams)
	for i in range(4):
		if newScores[i] < oldScore:
			oldScore = newScores[i]
			params = newParams[i]
			print oldScore
			#print params
			with open('params','wb') as f:
				print >>f, params.tolist()