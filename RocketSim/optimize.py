from numpy import sin,cos
import pygame
import numpy as np
import time
import sys
import os
import ast
from multiprocessing import Pool
from rocketPhysics import rocketPhysics



def eval_score(params):
	rockets = []
	x = [-1, 1]

	for i in x:
		for j in x:
			for k in x:
				for l in x:
					rocket = rocketPhysics()
					rocket.x = 10.0
					rocket.y = 40.0+i*10.0
					rocket.vx = 0.2*j
					rocket.vy = 0.2*k
					rocket.omega = 0.2*l
					rockets.append(rocket)

	score = 0
	for rocket in rockets:
		for i in range(150):
			features = [rocket.x,rocket.y,rocket.vx,rocket.vy,sin(rocket.pitch),cos(rocket.pitch),sin(rocket.omega),cos(rocket.omega)]
			rocket.throttle = np.dot(features,params[0:8]) + params[8]
			rocket.gimbal = np.dot(features,params[9:17]) + params[17]
			rocket.timestep(1.0/30)

			score += abs(rocket.x)
			score += abs(rocket.y-40)
			score += 100*abs(rocket.vx)
			score += 100*abs(rocket.vy)
			score += 100*abs(rocket.omega)
	return score

params = [0.0001 for i in range(18)]
if os.path.isfile('params'):
	with open('params','rb') as f:
		params = ast.literal_eval(f.read())

params = np.array(params)
oldScore = eval_score(params)
proc_pool = Pool(processes=4,)

while True:
	newParams = [params + (np.random.rand(18)*2-1)*0.004 for i in range(4)]
	newScores = proc_pool.map(eval_score, newParams)
	for i in range(4):
		if newScores[i] < oldScore:
			oldScore = newScores[i]
			params = newParams[i]
			print oldScore
			#print params
			with open('params','wb') as f:
				print >>f, params.tolist()