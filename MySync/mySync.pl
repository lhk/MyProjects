use IO::Socket;
use strict;
use Sys::Hostname;
use IO::Select;
use Data::Dumper;
require "inetMacros.pl";
require "config.pl";

my $localPort = 43816;


# flush after every write
$| = 1;

my $sock = IO::Socket::INET->new(
    Proto    => 'udp',
    #LocalAddr => gethostbyname(hostname()),
	LocalPort => $localPort,
	Type => SOCK_DGRAM,
	Broadcast => 1
) or die "Could not create socket: $!\n";
$sock->bind($localPort);
my $select = IO::Select->new($sock);



my $BroadcastEndpoint = GetBroadcastEndpoint($localPort);
my $LocalEndpoint = GetLocalEndpoint($localPort);
my $LastBroadcast = 0;
my $Peers = {};

while (1)
{
	print "Polling...\n";
	my @read_handles = $select->can_read(0.001);
	my $dataReceived = 0;
	
	for my $read_handle (@read_handles)
	{
		my $received_data = "";
		my $SenderEndpoint = EndpointDecode($read_handle->recv($received_data, 4096, 0) or die "recv: $!");
		if($SenderEndpoint ne $LocalEndpoint)
		{
			if(length($received_data) > 0) {$dataReceived = 1;}
			
			$Peers->{$SenderEndpoint}->{lastContact} = time();
			print "($SenderEndpoint): " . $received_data . "\n";
			#print Dumper($Peers)."\n";
		}
	}
	
	if($dataReceived == 0) {sleep 1;}
	
	if($LastBroadcast + 10 < time())
	{
		$sock->send("Anyone there?", 0, EndpointEncode($BroadcastEndpoint)) or die "send: $!";
		print "Broadcast sent.\n";
		$LastBroadcast = time();
	}
}