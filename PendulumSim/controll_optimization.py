import numpy as np
from numpy import array
from nPendulum import nPendulum
from multiprocessing import Pool
import cPickle as pickle



def simulate(arg):
	controll_params = arg	
	score = 0
	p = nPendulum(2)
	p.dynamic_state[0]=3
	p.dynamic_state[1]=-0.02
	p.dynamic_state[2]=0.02
	for i in range(300):
		p.fx[0] = np.dot(p.dynamic_state[1:],controll_params)
		p.timestep(0.005)
		score += np.sum(np.abs(p.dynamic_state[1:]))

	p = nPendulum(2)
	p.dynamic_state[0]=3
	p.dynamic_state[1]=0.02
	p.dynamic_state[2]=0.04
	for i in range(300):
		p.fx[0] = np.dot(p.dynamic_state[1:],controll_params)
		p.timestep(0.005)
		score += np.sum(np.abs(p.dynamic_state[1:]))
	return [score,controll_params]


proc_pool = Pool(processes=4,)



with open('params', 'rb') as fp:
	pool = [pickle.load(fp)]
for i in range(len(pool)):
	pool[i][0] = 1.0e20

while True:
	print('starting epoch')
	jobs=[]
	for i in range(len(pool)):
		for j in range(4):
			delta = (np.random.rand(5)*2-1)*0.05
			params = pool[i][1] + delta
			jobs.append(params)

	pool += proc_pool.map(simulate, jobs)

	pool.sort(key=lambda x:x[0])
	pool = pool[:1]
	for l in pool:
		print l

	with open('params', 'wb') as fp:
		pickle.dump(pool[0], fp)
