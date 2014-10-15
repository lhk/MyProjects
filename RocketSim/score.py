from RocketPhysics import RocketPhysics
from RocketController import RocketController
from RocketController2 import RocketController2
from RocketControllerLars import RocketControllerLars

rocket = RocketPhysics()

controllers = [('RocketController2',RocketController2),('RocketControllerLars',RocketControllerLars)]

for name,clas in controllers:
	print '\n'+name+':'
	score, crashed, timeout = rocket.controllerScore(clas)
	if crashed: print 'crashed'
	elif timeout: print 'simulation time exceeded'
	else: print score