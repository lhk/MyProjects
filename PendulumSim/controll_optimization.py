import numpy as np
from numpy import array
from nPendulum import nPendulum
from multiprocessing import Pool

def simulate(controll_params):
	p = nPendulum(2)
	y=4
	score = 0
	while y > 2.1 and score < 10000:
		p.fx[0] = np.dot(p.dynamic_state[1:],controll_params)
		p.timestep(0.005)
		y = p.getPoints()[-1][1]
		score += 1
	return (score,controll_params)


def simulate2(controll_params):
	p = nPendulum(2)
	p.dynamic_state[0]=3
	p.dynamic_state[1]=-0.02
	p.dynamic_state[2]=0.02
	score = 0
	for i in range(800):
		p.fx[0] = np.dot(p.dynamic_state[1:],controll_params)
		p.timestep(0.005)
		score += np.sum(np.abs(p.dynamic_state[1:]))
	return (score,controll_params)


proc_pool = Pool(processes=8,)


pool = [(2185.4832133687346, array([-603.86411394,  506.9427075 ,   -2.12051099,  -51.31956082,         95.11927173]))]


while True:
	print('starting epoch')
	jobs=[]
	for i in range(len(pool)):
		for j in range(4):
			params = pool[i][1] + (np.random.rand(5)*2-1)*0.2
			jobs.append(params)

	pool += proc_pool.map(simulate2, jobs)

	pool.sort(key=lambda x:x[0])
	pool = pool[:1]
	for l in pool:
		print l
