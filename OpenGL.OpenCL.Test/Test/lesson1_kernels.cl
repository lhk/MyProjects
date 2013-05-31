#pragma OPENCL EXTENSION cl_khr_byte_addressable_store : enable
__constant char hw[] = "Hello World\n";


__kernel void hello(__global char * out)
{
	size_t tid = get_global_id(0);
	int i = (int) tid;

	printf("%s\n", "this is a test string");

	/*out[0] = ByteToHexHi((char)i);
	out[1] = ByteToHexLo((char)i);*/

	/*
	out[0] = 'H';
	out[1] = 'a';
	out[2] = 'l';
	out[3] = 'l';
	out[4] = 'o';
	out[5] = 0;
	*/
	
}

