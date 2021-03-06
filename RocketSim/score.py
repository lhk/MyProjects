from RocketPhysics import RocketPhysics
from RocketController import RocketController
from RocketController2 import RocketController2
from RocketControllerLars3 import RocketControllerLars3

rocket = RocketPhysics()

controllers = [('RocketController2',RocketController2),('RocketControllerLars3',RocketControllerLars3)]

for name,clas in controllers:
	print '\n'+name+':'
	score, crashed, timeout = rocket.controllerScore(clas)
	if crashed: print 'crashed'
	elif timeout: print 'simulation time exceeded'
	else: print score
