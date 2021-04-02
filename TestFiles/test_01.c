changeArray(int arr[]; int b)
{
	arr[5] = b;
	return;
}

int main()
{
	int a[10] : 10;
	
	print(a[5]);
	changeArray(a, 49494);
	print(a[5]);

	return 0;
}