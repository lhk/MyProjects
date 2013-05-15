sub EndpointDecode
{
	my ($port, $ip) = unpack_sockaddr_in(shift);
	$ip = inet_ntoa($ip);
	return "$ip:$port";
}

sub EndpointEncode
{
	my $ip_port = shift;
	my($ip, $port) = split(':',$ip_port);
	return pack_sockaddr_in($port, inet_aton($ip));
}

sub GetIP
{
	my $ip;
	$ip = gethostbyname(hostname());
	if(length($ip) == 4) {return $ip;}
	
	$ip = inet_aton(`ipconfig getifaddr en0`);
	if(length($ip) == 4) {return $ip;}
	
	die('could not find device\'s IP');
}

sub GetLocalEndpoint
{
	my $port = shift;
	my $ip = GetIP();
	return inet_ntoa($ip) . ":" . $port;
}

sub GetBroadcastEndpoint
{
	my $port = shift;
	my $ip = GetIP();
	my @ip_arr = split('', $ip);
	$ip_arr[3] = chr(255);
	$ip = join('', @ip_arr);
	return inet_ntoa($ip) . ":" . $port;
}



1;